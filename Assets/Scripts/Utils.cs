using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using UnityEngine.Networking;


public class BoundingBox
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}
public class DetectionResult
{
    public BoundingBox Bbox { get; set; }
    public string Label { get; set; }
    public int LabelIdx { get; set; }
    public float Confidence { get; set; }

    public float Distance { get; set; }
    public Vector3 Center { get; set; }
    public Vector3 Angle { get; set; }
    private string _dirText;
    public string DirText 
    {
        get 
        {
            if (_dirText == null || _dirText.Length == 0) 
            {
                string dirText = "";  //(左/右)前(上/下)
                float yaw = Angle[1];  //偏航角
                if (yaw <= 150) {
                    dirText += "左";
                }
                else if (yaw > 210) {
                    dirText += "右";
                }
                dirText += "前";
                float pitch = Angle[0] <= 90 ? Angle[0] : Angle[0] - 360; //俯仰角
                if (pitch <= -30) {
                    dirText += "下";
                }
                else if (pitch > 30) {
                    dirText += "上";
                }
                _dirText = dirText;
            }
            return _dirText;
        } 
    }

    public Rect Rect
    {
        get { return new Rect(Bbox.X, Bbox.Y, Bbox.Width, Bbox.Height); }
    }

    public override string ToString()
    {
        return $"{Label}:{Confidence:N2}[距离:{Distance:N2}m][方向:{DirText}]";
    }

    public string getTextToSpeak()
    {
        return $"{Label}{DirText}{Distance:N2}米";
    }
}

public class COCONames
{
    public List<String> map_zh;

    public COCONames()
    {
        /*
        map = new List<string>(){
        "person",
        "bicycle",
        "car",
        "motorcycle",
        "airplane",
        "bus",
        "train",
        "truck",
        "boat",
        "traffic light",
        "fire hydrant",
        "stop sign",
        "parking meter",
        "bench",
        "bird",
        "cat",
        "dog",
        "horse",
        "sheep",
        "cow",
        "elephant",
        "bear",
        "zebra",
        "giraffe",
        "backpack",
        "umbrella",
        "handbag",
        "tie",
        "suitcase",
        "frisbee",
        "skis",
        "snowboard",
        "sports ball",
        "kite",
        "baseball bat",
        "baseball glove",
        "skateboard",
        "surfboard",
        "tennis racket",
        "bottle",
        "wine glass",
        "cup",
        "fork",
        "knife",
        "spoon",
        "bowl",
        "banana",
        "apple",
        "sandwich",
        "orange",
        "broccoli",
        "carrot",
        "hot dog",
        "pizza",
        "donut",
        "cake",
        "chair",
        "couch",
        "potted plant",
        "bed",
        "dining table",
        "toilet",
        "tv",
        "laptop",
        "mouse",
        "remote",
        "keyboard",
        "cell phone",
        "microwave",
        "oven",
        "toaster",
        "sink",
        "refrigerator",
        "book",
        "clock",
        "vase",
        "scissors",
        "teddy bear",
        "hair drier",
        "toothbrush"
        };
        */
        /**
        map_zh = new List<string>(){
            "人",
            "自行车",
            "汽车",
            "摩托车",
            "飞机",
            "公交车",
            "火车",
            "货车",
            "船",
            "交通信号灯",
            "消防栓",
            "停止标志",
            "停车计时器",
            "长凳",
            "鸟",
            "猫",
            "狗",
            "马",
            "羊",
            "奶牛",
            "大象",
            "熊",
            "斑马",
            "长颈鹿",
            "背包",
            "雨伞",
            "手提包",
            "领带",
            "手提箱",
            "飞盘",
            "滑雪橇",
            "滑雪板",
            "球",
            "风筝",
            "棒球棒",
            "棒球手套",
            "滑板",
            "冲浪板",
            "网球拍",
            "瓶子",
            "红酒杯",
            "杯子",
            "餐叉",
            "刀子",
            "勺子",
            "碗",
            "香蕉",
            "苹果",
            "三明治",
            "橘子",
            "西兰花",
            "胡萝卜",
            "热狗",
            "披萨",
            "甜甜圈",
            "蛋糕",
            "椅子",
            "长沙发",
            "盆栽",
            "床",
            "餐桌",
            "马桶",
            "电视",
            "笔记本电脑",
            "鼠标",
            "遥控器",
            "键盘",
            "手机",
            "微波炉",
            "烤箱",
            "烤面包炉",
            "水槽",
            "冰箱",
            "书",
            "钟",
            "花瓶",
            "剪刀",
            "泰迪熊",
            "吹风机",
            "牙刷"
        };
        */
        ///**
        map_zh = new List<string>(){
           "猫",
           "狗",
          "背包",
          "雨伞",
          "手提包",
          "手提箱",
           "水瓶",
           "茶杯",
           "叉子",
           "小刀",
           "勺子",
           "碗",
          "香蕉",
          "苹果",
          "三明治",
          "椅子",
           "床",
          "餐桌",
           "电视",
          "笔记本电脑",
          "鼠标",
          "键盘",
          "微波炉",
          "烤箱",
          "冰箱",
          "书",
          "时钟",
          "花瓶",
          "手机",
          "人脸",
          "文字"
          };

        //*/

    }
}

