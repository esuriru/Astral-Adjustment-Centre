// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    // NOTE - Missing access specifiers, prefer: private
	[SerializeField] PlayerData playerData;
	[SerializeField] Transform orientation;
		
    // NOTE - In my opinion, I wouldn't shorten cam
	[Header("Crouch and Prone")]

    // NOTE - Put this below. It is not serialized
	private Camera cam;

    // NOTE - Missing access specifiers, prefer: private
	[SerializeField] GameObject capsule;
	[SerializeField] CapsuleCollider capsuleCollider;

    // NOTE - Put this below. It is not serialized
	private float playerHeight;

	[Header("Stair movement")]
    // NOTE - Missing access specifiers, prefer: private
	[SerializeField] GameObject stepRayUpper;
    [SerializeField] GameObject stepRayLower;
    [SerializeField] float stepSmooth = 3;

    // NOTE - Use a FSM
	private bool isCrouching;
	private bool isProning;
	private bool isGrounded;

    // NOTE - Unused isMoving bool
	private bool isMoving;
	private	Vector3 moveDirection;

    // NOTE - Put this above
	[SerializeField] private Transform groundCheck;
	private Vector3 slopeMoveDirection;

    // NOTE - Missing access specifiers, prefer: private
    // NOTE - Also, rather not shorten rigidbody to rb
	Rigidbody rb;

	RaycastHit slopeHit;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();

        // NOTE - Just set this in the editor
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
        // NOTE - Remove this line break below
		
	}

	private void Jump()
	{
		rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
		rb.AddForce(transform.up * playerData.jumpForce, ForceMode.Impulse);
	}

    // NOTE - Maybe make a FSM where then, the scale could be controlled by the
    // state instead of setting it in each function, and of course, not having
    // to use functions in the first place
	private void Crouch()
	{
		capsule.transform.localScale = new Vector3(capsule.transform.localScale.x, playerData.crouchScale, capsule.transform.localScale.z);
		rb.AddForce(Vector3.down * 6f, ForceMode.Impulse);
		
		isCrouching = true;

        // NOTE - One line if missing braces 
		if (isProning)
		isProning = false;
	}

	private void Prone()
	{
		capsule.transform.localScale = new Vector3(capsule.transform.localScale.x, playerData.proneScale, capsule.transform.localScale.z);
		rb.AddForce(Vector3.down * 9f, ForceMode.Impulse);

		isProning = true;

        // NOTE - One line if missing braces 
		if (isCrouching)
		isCrouching = false;
	}

	private void Stand()
	{
		capsule.transform.localScale = new Vector3(capsule.transform.localScale.x, playerData.standScale, capsule.transform.localScale.z);

        // NOTE - One line if missing braces 
		if (isCrouching)
		isCrouching = false;

        // NOTE - One line if missing braces 
		if (isProning)
		isProning = false;
	}

    // NOTE - Missing access specifier
	void ControlSpeed()
	{
		ScriptableBuff spdBuff = BuffManager.Instance.buffs[4];
        float spdBuffMultiplier;

        // NOTE - Use a ternary operator
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

    // NOTE - Missing access specifier
	void ControlDrag()
	{
        // NOTE - Use a ternary operator
		if (isGrounded)
		{
			rb.drag = playerData.groundDrag;
		}
		else
		{
			rb.drag = playerData.airDrag;
		}
	}

    // NOTE - Missing access specifier
	void MovePlayer()
	{
		if (isGrounded && !OnSlope())
		{
			rb.AddForce(moveDirection.normalized * playerData.moveSpeed, ForceMode.Acceleration);
		}
		else if (isGrounded && OnSlope())
		{
			rb.AddForce(slopeMoveDirection.normalized * playerData.moveSpeed, ForceMode.Acceleration);
            // NOTE - Remove debug code 
			// Debug.Log(slopeMoveDirection);
		}
		else if (!isGrounded)
		{
            // NOTE - Reorder operands 
			rb.AddForce(moveDirection.normalized * playerData.moveSpeed * playerData.airMultiplier, ForceMode.Acceleration);
		}
	}

	private bool OnSlope()
	{
        // NOTE - This entire function can be one line. It's not like you are
        // going to read it again

		// If there is something under the player
        // NOTE - Hardcoded height/leeway, not sure (0.6f)
		if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.6f))
		{
			// If it is a slope
            // NOTE - Just return this condition
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
        // NOTE - Inline the declaration
		RaycastHit hitLower;
        // NOTE - Combine these if statements into one. Also, discard the
        // raycast hits.
        if (Physics.Raycast(stepRayLower.transform.position, orientation.transform.forward, out hitLower, 0.1f))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepRayUpper.transform.position, orientation.transform.transform.forward, out hitUpper, 0.2f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }

        // NOTE - Same as above, also what does the 45 mean?
		RaycastHit hitLower45;
		if (Physics.Raycast(stepRayLower.transform.position, orientation.transform.forward + orientation.transform.right, out hitLower45, 0.1f))
        {
            RaycastHit hitUpper45;
            if (!Physics.Raycast(stepRayUpper.transform.position, orientation.transform.forward + orientation.transform.right, out hitUpper45, 0.2f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }

        // NOTE - Same as above, also what does the 45 mean?
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
