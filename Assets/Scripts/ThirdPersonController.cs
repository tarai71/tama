using UnityEngine;
using System.Collections;

// Require a character controller to be attached to the same game object
[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Third Person Player/Third Person Controller")]
public class ThirdPersonController : MonoBehaviour
{

	// The speed when walking
	public float walkSpeed = 3.0f;
	// The speed when Runing
	public float runSpeed = 6.0f;

// How high do we jump when pressing jump and letting go immediately
	public float jumpHeight = 0.5f;

// The gravity for the character
	public float gravity = 20.0f;

	public float speedSmoothing = 10.0f;
	public float rotateSpeed = 500.0f;

	public bool canJump = true;

	private float jumpRepeatTime = 0.05f;
	private float jumpTimeout = 0.15f;
	private float groundedTimeout = 0.25f;

// The current move direction in x-z
	private Vector3 moveDirection = Vector3.zero;
// The current vertical speed
	private float verticalSpeed = 0.0f;
// The current x-z move speed
	private float moveSpeed = 0.0f;

// The last collision flags returned from controller.Move
	private CollisionFlags collisionFlags;

// Are we jumping? (Initiated with jump button and not grounded yet)
	private bool jumping = false;

// Are we moving backwards (This locks the camera to not do a 180 degree spin)
	private bool movingBack = false;
// Is the user pressing any keys?
	public bool isMoving = false;
// Last time the jump button was clicked down
	private float lastJumpButtonTime = -10.0f;
// Last time we performed a jump
	private float lastJumpTime = -1.0f;

	private float lastGroundedTime = 0.0f;

// The vertical/horizontal input axes and jump button from user input, synchronized over network
	public float verticalInput;
	public float horizontalInput;
	public bool jumpButton;

	public bool getUserInput = true;

	private ThirdPersonSimpleAnimation thirdPersonSimpleAnimation;
	void Awake ()
	{
		//moveDirection = transform.TransformDirection(Vector3.forward);
		thirdPersonSimpleAnimation = GetComponent<ThirdPersonSimpleAnimation> ();
	}

	void UpdateSmoothedMovementDirection ()
	{
		Transform cameraTransform = Camera.main.transform;
		bool grounded = IsGrounded ();
		
		// Forward vector relative to the camera along the x-z plane	
		Vector3 forward = cameraTransform.TransformDirection (Vector3.forward);
		forward.y = 0;
		forward = forward.normalized;
		
		// Right vector relative to the camera
		// Always orthogonal to the forward vector
		Vector3 right = new Vector3 (forward.z, 0, -forward.x);
		
		if (getUserInput) {
			verticalInput = Input.GetAxisRaw ("Vertical");
			horizontalInput = Input.GetAxisRaw ("Horizontal");
		}
		
		// Are we moving backwards or looking backwards
		if (verticalInput < -0.2f)
			movingBack = true;
		else
			movingBack = false;
		
		isMoving = Mathf.Abs (horizontalInput) > 0.1f || Mathf.Abs (verticalInput) > 0.1f;
		
		// Target direction relative to the camera
		Vector3 targetDirection = horizontalInput * right + verticalInput * forward;
		
		// Grounded controls
		if (grounded) {
			// We store speed and direction seperately,
			// so that when the character stands still we still have a valid forward direction
			// moveDirection is always normalized, and we only update it if there is user input.
			if (targetDirection != Vector3.zero) {
				// If we are really slow, just snap to the target direction
				if (moveSpeed < walkSpeed * 0.9f && grounded) {
					moveDirection = targetDirection.normalized;
					// Otherwise smoothly turn towards it
				} else {
					moveDirection = Vector3.RotateTowards (moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
					
					moveDirection = moveDirection.normalized;
				}
			}
			
			// Smooth the speed based on the current target direction
			float curSmooth = speedSmoothing * Time.deltaTime;
			
			// Choose target speed
			//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
			float targetSpeed = Mathf.Min (targetDirection.magnitude, 1.0f);
			
			// Pick speed modifier
			if (Input.GetButton ("Fire2")) {
				targetSpeed *= runSpeed;
			} 
			
			moveSpeed = Mathf.Lerp (moveSpeed, targetSpeed, curSmooth);
			
			// In air controls
		}
	}

	void ApplyJumping ()
	{
		// Prevent jumping too fast after each other
		if (lastJumpTime + jumpRepeatTime > Time.time)
			return;
		
		if (IsGrounded ()) {
			// Jump
			// - Only when pressing the button down
			// - With a timeout so you can press the button slightly before landing		
			if (canJump && Time.time < lastJumpButtonTime + jumpTimeout) {
				verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
				DidJump ();
			}
		}
	}

	void ApplyGravity ()
	{
		verticalSpeed -= gravity * Time.deltaTime;
	}

	float CalculateJumpVerticalSpeed (float targetJumpHeight)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt (2 * targetJumpHeight * gravity);
	}

	void DidJump ()
	{
		jumping = true;
		lastJumpTime = Time.time;
		lastJumpButtonTime = -10;
	}

	void Update ()
	{
		
		if (getUserInput) {
			if (Input.GetButtonDown ("Jump"))
				lastJumpButtonTime = Time.time;
		} else {
			if (jumpButton)
				lastJumpButtonTime = Time.time;
		}
		
		
		UpdateSmoothedMovementDirection ();
		
		// Apply gravity
		// - extra power jump modifies gravity
		ApplyGravity ();
		
		// Apply jumping logic
		ApplyJumping ();
		
		// Calculate actual motion
		Vector3 movement = moveDirection * moveSpeed + new Vector3 (0, verticalSpeed, 0);
		movement *= Time.deltaTime;
		
		// Move the controller
		CharacterController controller = GetComponent<CharacterController> ();
		collisionFlags = controller.Move (movement);
		
		// Set rotation to the move direction
		if (IsGrounded () && moveDirection != Vector3.zero) {
			transform.rotation = Quaternion.LookRotation (moveDirection);
		} else {
			Vector3 xzMove = movement;
			xzMove.y = 0;
			if (xzMove.magnitude > 0.001f) {
				transform.rotation = Quaternion.LookRotation (xzMove);
			}
		}
		
		// We are in jump mode but just became grounded
		if (IsGrounded ()) {
			lastGroundedTime = Time.time;
			if (jumping) {
				jumping = false;
				thirdPersonSimpleAnimation.SendMessage ("DidLand", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	void OnControllerColliderHit (ControllerColliderHit hit)
	{
		if (hit.moveDirection.y > 0.01f)
			return;
	}

	public float GetSpeed ()
	{
		return moveSpeed;
	}

	public bool IsJumping ()
	{
		return jumping;
	}

	public bool IsGrounded ()
	{
		return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
	}

	public Vector3 GetDirection ()
	{
		return moveDirection;
	}

	public bool IsMovingBackwards ()
	{
		return movingBack;
	}

	public bool IsGroundedWithTimeout ()
	{
		return lastGroundedTime + groundedTimeout > Time.time;
	}

	void Reset ()
	{
		gameObject.tag = "Player";
	}
}
