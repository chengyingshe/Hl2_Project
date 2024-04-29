using Microsoft.MixedReality.Toolkit.Audio;
using TMPro;
using UnityEngine;

public class SpeakDetectionResult : MonoBehaviour
{
    public string textToSpeak;
    public bool speechOpened = true;
    public TMP_Text isSpeakingTMP;

    private TextToSpeech speaker;

    private void Start()
    {
        textToSpeak = "";
        speaker = gameObject.GetComponent<TextToSpeech>();
    }

    void OpenSpeech()
    {
        SwitchDetectionSpeech(true, true);
    }
    void CloseSpeech()
    {
        SwitchDetectionSpeech(false, true);
    }

    public void SwitchDetectionSpeech(bool toOpen = true, bool speaking = true)
    {
        speechOpened = toOpen;
        isSpeakingTMP.text = toOpen ? "��������: ����" : "��������: �ر�";
        if (speaking)
            StartSpeakingText(toOpen ? "�ѿ�����������" : "�ѹر���������");
    }
    public bool IsReady() { return speechOpened && !speaker.IsSpeaking() && textToSpeak.Length > 0; }
    public void SpeakResult()
    {
        //text = "�ֻ�ǰ��1��";
        speaker.StartSpeaking(textToSpeak);
    }

    public bool IsSpeaking() { return speaker.IsSpeaking(); }

    public void StartSpeakingText(string text, bool interrupt = true)
    {
        if (interrupt)
        {
            StopSpeaking();
        }
        speaker.StartSpeaking(text);
    }

    public void StopSpeaking()
    {
        if (speaker.IsSpeaking())
        {
            speaker.StopSpeaking();
        }
    }
}
