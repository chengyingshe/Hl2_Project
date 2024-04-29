using Microsoft.MixedReality.Toolkit.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{   
    private SpeakDetectionResult speaker;

    void Start()
    {
        GameObject tts = GameObject.Find("TTS");
        speaker = tts.GetComponent<SpeakDetectionResult>();
        StartCoroutine(ReadText());
    }


    public IEnumerator ReadText() {
        while (true) {
            if (speaker.IsReady())
            {
                speaker.SpeakResult();
                yield return new WaitForSeconds(1f);
                while (speaker.IsSpeaking()) 
                { 
                    yield return new WaitForSeconds(0.5f); 
                }
                yield return null;
            } else
            {
                yield return null;
            }
        }
    }

    public void ApplicationQuit() {
        Application.Quit();
    }
}
