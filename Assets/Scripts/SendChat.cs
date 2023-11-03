using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAIGPT;
using UnityEngine.UI;
using System.Text.RegularExpressions; // 正規表現用
using UnityChan;

public class SendChat : MonoBehaviour
{
    [SerializeField]
    private string openAIApiKey;

    [SerializeField]
    private InputField inputField;

    [SerializeField]
    private GameObject content_obj;

    [SerializeField]
    private GameObject chat_obj;

    [SerializeField]
    private GameObject speech_obj;

    [SerializeField]
    private GameObject avatar_obj;

    public void OnClick()
    {
        var text = inputField.GetComponent<InputField>().text;

        sendmessage(text);
        inputField.GetComponent<InputField>().text = "";
    }

    private async void sendmessage(string text)
    {
        var chatGPTConnection = new ChatGPTConnection(openAIApiKey);
        var sendObj = Instantiate(chat_obj, this.transform.position, Quaternion.identity);
        sendObj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
        GameObject Child = sendObj.transform.GetChild(0).gameObject;
        Child.GetComponent<Text>().text = text;
        sendObj.transform.SetParent(content_obj.transform, false);

        var response = await chatGPTConnection.RequestAsync(text);

        if (response.choices != null && response.choices.Length > 0)
        {
            var choice = response.choices[0];
            Debug.Log("ChatGPT Response: " + choice.message.content);
            // テキストと感情を分割する
            var match = Regex.Match(choice.message.content, 
            @"【感情ステータス】
喜び：(?<happy>\d+).*
怒り：(?<angry>\d+).*
悲しみ：(?<sad>\d+).*
楽しさ：(?<excited>\d+).*
【対話の内容】
(?<text>.+)", RegexOptions.Singleline);
            Debug.Log("ChatGPT Response: " + match.Groups["happy"].Value);
            Debug.Log("ChatGPT Response: " + match.Groups["text"].Value);
            var responseData = new Response(match);
            if (speech_obj == null) 
            {
                Debug.LogError("speech_obj is null!");
                return;
            }
            if (speech_obj.GetComponent<GoogleTextToSpeech>() == null) 
            {
                Debug.LogError("GoogleTextToSpeech component is missing on speech_obj!");
                return;
            }
            speech_obj.GetComponent<GoogleTextToSpeech>().SynthesizeAndPlay(responseData.GetResponseText());

            var responseObj = Instantiate(chat_obj, this.transform.position, Quaternion.identity);
            responseObj.GetComponent<Image>().color = new Color(0.6f, 1.0f, 0.1f, 0.3f);
            GameObject Child_responce = responseObj.transform.GetChild(0).gameObject;
            Child_responce.GetComponent<Text>().text = responseData.GetResponseText();
            responseObj.transform.SetParent(content_obj.transform, false);
            avatar_obj.GetComponent<FaceUpdate>().OnCallChangeFace(avatar_obj.GetComponent<FaceUpdate>().animations[responseData.GetMostEmotion()].name);
        }
    }
}

class Response
{
    // メンバー
    private int happy;
    private int angry;
    private int sad;
    private int excited;
    private string responsetext;

    // コンストラクタ
    public Response(System.Text.RegularExpressions.Match match)
    {
        happy = int.Parse(match.Groups["happy"].Value);
        angry = int.Parse(match.Groups["angry"].Value);
        sad = int.Parse(match.Groups["sad"].Value);
        excited = int.Parse(match.Groups["excited"].Value);
        responsetext = match.Groups["text"].Value;
    }

    // ゲッター
    public int GetHappy() { return happy; }
    public int GetAngry() { return angry; }
    public int GetSad() { return sad; }
    public int GetExcited() { return excited; }
    public string GetResponseText() { return responsetext; }

    public int GetMostEmotion() {
        // 最も高い感情の名前を返す
        if (happy > angry && happy > sad && happy > excited) {
            return 1;
        } else if (angry > happy && angry > sad && angry > excited) {
            return 2;
        } else if (sad > happy && sad > angry && sad > excited) {
            return 3;
        } else if (excited > happy && excited > angry && excited > sad) {
            return 4;
        } else
        {
            return 0;
        }
    }

    // 必要に応じてメソッドを追加
    // 例: public string GetResponseText() { return responsetext; }
}