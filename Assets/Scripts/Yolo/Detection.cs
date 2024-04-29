using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Data;

using UnityEngine;
using Unity.Barracuda;
using TMPro;

using HoloLensCameraStream;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using Unity.VisualScripting;


#if WINDOWS_UWP && XR_PLUGIN_OPENXR
using Windows.Perception.Spatial;
#endif

#if WINDOWS_UWP
using Windows.UI.Input.Spatial;
#endif


public class Detection : MonoBehaviour
{
    public NNModel model;
    public int cameraResolutionWidth = 424;
    public int cameraResolutionHeight = 240;
    public int inferenceImgSize = 160;
    public float confidenceThreshold = 0.2f;
    public bool isDetecting = true;
    public DetectionType detectionType = DetectionType.Yolo;
    public Material laserMaterial;
    public GameObject labelPrefab;
    public TMP_Text detectionTypeTMP;
    public TMP_Text fpsTMP;
    public TMP_Text debugText;


    [SerializeField]
    private string baseUrl = "http://192.168.3.40:5000";
    private Hl2ServerApi webApi;
    private Model runtimeModel;
    private IWorker worker;
    private HoloLensCameraStream.Resolution resolution;
    private VideoCapture videoCapture;
    private List<DetectionResult> boxes;
    private List<DetectionResult> boxes_tmp;
    private List<Tuple<GameObject, Renderer, GameObject>> labels;
    private SpeakDetectionResult speaker;
    private COCONames names;
    private int numClasses;
    private COCOColors colors;
    private Texture2D pictureTexture;
    //private Texture2D scaledTexture;
    private Texture2D croppedTexture;
    private string textToRead;
    private RaycastLaser laser;
    private IntPtr _spatialCoordinateSystemPtr;
    Matrix4x4 camera2WorldMatrix;
    Matrix4x4 projectionMatrix;

    Matrix4x4 camera2WorldMatrix_local;
    Matrix4x4 projectionMatrix_local;

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
    SpatialCoordinateSystem _spatialCoordinateSystem;
#endif

    private byte[] _latestImageBytes;
    private bool stopVideo;
    
    private class SampleStruct
    {
        public float[] camera2WorldMatrix, projectionMatrix;
        public byte[] data;
    }

    void Start()
    {
#if WINDOWS_UWP


#if XR_PLUGIN_OPENXR

        _spatialCoordinateSystem = Microsoft.MixedReality.OpenXR.PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;

#endif
#endif
        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);

        runtimeModel = ModelLoader.Load(model);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, runtimeModel);

        names = new COCONames();
        numClasses = names.map_zh.Count;
        colors = new COCOColors();
        croppedTexture = new Texture2D(inferenceImgSize, inferenceImgSize, TextureFormat.RGB24, false);
        boxes = new List<DetectionResult>();
        boxes_tmp = new List<DetectionResult>();
        labels = new List<Tuple<GameObject, Renderer, GameObject>>();
        laser = GetComponent<RaycastLaser>();
        speaker = GameObject.Find("TTS").GetComponent<SpeakDetectionResult>();
        webApi = new Hl2ServerApi(baseUrl);
        IEnumerator wait()
        {
            yield return new WaitForSeconds(2.0f);
        }
        StartCoroutine(wait());
        StartCoroutine(DetectWebcam());
        OutputLog("初始化完成", true);
    }


    private void OnDestroy()
    {
        if (videoCapture == null)
            return;

        videoCapture.FrameSampleAcquired += null;
        videoCapture.Dispose();
    }

    private void OnVideoCaptureCreated(VideoCapture v)
    {
        if (v == null)
        {
            Debug.LogError("No VideoCapture found");
            return;
        }

        videoCapture = v;

#if WINDOWS_UWP
#if XR_PLUGIN_OPENXR
        CameraStreamHelper.Instance.SetNativeISpatialCoordinateSystem(_spatialCoordinateSystem);

#endif
#endif

        resolution = CameraStreamHelper.Instance.GetLowestResolution();
        resolution = new HoloLensCameraStream.Resolution(cameraResolutionWidth, cameraResolutionHeight);
        float frameRate = CameraStreamHelper.Instance.GetHighestFrameRate(resolution);

        videoCapture.FrameSampleAcquired += OnFrameSampleAcquired;

        CameraParameters cameraParams = new CameraParameters();
        cameraParams.cameraResolutionHeight = resolution.height;
        cameraParams.cameraResolutionWidth = resolution.width;
        cameraParams.frameRate = Mathf.RoundToInt(frameRate);
        cameraParams.pixelFormat = CapturePixelFormat.BGRA32;

        UnityEngine.WSA.Application.InvokeOnAppThread(() => { pictureTexture = new Texture2D(resolution.width, resolution.height, TextureFormat.BGRA32, false); }, false);

        videoCapture.StartVideoModeAsync(cameraParams, OnVideoModeStarted);

        Debug.LogWarning($"{resolution.height},  {resolution.width}, {cameraParams.frameRate}");
    }

    private void OnVideoModeStarted(VideoCaptureResult result)
    {
        if (result.success == false)
        {
            Debug.LogWarning("Could not start video mode.");
            return;
        }

        Debug.Log("Video capture started.");
    }

    private void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
        // Allocate byteBuffer
        if (_latestImageBytes == null || _latestImageBytes.Length < sample.dataLength)
            _latestImageBytes = new byte[sample.dataLength];

        // Fill frame struct 
        SampleStruct s = new SampleStruct();
        sample.CopyRawImageDataIntoBuffer(_latestImageBytes);
        s.data = _latestImageBytes;

        // Get the cameraToWorldMatrix and projectionMatrix
        if (!sample.TryGetCameraToWorldMatrix(out s.camera2WorldMatrix) || !sample.TryGetProjectionMatrix(out s.projectionMatrix))
            return;

        sample.Dispose();

        camera2WorldMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(s.camera2WorldMatrix);
        projectionMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(s.projectionMatrix);

        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            pictureTexture.LoadRawTextureData(s.data);
            pictureTexture.Apply();

            Vector3 inverseNormal = -camera2WorldMatrix.GetColumn(2);
            // Position the canvas object slightly in front of the real world web camera.
            Vector3 imagePosition = camera2WorldMatrix.GetColumn(3) - camera2WorldMatrix.GetColumn(2);

