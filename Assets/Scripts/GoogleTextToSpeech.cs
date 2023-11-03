using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
[RequireComponent(typeof(AudioSource))]
public class GoogleTextToSpeech : MonoBehaviour
{
    [SerializeField]
    private string apikey;
    private string URL;
    private AudioSource _audioSource;

    [System.Serializable]
    private class SynthesisInput
    {
        public string text;
    }

    [System.Serializable]
    private class VoiceSelectionParams
    {
        public string languageCode = "ja-JP";
        public string name;
    }

    [System.Serializable]
    private class AudioConfig
    {
        public string audioEncoding = "LINEAR16";

        public int speakingRate = 1;
        public int pitch = 0;
        public int sampleRateHertz = 16000;
    }

    [System.Serializable]
    private class SynthesisRequest
    {
        public SynthesisInput input;
        public VoiceSelectionParams voice;
        public AudioConfig audioConfig;
    }

    [System.Serializable]
    private class SynthesisResponse
    {
        public string audioContent;
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        URL = "https://texttospeech.googleapis.com/v1/text:synthesize?key=" + apikey;
    }

    public void SynthesizeAndPlay(string text)
    {
        StartCoroutine(Synthesize(text));
    }

    private IEnumerator Synthesize(string text)
    {
        SynthesisRequest requestData = new SynthesisRequest
        {
            input = new SynthesisInput { text = text },
            voice = new VoiceSelectionParams { languageCode = "ja-JP", name = "ja-JP-Neural2-B" },
            audioConfig = new AudioConfig { audioEncoding = "LINEAR16", speakingRate = 1 , pitch = 0, sampleRateHertz = 16000 }
        };

        using (UnityWebRequest www = new UnityWebRequest(URL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestData));
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string response = www.downloadHandler.text;
                SynthesisResponse synthesisResponse = JsonUtility.FromJson<SynthesisResponse>(response);
                PlayAudioFromBase64(synthesisResponse.audioContent);
            }
            else
            {
                Debug.LogError("Google Text-to-Speech Error: " + www.error);
            }
        }
    }

    private void PlayAudioFromBase64(string base64AudioData)
    {
        byte[] audioBytes = System.Convert.FromBase64String(base64AudioData);
        LoadAudioClipAndPlay(audioBytes);
    }

    private void LoadAudioClipAndPlay(byte[] audioData)
    {
        int sampleRate = 16000; // Google Text-to-Speechのデフォルトサンプルレートは16kHz
        int channels = 1; // モノラル

        int samplesCount = audioData.Length / 2; // 16-bit PCM, so 2 bytes per sample
        float[] audioFloatData = new float[samplesCount];
        
        // Convert PCM byte data to float array
        for (int i = 0; i < samplesCount; i++)
        {
            // Convert two bytes to one short int
            short sampleInt = BitConverter.ToInt16(audioData, i * 2);
            // Convert short int range (-32768 to 32767) to float range (-1 to 1)
            audioFloatData[i] = sampleInt / 32768.0f;
        }

        AudioClip clip = AudioClip.Create("SynthesizedSpeech", samplesCount, channels, sampleRate, false);
        clip.SetData(audioFloatData, 0);

        _audioSource.clip = clip;
        _audioSource.Play();
    }
}
