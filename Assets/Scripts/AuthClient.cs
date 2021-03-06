using UnityEngine;
using System;
using System.Collections;
using WebSocket4Net;
using LitJson;


public class AuthClient: MonoBehaviour
{
	const int MAX_LINES = 10;
	
	private WebSocket websocket;
	private String userid = "";
	private String password = "";
	private bool createNew = false;
	private bool loged = false;
	private Rect authWindowRect;

	void OnGUI ()
	{
		authWindowRect = GUILayout.Window (2, authWindowRect, MakeAuthWindow, "ユーザー認証コンソール");
	}

	private void websocket_Opened(object sender, EventArgs e)
	{
	    //websocket.Send("{\"type\":\"join\",\"user\":\"" + userName + "\"}");
	    //     websocket.Send("Hello World!");
	}
	
	private void websocket_Closed(object sender, EventArgs e)
	{
	    //     websocket.Send("Hello World!");
	}
	
	private void websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
	{
	/*
	  $('#chat-area').empty()
	    .addClass('alert alert-error')
	    .append('<button type="button" class="close" data-dismiss="alert">×</button>',
	      $('<i/>').addClass('icon-warning-sign'),
	      'サーバに接続できませんでした。'
	    );
	 */
	}
	
	private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
	{
		// 受信したメッセージを復元
		//DataList[ptr++] = JsonMapper.ToObject<Data> (e.Message );
		//if( ptr >= MAX_LINES )
		//	ptr = 0;
		Result result = JsonMapper.ToObject<Result> (e.Message );
		Debug.Log ("Result: " + result.state);
		Debug.Log ("Mssage: " + result.message);
		if (result.state == "allow") {
			loged = true;
		}
	}

	void Awake ()
	{
		DontDestroyOnLoad (this);
		authWindowRect = new Rect (10, 0, 300, 80);
		
		websocket = new WebSocket("ws://172.31.8.111:8002/");
		websocket.Opened += new EventHandler(websocket_Opened);
		websocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs >(websocket_Error);
		websocket.Closed += new EventHandler(websocket_Closed);
		websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(websocket_MessageReceived);
		websocket.Open();
	}
	
	void Update() {
		if (loged) {
			CreateChatGui cc = FindObjectOfType (typeof(CreateChatGui)) as CreateChatGui;
			if (cc) {
				cc.SetLog(userid, true);
			}
			Destroy(this);
		}
	}
	
	void MakeAuthWindow (int id)
	{
		GUILayout.Space (10);
		
		GUILayout.BeginHorizontal ();
		GUILayout.Space (10);
		GUILayout.Label("User ID", GUILayout.MinWidth (60));
		userid = GUILayout.TextField (userid, 255, GUILayout.MinWidth (120));
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		GUILayout.Space (10);
		GUILayout.Label("password", GUILayout.MinWidth (60));
		password = GUILayout.PasswordField (password, '*', GUILayout.MinWidth (120));
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		GUILayout.Space (10);
		createNew = GUILayout.Toggle(createNew, "アカウントの新規作成");
		if (createNew) {
			if (GUILayout.Button ("登録")) {
				websocket.Send("{\"type\":\"regist\",\"userid\":\"" + userid + "\",\"password\":\"" + password + "\"}");
			}
		} else {
			if (GUILayout.Button ("ログイン")) {
				websocket.Send("{\"type\":\"login\",\"userid\":\"" + userid + "\",\"password\":\"" + password + "\"}");
			}
		}
		GUILayout.EndHorizontal ();

		GUILayout.FlexibleSpace ();
	}

	void OnDestroy ()
	{
        websocket.Send("{\"type\":\"logout\",\"user\":\"" + userid + "\"}");
        websocket.Close();
	}
	
[System.Serializable]
public class Result {
		public string state = "";
		public string message = "";
}
	
}