#if XR_PLUGIN_WINDOWSMR || XR_PLUGIN_OPENXR

            Camera unityCamera = Camera.main;
            Matrix4x4 invertZScaleMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1));
            Matrix4x4 localToWorldMatrix = camera2WorldMatrix * invertZScaleMatrix;
            unityCamera.transform.localPosition = localToWorldMatrix.GetColumn(3);
            unityCamera.transform.localRotation = Quaternion.LookRotation(localToWorldMatrix.GetColumn(2), localToWorldMatrix.GetColumn(1));
#endif
        }, false);
    }

    public List<DetectionResult> getBoxes() { return boxes; }
    void Switch2Ocr()
    {
        if (detectionType != DetectionType.Ocr)
        {
            speaker.textToSpeak = "";
            detectionType = DetectionType.Ocr;
            detectionTypeTMP.text = "当前状态: 文字识别";
            speaker.StartSpeakingText("开始文字识别");
        }
    }
    void Switch2FaceEmotion()
    {
        if (detectionType != DetectionType.FaceEmotion)
        {
            speaker.textToSpeak = "";
            detectionType = DetectionType.FaceEmotion;
            detectionTypeTMP.text = "当前状态: 表情识别";
            speaker.StartSpeakingText("开始表情识别");
        }
    }
    void Switch2Yolo()
    {
        if (detectionType != DetectionType.Yolo)
        {
            speaker.textToSpeak = "";
            detectionType = DetectionType.Yolo;
            detectionTypeTMP.text = "当前状态: 目标识别";
            speaker.StartSpeakingText("开始目标识别");
        }
    }

    private void DetectOneFrame()
    {  //识别帧
        if (!labels.IsUnityNull() && labels.Count > 0)
        {
            foreach (var (go, r, la) in labels)
            { //destroy�������
                Destroy(r);
                Destroy(go);
                Destroy(la);
            }

            labels.Clear();
        }

        switch (detectionType)
        {
            case DetectionType.Yolo:
                //OutputLog("进入DetectOneYoloFrame");
                DetectOneYoloFrame();
                break;
            case DetectionType.Ocr:
                DetectOneOcrFrame();
                break;
            case DetectionType.FaceEmotion:
                DetectOneFaceEmotionFrame();
                break;
            default: 
                break;
        }
    }

    private void OutputLog(string msg, bool clear = false)
    {
        if (clear)
        {
            debugText.text = $"{msg}\n";
        } else
        {
            debugText.text += $"{msg}\n";
        }
    }

    private void DetectOneYoloFrame() {

        camera2WorldMatrix_local = camera2WorldMatrix;
        projectionMatrix_local = projectionMatrix;

        CropTexture();
        //OutputLog("完成CropTexture");

        var tensor = new Tensor(croppedTexture, false, Vector4.one, Vector4.zero);

        worker.Execute(tensor).FlushSchedule(true);
        Tensor result = worker.PeekOutput("output0");
        //OutputLog("完成onnx模型推理");
        boxes_tmp.Clear();
        boxes.Clear();

        ParseYoloOutput(result, confidenceThreshold, boxes_tmp);
        boxes = NonMaxSuppression(0.5f, boxes_tmp);
        //OutputLog("完成boxes预处理操作");

        textToRead = "";
        foreach (var l in boxes) {
            GenerateBoundingBox(l, camera2WorldMatrix_local, projectionMatrix_local);
            textToRead += l.getTextToSpeak() + " ";
        }

        //OutputLog("完成boxes绘制");

        speaker.textToSpeak = textToRead;

        tensor.Dispose();
        result.Dispose();
    }

    private string ConvertPicture2Base64Str(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToJPG();
        string base64Str = Convert.ToBase64String(bytes);
        return base64Str;
    }

    private void DetectOneOcrFrame()
    {

        string res = webApi.OcrPostRequest(ConvertPicture2Base64Str(pictureTexture));

        if (res.Length > 0)
        {
            res = $"识别结果为：{res}";
            OutputLog(res, true);
            speaker.textToSpeak = res;
        }


    }
    private void DetectOneFaceEmotionFrame()
    {

        string res = webApi.FaceEmotionPostRequest(ConvertPicture2Base64Str(pictureTexture));

        if (res.Length > 0)
        {
            res = $"情绪：{res}";
            OutputLog(res, true);
            speaker.textToSpeak = res;
        }

    }

    public IEnumerator DetectWebcam()
    {
        while (true)
        {
            if (pictureTexture)
            {
                float start = Time.realtimeSinceStartup;

                //OutputLog("开始目标识别");

                DetectOneFrame();

                //OutputLog("完成目标识别", true);

                float end = Time.realtimeSinceStartup;
                fpsTMP.text = $"FPS: {(1.0f / (end - start)):N1}";  //��ʾ���֡��

                yield return null;
            }
            else
            {
                yield return null;
            }
        }
    }

    private void ParseYoloOutput(Tensor tensor, float confidenceThreshold, List<DetectionResult> boxes)
    {
            for (int i = 0; i < tensor.shape.width; i++)
            {
                var (label, confidence) = GetClassIdx(tensor, i, 0);
                if (confidence < confidenceThreshold)
                {
                    continue;
                }

                BoundingBox box = ExtractBoundingBox(tensor, i);
                var labelName = names.map_zh[label];
            boxes.Add(new DetectionResult
                {
                    Bbox = box,
                    Confidence = confidence,
                    Label = labelName,
                    LabelIdx = label
                });
            }
    }

    private BoundingBox ExtractBoundingBox(Tensor tensor, int row)
    {
        return new BoundingBox
        {
            X = tensor[0, 0, row, 0],
            Y = tensor[0, 0, row, 1],
            Width = tensor[0, 0, row, 2],
            Height = tensor[0, 0, row, 3]
        };
    }

    private ValueTuple<int, float> GetClassIdx(Tensor tensor, int row, int batch)
    {
        int classIdx = 0;
        float maxConf = tensor[0, 0, row, 4];
        for (int i = 0; i < numClasses; i++)
        {
            if (tensor[batch, 0, row, 4 + i] > maxConf)
            {
                maxConf = tensor[0, 0, row, 4 + i];
                classIdx = i;
            }
        }

        return (classIdx, maxConf);
    }

    private float IoU(Rect boundingBoxA, Rect boundingBoxB)
    {
        float intersectionArea = Mathf.Max(0, Mathf.Min(boundingBoxA.xMax, boundingBoxB.xMax) - Mathf.Max(boundingBoxA.xMin, boundingBoxB.xMin)) *
                        Mathf.Max(0, Mathf.Min(boundingBoxA.yMax, boundingBoxB.yMax) - Mathf.Max(boundingBoxA.yMin, boundingBoxB.yMin));

        float unionArea = boundingBoxA.width * boundingBoxA.height + boundingBoxB.width * boundingBoxB.height - intersectionArea;

        if (unionArea == 0)
        {
            return 0;
        }

        return intersectionArea / unionArea;
    }

    private List<DetectionResult> NonMaxSuppression(float threshold, List<DetectionResult> boxes)
    {
        var results = new List<DetectionResult>();
        if (boxes.Count == 0)
        {
            return results;
        }
        var detections = boxes.OrderByDescending(b => b.Confidence).ToList();
        results.Add(detections[0]);

        for (int i = 1; i < detections.Count; i++)
        {
            bool add = true;
            for (int j = 0; j < results.Count; j++)
            {
                float iou = IoU(detections[i].Rect, results[j].Rect);
                if (iou > threshold)
                {
                    add = false;
                    break;
                }
            }
            if (add)
                results.Add(detections[i]);
        }

        return results;

    }
    public Vector3 shootLaserFrom(Vector3 from, Vector3 direction, float length, Material mat = null)
    {
        Ray ray = new Ray(from, direction);
        Vector3 to = from + length * direction;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, length))
            to = hit.point;

        return to;
    }

    public RaycastHit shootLaserRaycastHit(Vector3 from, Vector3 direction, float length, Material mat = null)
    {
        Ray ray = new Ray(from, direction);
        Vector3 to = from + length * direction;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, length))
            to = hit.point;

        return hit;
    }

    public Vector3 GenerateBoundingBox(DetectionResult det, Matrix4x4 camera2WorldMatrix_local, Matrix4x4 projectionMatrix_local)
    {
        var x_offset = (cameraResolutionWidth - inferenceImgSize) / 2;
        var y_offset = (cameraResolutionHeight - inferenceImgSize) / 2;
        Vector3 direction = LocatableCameraUtils.PixelCoordToWorldCoord(camera2WorldMatrix_local, projectionMatrix_local, resolution, new Vector2(det.Bbox.X + x_offset, det.Bbox.Y + y_offset));
       
        /*
        BoundingBox box = offsetAndScaledBox(det.Bbox);
        Vector3 direction = LocatableCameraUtils.PixelCoordToWorldCoord(camera2WorldMatrix_local, projectionMatrix_local, resolution, new Vector2(box.X, box.Y));
        */

        var centerHit = shootLaserRaycastHit(camera2WorldMatrix_local.GetColumn(3), direction, 10f);
        var distance = centerHit.distance;
        distance -= 0.05f;

        Vector3 corner_0 = LocatableCameraUtils.PixelCoordToWorldCoord(camera2WorldMatrix_local, projectionMatrix_local, resolution, new Vector2((det.Bbox.X + x_offset) - (det.Bbox.Width / 2), det.Bbox.Y + y_offset - (det.Bbox.Height / 2)));  //���Ͻǵ�
        Vector3 corner_1 = LocatableCameraUtils.PixelCoordToWorldCoord(camera2WorldMatrix_local, projectionMatrix_local, resolution, new Vector2((det.Bbox.X + x_offset) - (det.Bbox.Width / 2), det.Bbox.Y + y_offset + (det.Bbox.Height / 2)));  //���½ǵ�
        Vector3 corner_2 = LocatableCameraUtils.PixelCoordToWorldCoord(camera2WorldMatrix_local, projectionMatrix_local, resolution, new Vector2((det.Bbox.X + x_offset) + (det.Bbox.Width / 2), det.Bbox.Y + y_offset - (det.Bbox.Height / 2)));  //���Ͻǵ�
        Vector3 corner_3 = LocatableCameraUtils.PixelCoordToWorldCoord(camera2WorldMatrix_local, projectionMatrix_local, resolution, new Vector2((det.Bbox.X + x_offset) + (det.Bbox.Width / 2), det.Bbox.Y + y_offset + (det.Bbox.Height / 2)));  //���½ǵ�
        
        /*
        Vector3 corner_0 = LocatableCameraUtils.PixelCoordToWorldCoord(camera2WorldMatrix_local, projectionMatrix_local, resolution, new Vector2(box.X - box.Width / 2, box.Y - box.Height / 2)); //left-top
        Vector3 corner_1 = LocatableCameraUtils.PixelCoordToWorldCoord(camera2WorldMatrix_local, projectionMatrix_local, resolution, new Vector2(box.X - box.Width / 2, box.Y + box.Height / 2)); //left-bottom
        Vector3 corner_2 = LocatableCameraUtils.PixelCoordToWorldCoord(camera2WorldMatrix_local, projectionMatrix_local, resolution, new Vector2(box.X + box.Width / 2, box.Y - box.Height / 2)); //right-top
        Vector3 corner_3 = LocatableCameraUtils.PixelCoordToWorldCoord(camera2WorldMatrix_local, projectionMatrix_local, resolution, new Vector2(box.X + box.Width / 2, box.Y + box.Height / 2)); //right-bottom
        */

        var point_0 = shootLaserFrom(camera2WorldMatrix_local.GetColumn(3), corner_0, distance);
        var point_1 = shootLaserFrom(camera2WorldMatrix_local.GetColumn(3), corner_1, distance);
        var point_2 = shootLaserFrom(camera2WorldMatrix_local.GetColumn(3), corner_2, distance);
        var point_3 = shootLaserFrom(camera2WorldMatrix_local.GetColumn(3), corner_3, distance);
        det.Center = (point_0 + point_2) / 2.0f;  //求矩形框的中点
        det.Distance = Vector3.Distance(det.Center, Camera.main.transform.position);  //求矩形框的中点到摄像机的距离
        Vector3 direct = Camera.main.transform.position - det.Center;
        det.Angle = Quaternion.LookRotation(direct).eulerAngles;  //求矩形框的中点与摄像机的欧拉角

        var go = new GameObject();

        go.transform.position = point_0;
        go.transform.rotation = Camera.main.transform.rotation;

        var renderer = go.GetComponent<Renderer>();

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.widthMultiplier = 0.01f;
        lr.loop = true;
        lr.positionCount = 4;
        lr.material = laserMaterial;
        lr.material.color = colors.map[det.LabelIdx];

        lr.SetPosition(0, point_0);
        lr.SetPosition(3, point_1);
        lr.SetPosition(1, point_2);
        lr.SetPosition(2, point_3);

        Vector3 pos = go.transform.position;
        pos.y += pos.z / 40.0f; // +=size / 10
        pos.x += pos.z / 4.0f;  // +=size

        GameObject labelGo = GameObject.Instantiate(labelPrefab, pos, go.transform.rotation);
        TextMeshPro labelTMP = labelGo.GetComponent<TextMeshPro>();
        labelTMP.text = det.ToString();

        labelTMP.fontSize = pos.z / 4.0f;

        labels.Add(Tuple.Create(go, renderer, labelGo));

        return centerHit.point;
    }

    private void CropTexture()
    {   //pictureTexture: 424x240 -> croppedTexture: 160x160
        //从pictureTexture中间裁剪一块cropWidth*cropHeight的图像出来
        int centerX = (pictureTexture.width - inferenceImgSize) / 2;
        int centerY = (pictureTexture.height - inferenceImgSize) / 2;
        Color[] pixels = pictureTexture.GetPixels(centerX, centerY, inferenceImgSize, inferenceImgSize);

        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();
    }

   /*
    private BoundingBox offsetAndScaledBox(BoundingBox box) {
        //将在小图中生成的Box转换到原始大图中
        BoundingBox newBox = new BoundingBox();
        float x_offset = (scaledTexture.width - inferenceImgSize) / 2;
        float y_offset = (scaledTexture.height - inferenceImgSize) / 2;
        newBox.X = (box.X + x_offset) / scaledRatio;
        newBox.Y = (box.Y + y_offset) / scaledRatio;
        newBox.Width = box.Width / scaledRatio;
        newBox.Height = box.Height / scaledRatio;
        return newBox;
    }
    private void ScaleTexture(Texture2D tex, float ratio)
    {
        Color color;
        for (int i = 0; i < scaledTexture.height; i++)
        {
            for (int j = 0; j < scaledTexture.width; j++)
            {
                color = tex.GetPixel((int)(j * (1 / ratio)), (int)(i * (1 / ratio)));
                scaledTexture.SetPixel(j, i, color);
            }
        }
    }

    private void ScaleCropTexture() {
        //pictureTexture: 1920x1080 -> scaledTexture: 384x216 -> croppedTexture: 160x160
        int scaledWidth = (int)(pictureTexture.width * scaledRatio);
        int scaledHeight = (int)(pictureTexture.height * scaledRatio);

        ScaleTexture(pictureTexture, scaledRatio);

        int leftTopX = (scaledWidth - inferenceImgSize) / 2;
        int leftTopY = (scaledHeight - inferenceImgSize) / 2;
        croppedTexture.SetPixels(scaledTexture.GetPixels(leftTopX, leftTopY, inferenceImgSize, inferenceImgSize));
        croppedTexture.Apply();
}
    */


}