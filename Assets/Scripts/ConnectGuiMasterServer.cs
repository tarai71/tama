using UnityEngine;
using System.Collections;

public class ConnectGuiMasterServer : MonoBehaviour
{
	public string gameTypeName = "ゲームタイプ";
	public string gameName = "ゲーム名";
	public string comment = "コメント";
	private int serverPort = 25002;

	public string ipAddress = "172.31.8.74";
	public int port = 23466;
	private float lastHostListRequest = -1000.0f;
	private float hostListRefreshTimeout = 10.0f;
	private bool filterNATHosts = false;
	private bool useNat = false;
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
		windowRect = GUILayout.Window (0, windowRect, MakeWindow, "サーバーコンソール");
		if (Network.peerType == NetworkPeerType.Disconnected && MasterServer.PollHostList ().Length != 0)
			serverListRect = GUILayout.Window (1, serverListRect, MakeClientWindow, "サーバーリスト");
	}

	void Awake ()
	{
		MasterServer.ipAddress = ipAddress;
		MasterServer.port = port;
		// サーバーを立てたプレイヤーを人数に加えたくない場合trueにしてください
		//MasterServer.dedicatedServer = true;
		DontDestroyOnLoad (this);
		windowRect = new Rect (Screen.width - 220, 0, 200, 50);
		serverListRect = new Rect (0, 70, Screen.width, 100);
		
		//所有しているIPアドレスを確認します
		if (Network.HavePublicAddress ())
			Debug.Log ("パブリックIPアドレスを所有しています");
		else
			Debug.Log ("プライベートIPアドレスを所有しています");
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
}
