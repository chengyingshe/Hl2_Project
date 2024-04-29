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
        isSpeakingTMP.text = toOpen ? "语音播报: 开启" : "语音播报: 关闭";
        if (speaking)
            StartSpeakingText(toOpen ? "已开启语音播报" : "已关闭语音播报");
    }
    public bool IsReady() { return speechOpened && !speaker.IsSpeaking() && textToSpeak.Length > 0; }
    public void SpeakResult()
    {
        //text = "手机前方1米";
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
