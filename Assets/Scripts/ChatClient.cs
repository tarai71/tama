using UnityEngine;
using System;
using System.Collections;
using WebSocket4Net;
using LitJson;

public class ChatClient : MonoBehaviour
{
	const int MAX_LINES = 10;
	
	private WebSocket websocket;
	private String userName;
	private Rect chatWindowRect;
	private Rect messWindowRect;
	private Data[] DataList = new Data[MAX_LINES];
	private int ptr = 0;
	private string stringToEdit = "";

	public GUIStyle style1;
	public GUIStyle style2;
	public GUIStyle style3;
	public GUIStyle style4;
	
	void OnGUI ()
	{
		chatWindowRect = GUILayout.Window (3, chatWindowRect, MakeChatWindow, "チャットコンソール");
		messWindowRect = GUILayout.Window (4, messWindowRect, MakeMessWindow, "メッセージコンソール");
	}

	private void websocket_Opened(object sender, EventArgs e)
	{
	    websocket.Send("{\"type\":\"join\",\"user\":\"" + userName + "\"}");
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
		DataList[ptr++] = JsonMapper.ToObject<Data> (e.Message );
		if( ptr >= MAX_LINES )
			ptr = 0;
		
	}

	void Awake ()
	{
		DontDestroyOnLoad (this);
		chatWindowRect = new Rect (20, Screen.height - 70, Screen.width - 40, 50);
		messWindowRect = new Rect (20, 70, Screen.width - 40, Screen.height - 70 - 80);
		
		// ユーザ名を取得
		CreateChatGui cc = FindObjectOfType (typeof(CreateChatGui)) as CreateChatGui;
		if (cc) {
			userName = cc.GetUser();
		}
		
		websocket = new WebSocket("ws://172.31.8.111:8888/");
		websocket.Opened += new EventHandler(websocket_Opened);
		websocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs >(websocket_Error);
		websocket.Closed += new EventHandler(websocket_Closed);
		websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(websocket_MessageReceived);
		websocket.Open();
	}
	
	void MakeChatWindow (int id)
	{
		GUILayout.Space (10);
		GUILayout.BeginHorizontal ();
		GUILayout.Space (10);
		stringToEdit = GUILayout.TextField (stringToEdit, 255, GUILayout.MinWidth (Screen.width - 120));
		if (GUILayout.Button ("送信")) {
			websocket.Send("{\"type\":\"chat\",\"user\":\"" + userName + "\",\"text\":\"" + stringToEdit + "\"}");
			stringToEdit = "";
		}
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
	}

	void MakeMessWindow (int id)
	{
		GUILayout.Space (10);
		for( int i=0, p=ptr; i<MAX_LINES; i++ )
		{
			p--;
			if( p < 0 )
				p = MAX_LINES-1;

			if( DataList[p] != null )
			{
				// pushされたメッセージを解釈し、要素を生成する
				if (DataList[p].type == "join") {
					GUILayout.Label (DataList[p].user + "が入室しました   " + DataList[p].time, style1, GUILayout.MinWidth (Screen.width - 120));
				} else if (DataList[p].type == "chat") {
					GUILayout.Label (DataList[p].user + " " + DataList[p].time, style2, GUILayout.MinWidth (Screen.width - 120));
					GUILayout.Label (DataList[p].text, style3, GUILayout.MinWidth (Screen.width - 120));
				} else if (DataList[p].type == "defect") {
					GUILayout.Label (DataList[p].user + "が退室しました   " + DataList[p].time, style4, GUILayout.MinWidth (Screen.width - 120));
				} else {
				}
			}
		}
	}
	
	void OnDestroy ()
	{
        websocket.Send("{\"type\":\"defect\",\"user\":\"" + userName + "\"}");
        websocket.Close();
	}
	
}

[System.Serializable]
public class Data {
		public string type = "";
		public string user = "";
		public string text = "";
		public string time = "";
}
	
