using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Audio;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class Navigator : MonoBehaviour
{
    public TMP_Text speechToTextTMP;
    public GameObject targetObjectPrefab;
    public string dictationResult
    {
        set
        {
            RecognizeText(value);
        }
    }

    private DictationHandler dictationHandler;
    private Detection detection;
    private SpeakDetectionResult speaker;
    private bool isNavigating = false;
    private COCONames cocoNames;
    private string targetObjectName;
    private GameObject targetObject;

    private void Start()
    {
        speaker = GameObject.Find("TTS").GetComponent<SpeakDetectionResult>();
        dictationHandler = GetComponent<DictationHandler>();
    }
    void StartNavigation()
    {
        if (isNavigating) return;
        isNavigating = true;
        detection.isDetecting = false;
        speaker.SwitchDetectionSpeech(false, false);
        speaker.StartSpeakingText("�����������ҪѰ�ҵ�Ŀ��");
        dictationHandler.StartRecording();
    }

    bool FindKeyword(string text)
    {
        targetObjectName = "";
        foreach (string obj in cocoNames.map_zh)
        {
            if (text.IndexOf(obj) >= 0)
            { //���ҵ���Ʒ��
                targetObjectName = obj;
                return true;
            }
        }
        return false;
    }

    void RecognizeText(string res)
    {
        speechToTextTMP.text = $"ʶ���ı�: {res}";
        bool found = FindKeyword(res);
        if (found)
        {
            detection.isDetecting = true;
            speaker.StartSpeakingText($"��ʼѰ����Ʒ��{targetObjectName}����ת��ͷ�����м��");
            FindTarget(targetObjectName);
        }
    }

    public void RecognizeComplete(string res)
    {
        bool found = FindKeyword(res);
        if (!found)
        {
            isNavigating = false;
            StartNavigation();
        }
    }

    void FindTarget(string target)
    {
        if (targetObject != null)
        {
            GameObject.Destroy(targetObject);
        }

        detection.detectionType = DetectionType.Yolo; //�л�ΪĿ����ģʽ
        speaker.SwitchDetectionSpeech(true, false);
        bool found = false;
        DetectionResult box = new DetectionResult();
        while (!found)
        {
            foreach (DetectionResult res in detection.getBoxes())
            {
                if (res.Label.Equals(target))
                {
                    box = res;
                    found = true;
                    break;
                }
            }
        }
        Vector3 center = box.Center; //�������ĵ�����
       
        //�ڵ�ǰλ�÷���һ��Ԥ����
        targetObject = GameObject.Instantiate(targetObjectPrefab, center, Quaternion.identity);
        Navigating();
    }

    void Navigating(float minDist = 1f)
    {
        //TODO: ʵ�ֵ�������

        Camera mainCamera = Camera.main;
        Vector3 camPos = mainCamera.transform.position;
        Vector3 targetObjPos = targetObject.transform.position;
        float dist = Vector3.Distance(camPos, targetObjPos);
        float angle = Vector3.Angle(mainCamera.transform.forward, targetObjPos - camPos);
        while (dist > minDist)
        {

        }
    }

    void StopNavigation()
    {
        if (!isNavigating) return;
        isNavigating = false;
        speaker.SwitchDetectionSpeech(false, false);
        speaker.StartSpeakingText("��ֹͣ����");
    }
}
