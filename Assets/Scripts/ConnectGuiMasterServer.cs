using UnityEngine;
using System.Collections;

using System;
using WebSocket4Net;
using LitJson;

public class ConnectGuiMasterServer : MonoBehaviour
{

	public string gameTypeName = "ゲームタイプ";
	public string gameName = "ゲーム名";
	public string comment = "コメント";
	private int serverPort = 25002;

	public string ipAddress = "172.31.8.50";
	public int port = 23466;
	private float lastHostListRequest = -1000.0f;
	private float hostListRefreshTimeout = 10.0f;
	private bool filterNATHosts = false;
	private bool useNat = false;
	private Rect chatWindowRect;
	private Rect windowRect;
	private Rect serverListRect;




	void OnFailedToConnectToMasterServer (NetworkConnectionError info)
	{
		Debug.Log ("OnFailedToConnectToMasterServer : "+info);
	}

	void OnFailedToConnect (NetworkConnectionError info)
	{
		Debug.Log ("OnFailedToConnect : "+info);
	}

	void OnGUI ()
	{
chatWindowRect = GUILayout.Window (2, chatWindowRect, MakeChatWindow, "チャットコンソール");
		windowRect = GUILayout.Window (0, windowRect, MakeWindow, "サーバーコンソール");
		if (Network.peerType == NetworkPeerType.Disconnected && MasterServer.PollHostList ().Length != 0)
			serverListRect = GUILayout.Window (1, serverListRect, MakeClientWindow, "サーバーリスト");
	}

WebSocket websocket;
private void websocket_Opened(object sender, EventArgs e)
{
    websocket.Send("{\"type\":\"join\",\"user\":\"ゲスト00\"}");
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
  Data data = JsonMapper.ToObject<Data> (e.Message );
/*
  var data = JSON.parse(event.data);
  var item = $('<li/>').append(
    $('<div/>').append(
      $('<i/>').addClass('icon-user'),
      $('<small/>').addClass('meta chat-time').append(data.time))
  );

  // pushされたメッセージを解釈し、要素を生成する
  if (data.type === 'join') {
    item.addClass('alert alert-info')
    .prepend('<button type="button" class="close" data-dismiss="alert">×</button>')
    .children('div').children('i').after(data.user + 'が入室しました');
  } else if (data.type === 'chat') {
    item.addClass('well well-small')
    .append($('<div/>').text(data.text))
    .children('div').children('i').after(data.user);
  } else if (data.type === 'defect') {
    item.addClass('alert')
    .prepend('<button type="button" class="close" data-dismiss="alert">×</button>')
    .children('div').children('i').after(data.user + 'が退室しました');
  } else {
    item.addClass('alert alert-error')
    .children('div').children('i').removeClass('icon-user').addClass('icon-warning-sign')
      .after('不正なメッセージを受信しました');
  }
  $('#chat-history').prepend(item).hide().fadeIn(500);
 */
}

	void Awake ()
	{
		MasterServer.ipAddress = ipAddress;
		MasterServer.port = port;
		// サーバーを立てたプレイヤーを人数に加えたくない場合trueにしてください
		//MasterServer.dedicatedServer = true;
		DontDestroyOnLoad (this);
chatWindowRect = new Rect (20, Screen.height - 70, Screen.width - 40, 50);
		windowRect = new Rect (Screen.width - 220, 0, 200, 50);
		serverListRect = new Rect (0, 70, Screen.width, 100);
		
		//所有しているIPアドレスを確認します
		if (Network.HavePublicAddress ())
			Debug.Log ("パブリックIPアドレスを所有しています");
		else
			Debug.Log ("プライベートIPアドレスを所有しています");
websocket = new WebSocket("ws://172.31.8.50:8888/");
websocket.Opened += new EventHandler(websocket_Opened);
websocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs >(websocket_Error);
websocket.Closed += new EventHandler(websocket_Closed);
websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(websocket_MessageReceived);
websocket.Open();
}
	void MakeWindow (int id)
	{
		GUILayout.Space (10);
		if (Network.peerType == NetworkPeerType.Disconnected) {
			GUILayout.BeginHorizontal ();
			GUILayout.Space (10);
			
			if (GUILayout.Button ("サーバー起動")) {
				Network.InitializeServer (32, serverPort, useNat);
				MasterServer.RegisterHost (gameTypeName, gameName, comment);
			}
			
			if (GUILayout.Button ("リフレッシュ") || Time.realtimeSinceStartup > lastHostListRequest + hostListRefreshTimeout) {
				MasterServer.RequestHostList (gameTypeName);
				lastHostListRequest = Time.realtimeSinceStartup;
			}
			
			GUILayout.FlexibleSpace ();
			
			GUILayout.EndHorizontal ();
		} else {
			if (GUILayout.Button ("切断")) {
				Network.Disconnect ();
				MasterServer.UnregisterHost ();
			}
			GUILayout.FlexibleSpace ();
		}
		GUI.DragWindow (new Rect (0, 0, 1000, 1000));
	}

	void MakeClientWindow (int id)
	{
		GUILayout.Space (5);
		
		HostData[] data = MasterServer.PollHostList ();
		foreach (HostData element in data) {
			
			
			if (!(filterNATHosts && element.useNat)) {
				GUILayout.BeginHorizontal ();
				var connections = element.connectedPlayers + "/" + element.playerLimit;
				GUILayout.Label (element.gameName);
				GUILayout.Space (5);
				GUILayout.Label (connections);
				GUILayout.Space (5);
				var hostInfo = "";
				
				//全てのIPアドレスが表示されます。
				//マスターサーバーと内部LANで接続されている場合には複数IPアドレスが表示される場合があります。
				//Unityではこの全てのIPアドレスをチェックするようになっていて最初に有効だと判断したものに接続します。
				foreach (string host in element.ip)
					hostInfo = hostInfo + host + ":" + element.port + " ";
				
				//GUILayout.Label("[" + element.ip + ":" + element.port + "]");	
				
				GUILayout.Space (5);
				GUILayout.Label (element.comment);
				GUILayout.Space (5);
				GUILayout.FlexibleSpace ();
				if (GUILayout.Button ("接続"))
					Network.Connect (element);
				GUILayout.EndHorizontal ();
				GUILayout.Label (hostInfo);
			}
			
			
		}
	}
	
	void MakeChatWindow (int id)
	{
		GUILayout.Space (10);
		GUILayout.BeginHorizontal ();
		GUILayout.Space (10);
		string stringToEdit = "今なにしてる？";
		stringToEdit = GUILayout.TextField (stringToEdit, 10, GUILayout.MinWidth (Screen.width - 120));
		if (GUILayout.Button ("送信")) {
			websocket.Send("{\"type\":\"chat\",\"user\":\"ゲスト00\",\"text\":\"" + stringToEdit + "\"}");
		}
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
	}

	void OnDestroy ()
	{
        websocket.Send("{\"type\":\"defect\",\"user\":\"ゲスト00\"}");
        websocket.Close();
	}
	
}

[System.Serializable]
public class Data {
		public string type;
		public string user;
		public string text;
		public string time;
}
	
