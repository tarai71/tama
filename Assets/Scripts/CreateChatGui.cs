using UnityEngine;
using System.Collections;

public class CreateChatGui : MonoBehaviour
{

	public GameObject authGUI;
	public GameObject chatGUI;
	private Object gm = null;
	private bool enable = false;
	private bool loged = false;
	
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.RightControl)) {
			enable ^= true;
			SetLog (loged);
		}
	}

	public void SetLog (bool log) {
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
}
