using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangeSpeechSpeed : MonoBehaviour {
    public TMP_Text speechSpeed;

    private float speed;
    private AudioSource audioSource;

    private void Start() {
        audioSource = gameObject.GetComponent<AudioSource>();
        speed = LoadSpeed();
        ChangeSpeed(0);
    }

    void SpeedUp() {
        if (speed < 3) {
            ChangeSpeed(0.25f);
        }
    }

    void SlowDown() {
        if (speed > 0) {
            ChangeSpeed(-0.25f);
        }
    }

    void ChangeSpeed(float change) {
        speed += change;
        speechSpeed.text = $"²¥±¨ËÙ¶È: x{speed}";
        SaveSpeed(speed);
        audioSource.pitch = speed;
    }

    void SaveSpeed(float speed) {
        PlayerPrefs.SetFloat("speed", speed);
        PlayerPrefs.Save();
    }

    float LoadSpeed() {
        if (PlayerPrefs.HasKey("speed")) {
            return PlayerPrefs.GetFloat("speed");
        } else {
            return 1.5f;
        }
    }
}
