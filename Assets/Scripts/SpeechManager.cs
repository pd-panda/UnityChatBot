using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static WhisperSpeechToText; 
public class SpeechManager : MonoBehaviour
{
    public void OnClick()
    {
        var whisperSpeechToText = GetComponent<WhisperSpeechToText>();
        var recordingstate = GetComponentInChildren<Text>();
        if (whisperSpeechToText.IsRecording())
        {
            recordingstate.text = "録音開始";
            whisperSpeechToText.StopRecording();
        }
        else
        {
            recordingstate.text = "録音停止";
            whisperSpeechToText.StartRecording();
        }
    }
}
