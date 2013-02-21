using UnityEngine;
using System.Collections;

[AddComponentMenu("Third Person Player/Third Person Player Animation")]
public class ThirdPersonSimpleAnimation : MonoBehaviour
{

	public float runSpeedScale = 1.0f;
	public float walkSpeedScale = 1.0f;
	public Transform torso;
	private NetworkSyncAnimation networkSyncAnimation;
	void Awake ()
	{
		animation.Stop ();
		animation.Play ("idle");
	}

	void Update ()
	{
		ThirdPersonController marioController = GetComponent<ThirdPersonController> ();
		float currentSpeed = marioController.GetSpeed ();
		 networkSyncAnimation = GetComponent<NetworkSyncAnimation> ();
		
		// Fade in run
		if (currentSpeed > marioController.walkSpeed) {
			animation.CrossFade ("run");
			animation.wrapMode = WrapMode.Loop;
			networkSyncAnimation.SendMessage ("SyncAnimation", "run");
		// Fade in walk
		} else if (currentSpeed > 0.1f) {
			animation.CrossFade ("walk");
			animation.wrapMode = WrapMode.Loop;
			networkSyncAnimation.SendMessage ("SyncAnimation", "walk");
		// Fade in idle
		} else {
			animation.CrossFade ("idle");
			animation.wrapMode = WrapMode.Loop;
			networkSyncAnimation.SendMessage ("SyncAnimation", "idle");
		}
		
		animation["run"].normalizedSpeed = runSpeedScale;
		animation["walk"].normalizedSpeed = walkSpeedScale;
		
		if (marioController.IsJumping ()) {
			animation. CrossFade ("jump_pose");
			networkSyncAnimation.SendMessage ("SyncAnimation", "jump_pose");
			animation.wrapMode = WrapMode.ClampForever;
		}
	}

	public void DidLand ()
	{
		animation.Play ("idle");
		networkSyncAnimation.SendMessage ("SyncAnimation", "idle");
	}
}