public class COCOColors
{
    public List<Color> map;

    public COCOColors()
    {
        map = new List<Color>();
        for (var i = 0; i < 80; ++i)
        {
            map.Add(UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
        }
        map[0] = new Color(255.0f, 0.0f, 127.0f);
    }
}

public enum DetectionType
{
    Yolo,
    Ocr,
    FaceEmotion
}

[Serializable]
public class OcrLine
{
    public float confidence;
    public List<List<float>> position;
    public string text;
}

[Serializable]
public class OcrData
{
    public List<OcrLine> lines;
}

[Serializable]
public class OcrResult
{
    public OcrData data;
}
[Serializable]
public class FaceEmotionData
{
    public float confidence;
    public string emotion;
}

[Serializable]
public class FaceEmotionResult
{
    public FaceEmotionData data;
}

public class Hl2ServerApi
{
    private string baseUrl;
    private string ocrUrl;
    private string faceUrl;
    private UnityWebRequest request;
    public Hl2ServerApi(string bUrl = "http://127.0.0.1:5000")
    {
        baseUrl = bUrl;
        ocrUrl = baseUrl + "/ocr";
        faceUrl = baseUrl + "/face_emotion_recognition";
    }

    private string SendRequest(DetectionType type, string fileBase64Str)
    {
        //string fileBase64Str = ReadBase64StrFromLocal();
        string jsonStr = "{\"file\": \"" + fileBase64Str + "\"}";
        if (fileBase64Str == null)
        {
            return null;
        }
        string url = (type == DetectionType.Ocr) ? ocrUrl : faceUrl;
        request = UnityWebRequest.Post(url, jsonStr);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonStr);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SendWebRequest();

        while (!request.isDone) ;  // 等待请求完成

        if (request.result != UnityWebRequest.Result.Success)
        { //请求出错
            //Debug.LogError(request.error);
            request.uploadHandler.Dispose();
            request.downloadHandler.Dispose();
            request.Dispose();
            return null;
        }
        else
        {
            string jsonString = request.downloadHandler.text;
            //Debug.Log("jsonString: " + jsonString);
            request.uploadHandler.Dispose();
            request.downloadHandler.Dispose();
            request.Dispose();
            return jsonString;
        }
    }

    public string OcrPostRequest(string fileBase64Str)
    {
        string jsonString = SendRequest(DetectionType.Ocr, fileBase64Str);
        string wholeText = "";
        if (jsonString != null)
        {
            OcrResult ocrResult = JsonUtility.FromJson<OcrResult>(jsonString);
            if (!ocrResult.IsUnityNull())
            {
                foreach (OcrLine line in ocrResult.data.lines)
                {
                    wholeText += line.text;
                }
            }
        }
        return wholeText;
    }
    public string FaceEmotionPostRequest(string fileBase64Str)
    {
        string jsonString = SendRequest(DetectionType.FaceEmotion, fileBase64Str);
        string wholeText = "";
        if (jsonString != null)
        {
            FaceEmotionResult faceResult = JsonUtility.FromJson<FaceEmotionResult>(jsonString);
            if (faceResult.data.confidence != 0)
            {
                wholeText = faceResult.data.emotion;
            }
        }
        return wholeText;

    }
}
