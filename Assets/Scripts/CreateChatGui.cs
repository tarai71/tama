using UnityEngine;
using System.Collections;

public class CreateChatGui : MonoBehaviour
{

	public GameObject authGUI;
	public GameObject chatGUI;
	private Object gm = null;
	private bool enable = false;
	private bool loged = false;
	private string userid = "";
	
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.RightControl)) {
			enable ^= true;
			SetLog (userid, loged);
		}
	}

	public void SetLog (string user, bool log) {
		userid = user;
		loged = log;
		if (loged) {
			if (enable) {
				if (gm) Destroy (gm);
				gm = Instantiate (chatGUI, Vector3.zero, Quaternion.identity);
			} else {
				Destroy (gm);
			}
		} else {
			if (enable) {
				if (gm) Destroy (gm);
				gm = Instantiate (authGUI, Vector3.zero, Quaternion.identity);
			} else {
				Destroy (gm);
			}
		}
	}
	
	public string GetUser () {
		return userid;
	}
}
