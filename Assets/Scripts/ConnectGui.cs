using UnityEngine;
using System.Collections;

public class ConnectGui : MonoBehaviour
{

	public string remoteIP = "127.0.0.1";
	public int remotePort = 25001;
	public int listenPort = 25000;
	public int remoteGUID = 0;
//	private string connectionInfo = "";
		public string gameTypeName = "";

	void Awake ()
	{
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
		} else {
			NetworkLevelLoad n = (NetworkLevelLoad)FindObjectOfType (typeof(NetworkLevelLoad));
			n.OnDisconnectedFromServer ();
		}
	}
}
