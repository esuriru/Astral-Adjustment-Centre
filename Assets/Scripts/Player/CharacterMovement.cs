using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
	[SerializeField] PlayerData playerData;
	[SerializeField] Transform orientation;
		
	[Header("Crouch and Prone")]
	private Camera cam;
	[SerializeField] GameObject capsule;
	[SerializeField] CapsuleCollider capsuleCollider;
	private float playerHeight;

	[Header("Stair movement")]
	[SerializeField] GameObject stepRayUpper;
    [SerializeField] GameObject stepRayLower;
    [SerializeField] float stepSmooth = 3;

	private bool isCrouching;
	private bool isProning;
	private bool isGrounded;
	private bool isMoving;
	private	Vector3 moveDirection;

	[SerializeField] private Transform groundCheck;
	private Vector3 slopeMoveDirection;

	Rigidbody rb;

	RaycastHit slopeHit;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;

		playerHeight = transform.localScale.y;

		cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}

	private void Update()
	{
		isGrounded = Physics.CheckSphere(groundCheck.position, playerData.groundDistance, playerData.groundMask);

		PlayerInput();
		ControlDrag();
		ControlSpeed();
		ClimbStep();

		// Get the perpendicular angle of the plane
		slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
	}

	private void FixedUpdate()
	{
		MovePlayer();
	}

	public void PlayerInput()
	{
		float horizontalInput = Input.GetAxisRaw("Horizontal");
		float verticalInput = Input.GetAxisRaw("Vertical");

		// Jump input
		if (Input.GetKeyDown(playerData.jumpKey) && isGrounded)
		{
			Jump();
		}
		// Crouch input
		if (Input.GetKeyDown(playerData.crouchKey) && isGrounded && !isProning)
		{
			Crouch();
		}
		if (Input.GetKeyDown(playerData.proneKey) && isGrounded && !isCrouching)
		{
			Prone();
		}
		// Stand input
		if ((Input.GetKeyUp(playerData.crouchKey) && isCrouching) || Input.GetKeyUp(playerData.proneKey) && isProning)
		{
			Stand();
		}

		moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

		isMoving = (horizontalInput != 0 || verticalInput != 0) && isGrounded;
		
	}

	private void Jump()
	{
		rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
		rb.AddForce(transform.up * playerData.jumpForce, ForceMode.Impulse);
	}

	private void Crouch()
	{
		capsule.transform.localScale = new Vector3(capsule.transform.localScale.x, playerData.crouchScale, capsule.transform.localScale.z);
		rb.AddForce(Vector3.down * 6f, ForceMode.Impulse);
		
		isCrouching = true;

		if (isProning)
		isProning = false;
	}

	private void Prone()
	{
		capsule.transform.localScale = new Vector3(capsule.transform.localScale.x, playerData.proneScale, capsule.transform.localScale.z);
		rb.AddForce(Vector3.down * 9f, ForceMode.Impulse);

		isProning = true;

		if (isCrouching)
		isCrouching = false;
	}

	private void Stand()
	{
		capsule.transform.localScale = new Vector3(capsule.transform.localScale.x, playerData.standScale, capsule.transform.localScale.z);

		if (isCrouching)
		isCrouching = false;

		if (isProning)
		isProning = false;
	}

	void ControlSpeed()
	{
		ScriptableBuff spdBuff = BuffManager.Instance.buffs[4];
        float spdBuffMultiplier;
        if (spdBuff.currBuffTier > 0)
        {
            spdBuffMultiplier = spdBuff.buffBonus[spdBuff.currBuffTier - 1];
        }
        else
        {
            spdBuffMultiplier = 1;
        }

        if (Input.GetKey(playerData.sprintKey) && isGrounded)	
		{
			playerData.moveSpeed = Mathf.Lerp(playerData.moveSpeed, playerData.sprintSpeed * spdBuffMultiplier, playerData.acceleration * Time.deltaTime);
		}
		else if (Input.GetKey(playerData.crouchKey) && isGrounded)
		{
			playerData.moveSpeed = Mathf.Lerp(playerData.moveSpeed, playerData.crouchSpeed * spdBuffMultiplier, playerData.acceleration * Time.deltaTime);
		}
		else if (Input.GetKey(playerData.proneKey) && isGrounded)
		{
			playerData.moveSpeed = Mathf.Lerp(playerData.moveSpeed, playerData.proneSpeed * spdBuffMultiplier, playerData.acceleration * Time.deltaTime);
		}
		else
		{
			playerData.moveSpeed = Mathf.Lerp(playerData.moveSpeed, playerData.walkSpeed * spdBuffMultiplier, playerData.acceleration * Time.deltaTime);
		}
	}

	void ControlDrag()
	{
		if (isGrounded)
		{
			rb.drag = playerData.groundDrag;
		}
		else
		{
			rb.drag = playerData.airDrag;
		}
	}

	void MovePlayer()
	{
		if (isGrounded && !OnSlope())
		{
			rb.AddForce(moveDirection.normalized * playerData.moveSpeed, ForceMode.Acceleration);
		}
		else if (isGrounded && OnSlope())
		{
			rb.AddForce(slopeMoveDirection.normalized * playerData.moveSpeed, ForceMode.Acceleration);
			// Debug.Log(slopeMoveDirection);
		}
		else if (!isGrounded)
		{
			rb.AddForce(moveDirection.normalized * playerData.moveSpeed * playerData.airMultiplier, ForceMode.Acceleration);
		}
	}

	private bool OnSlope()
	{
		// If there is something under the player
		if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.6f))
		{
			// If it is a slope
			if (slopeHit.normal != Vector3.up)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		return false;
	}

	private void ClimbStep()
    {
		RaycastHit hitLower;
        if (Physics.Raycast(stepRayLower.transform.position, orientation.transform.forward, out hitLower, 0.1f))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepRayUpper.transform.position, orientation.transform.transform.forward, out hitUpper, 0.2f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }

		RaycastHit hitLower45;
		if (Physics.Raycast(stepRayLower.transform.position, orientation.transform.forward + orientation.transform.right, out hitLower45, 0.1f))
        {
            RaycastHit hitUpper45;
            if (!Physics.Raycast(stepRayUpper.transform.position, orientation.transform.forward + orientation.transform.right, out hitUpper45, 0.2f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }

        RaycastHit hitLowerMinus45;
        if (Physics.Raycast(stepRayLower.transform.position, orientation.transform.forward - orientation.transform.right, out hitLowerMinus45, 0.1f))
        {

            RaycastHit hitUpperMinus45;
            if (!Physics.Raycast(stepRayUpper.transform.position, orientation.transform.forward - orientation.transform.right, out hitUpperMinus45, 0.2f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }
    }
}
