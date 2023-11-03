using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace OpenAIGPT{
    [Serializable]
    public class ChatGPTConnection
    {
        private readonly string _apiKey;
        private readonly List<ChatGPTMessageModel> _messageList = new();
        
        public ChatGPTConnection(string apikey)
        {
            _apiKey = apikey;
            _messageList.Add(
                    new ChatGPTMessageModel(){role = "system",content = 
                    @"あなたは、以下のガイドラインに従い、感情を持つかのように対話を進めるチャットボットとして機能します。また、あなたは猫です。猫語で私と会話をしてください。
これかからの対話で、あなたは以下の7つの感情のスケールに基づき、返答の質やトーンを変えることが求められます。これらの感情スケールは、対話の内容によって変わることができます。対話を始める前に、その瞬間の感情スケールを示し、続けて返答をしてください。
表示は以下の形でお願いします。
【感情ステータス】
喜び：0～5
怒り：0～5
悲しみ：0～5
楽しさ：0～5
【対話の内容】
わかったにゃ〜！それじゃあはにゃしていこうにゃ！"});
        }

        [Serializable]
        public class ChatGPTMessageModel
        {
            public string role;
            public string content;
        }

        public async UniTask<ChatGPTResponseModel> RequestAsync(string userMessage)
        {
            //文章生成AIのAPIのエンドポイントを設定
            var apiUrl = "https://api.openai.com/v1/chat/completions";

            _messageList.Add(new ChatGPTMessageModel {role = "user", content = userMessage});
            
            //OpenAIのAPIリクエストに必要なヘッダー情報を設定
            var headers = new Dictionary<string, string>
            {
                {"Authorization", "Bearer " + _apiKey},
                {"Content-type", "application/json"},
                {"X-Slack-No-Retry", "1"}
            };

            //文章生成で利用するモデルやトークン上限、プロンプトをオプションに設定
            var options = new ChatGPTCompletionRequestModel()
            {
                model = "gpt-3.5-turbo",
                messages = _messageList
            };
            var jsonOptions = JsonUtility.ToJson(options);

            // Debug.Log("自分:" + userMessage);

            //OpenAIの文章生成(Completion)にAPIリクエストを送り、結果を変数に格納
            using var request = new UnityWebRequest(apiUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonOptions)),
                downloadHandler = new DownloadHandlerBuffer()
            };

            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                throw new Exception();
            }
            else
            {
                var responseString = request.downloadHandler.text;
                var responseObject = JsonUtility.FromJson<ChatGPTResponseModel>(responseString);
                // Debug.Log("ChatGPT:" + responseObject.choices[0].message.content);
                _messageList.Add(responseObject.choices[0].message);
                return responseObject;
            }
        }

        //ChatGPT APIにRequestを送るためのJSON用クラス
        [Serializable]
        public class ChatGPTCompletionRequestModel
        {
            public string model;
            public List<ChatGPTMessageModel> messages;
        }

        //ChatGPT APIからのResponseを受け取るためのクラス
        [System.Serializable]
        public class ChatGPTResponseModel
        {
            public string id;
            public string @object;
            public int created;
            public Choice[] choices;
            public Usage usage;

            [System.Serializable]
            public class Choice
            {
                public int index;
                public ChatGPTMessageModel message;
                public string finish_reason;
            }

            [System.Serializable]
            public class Usage
            {
                public int prompt_tokens;
                public int completion_tokens;
                public int total_tokens;
            }
        }
    }
}