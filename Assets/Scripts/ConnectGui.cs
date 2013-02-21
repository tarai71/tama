using UnityEngine;
using System.Collections;


using System;
using WebSocket4Net;

public class ConnectGui : MonoBehaviour
{

	public string remoteIP = "127.0.0.1";
	public int remotePort = 25001;
	public int listenPort = 25000;
	public int remoteGUID = 0;
//	private string connectionInfo = "";
		public string gameTypeName = "";

WebSocket websocket;
private void websocket_Opened(object sender, EventArgs e)
{
     websocket.Send("{\"type\":\"join\",\"user\":\"ゲスト00\",\"time\":\"2013-2-20 18:49:24\"}");
//     websocket.Send("Hello World!");
}
	void Awake ()
	{
 
websocket = new WebSocket("ws://172.31.8.50:8888/");
websocket.Opened += new EventHandler(websocket_Opened);
//websocket.Error += new EventHandler<ErrorEventArgs>(websocket_Error);
//websocket.Closed += new EventHandler(websocket_Closed);
//websocket.MessageReceived += new EventHandler(websocket_MessageReceived);
websocket.Open();
	}

	void OnGUI ()
	{
		GUILayout.Space (10);
		GUILayout.BeginHorizontal ();
		GUILayout.Space (10);
		if (Network.peerType == NetworkPeerType.Disconnected) {
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Space (10);
			
			GUILayout.BeginVertical ();
			if (GUILayout.Button ("Connect")) {
				Network.Connect (remoteIP, remotePort);
			}
			if (GUILayout.Button ("Start Server")) {
				Network.InitializeServer (32, listenPort, false);
				// Notify our objects that the level and the network is ready
				foreach (GameObject go in FindObjectsOfType (typeof(GameObject)))
					go.SendMessage ("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
websocket.Send("{\"type\":\"chat\",\"user\":\"ゲスト00\",\"text\":\"Start Server\",\"time\":\"2013-2-20 18:49:24\"}");
			}
			GUILayout.EndVertical ();
			remoteIP = GUILayout.TextField (remoteIP, GUILayout.MinWidth (100));
			remotePort = int.Parse (GUILayout.TextField (remotePort.ToString ()));
		} else {
			GUILayout.Label ("Local IP/port: " + Network.player.ipAddress + "/" + Network.player.port);
			GUILayout.Label (" - External IP/port: " + Network.player.externalIP + "/" + Network.player.externalPort);
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Disconnect"))
				Network.Disconnect (200);
		}
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
	}

	void OnServerInitialized ()
	{
		Debug.Log ("==> Local IP/port is " + Network.player.ipAddress + "/" + Network.player.port + ". Use this on clients to connect directly.");
	}

	void OnConnectedToServer ()
	{
		// Notify our objects that the level and the network is ready
		foreach (GameObject go in FindObjectsOfType (typeof(GameObject)))
			go.SendMessage ("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
	}

	void OnDisconnectedFromServer ()
	{
		if (this.enabled != false) {
			Application.LoadLevel (Application.loadedLevel);
websocket.Close();
		} else {
			NetworkLevelLoad n = (NetworkLevelLoad)FindObjectOfType (typeof(NetworkLevelLoad));
			n.OnDisconnectedFromServer ();
		}
	}
}
