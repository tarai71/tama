using UnityEngine;
using System.Collections;

public class ThirdPersonAction : MonoBehaviour
{
	public GameObject BallPrefab;

	void Update ()
	{
		if (networkView.isMine) {
			CreateBall ();
		}
	}
	void CreateBall ()
	{
		if (Input.GetKeyDown ("z"))
			Network.Instantiate (BallPrefab, transform.position + transform.forward * 2 + transform.up * 0.5f, Quaternion.identity, 0);
	}
	
	
	
	void OnControllerColliderHit (ControllerColliderHit hit)
	{
		float pushPower = 2.0f;
		Rigidbody body = hit.collider.attachedRigidbody;
		if (body == null || body.isKinematic)
			return;
		
		if (hit.moveDirection.y < -0.3f)
			return;
		
		Vector3 pushDir = new Vector3 (hit.moveDirection.x, 0, hit.moveDirection.z);
		body.velocity = pushDir * pushPower;
	}
}
