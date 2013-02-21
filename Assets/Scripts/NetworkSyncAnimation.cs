using UnityEngine;
using System.Collections;
using System;

public class NetworkSyncAnimation : MonoBehaviour
{

	public enum AniStates
	{
		idle = 0,
		run,
		walk,
		jump_pose
	}

	public AniStates currentAnimation = AniStates.idle;
	public AniStates lastAnimation = AniStates.idle;

	public void SyncAnimation (String animationValue)
	{
		currentAnimation = (AniStates)Enum.Parse (typeof(AniStates), animationValue);
		if (gameObject.name.Contains ("Remote")) {
			print ("SyncAnimation : " + animationValue);
			print ("currentAnimation : " + currentAnimation);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		
		if (lastAnimation != currentAnimation) {
			lastAnimation = currentAnimation;
			animation.CrossFade (Enum.GetName (typeof(AniStates), currentAnimation));
			animation["run"].normalizedSpeed = 1.0f;
			animation["walk"].normalizedSpeed = 1.0f;
		}
	}

	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting) {
			char ani = (char)currentAnimation;
			stream.Serialize (ref ani);
		} else {
			char ani = (char)0;
			stream.Serialize (ref ani);
			
			currentAnimation = (AniStates)ani;
		}
		
	}
	
}
