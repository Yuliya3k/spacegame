using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerControllerCode : MonoBehaviour
{
    public static PlayerControllerCode instance; // Singleton pattern

    Vector2 moveDirection;
    float jumpDirection;

    private PlayerInputActions inputActions;

    


    public float baseMaxForwardSpeed = 4f; // Base movement speed
    public float sprintMaxForwardSpeed = 8f; // Sprint movement speed

    private bool isSprinting = false; // Tracks if the player is sprinting

    public float rotationSpeed = 180f; // degrees per second
    private bool isControlEnabled = true;

    float desiredSpeed;
    float forwardSpeed;
    float jumpSpeed = 30000f;

    const float groundAccel = 5;
    const float groundDecel = 25;

    Rigidbody rb;

    bool onGround = true;

    Animator anim;

    [SerializeField] private Transform cameraTransform;

    [SerializeField] public float walkRotationOffset = 10f;
    [SerializeField] public float runRotationOffset = 5f;

    private CharacterStats characterStats;

    public VirtualJoystick virtualJoystick; // Assign in Inspector

    bool isMoveInput
    {
        get { return !Mathf.Approximately(moveDirection.sqrMagnitude, 0f); }
    }

    [Header("Footstep Audio")]
    public List<AudioClip> walkFootstepClips; // List of walking footstep sounds
    public List<AudioClip> runFootstepClips;  // List of running footstep sounds
    public AudioSource footstepAudioSource;   // Audio source to play footstep sounds

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure there's only one instance
        }

        inputActions = new PlayerInputActions();

        // Register input action callbacks
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Jump.canceled += OnJump;

        inputActions.Player.Sprint.performed += OnSprint;
        inputActions.Player.Sprint.canceled += OnSprint;

    }



    public void OnMove(InputAction.CallbackContext context)
    {
        if (isControlEnabled)
        {
            moveDirection = context.ReadValue<Vector2>();
        }
        else
        {
            moveDirection = Vector2.zero;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (isControlEnabled)
        {
            jumpDirection = context.ReadValue<float>();
        }
    }

    // Method to handle sprint input
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (isControlEnabled)
        {
            isSprinting = context.ReadValueAsButton();
        }
    }

    void Move(Vector2 direction)
    {
        if (!isControlEnabled) return; // Prevent movement when controls are disabled

        // Convert the move direction to be relative to the camera's forward and right vectors
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Keep movement only in the horizontal plane (ignore the Y axis)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // Determine the final move direction based on input and camera direction
        Vector3 desiredMoveDirection = forward * direction.y + right * direction.x;

        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        // Adjust the desired speed based on sprinting
        float baseSpeed = isSprinting ? sprintMaxForwardSpeed : baseMaxForwardSpeed;

        // 3) Ask CharacterStats for a multiplier
        float multiplier = 1f;
        bool canSprintByWeight = true;

        if (characterStats != null)
        {
            multiplier = characterStats.GetWeightSpeedMultiplier();
            canSprintByWeight = characterStats.CanSprintBasedOnWeight();
        }

        // 4) If the user is trying to sprint but we are too heavy, disable sprint
        if (!canSprintByWeight && isSprinting)
        {
            isSprinting = false;
            baseSpeed = baseMaxForwardSpeed; // revert to normal
        }

        // 5) Final speed 
        float speed = baseSpeed * multiplier;


        desiredSpeed = direction.magnitude * speed;



        float acceleration = isMoveInput ? groundAccel : groundDecel;

        forwardSpeed = Mathf.MoveTowards(forwardSpeed, desiredSpeed, acceleration * Time.deltaTime);

        // Apply movement based on the calculated direction
        anim.SetFloat("ForwardSpeed", forwardSpeed);

        //// After calculating forwardSpeed and desiredMoveDirection:
        //if (desiredMoveDirection.sqrMagnitude > 0.0001f) // Prevent jitter if direction is zero
        //{
        //    // Move the character forward according to forwardSpeed
        //    transform.position += desiredMoveDirection.normalized * forwardSpeed * Time.deltaTime;
        //}

        if (desiredMoveDirection != Vector3.zero)
        {
            // Choose the appropriate rotation offset based on sprinting
            float currentRotationOffset = isSprinting ? runRotationOffset : walkRotationOffset;

            // Apply the rotational offset to the character's orientation
            Quaternion offsetRotation = Quaternion.Euler(0, currentRotationOffset, 0);
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection) * offsetRotation;

            // Calculate the rotation step
            float rotationStep = rotationSpeed * Time.deltaTime;

            // Rotate towards the target rotation smoothly
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationStep);
        }

        if (characterStats != null)
        {
            characterStats.SetMovementState(isMoveInput, isSprinting);
        }
    }

    bool readyJump = false;
    float jumpEffort = 0;

    void Jump(float direction)
    {
        if (!isControlEnabled) return; // Prevent jumping when controls are disabled

        if (direction > 0 && onGround)
        {
            anim.SetBool("ReadyJump", true);
            readyJump = true;
            jumpEffort += Time.deltaTime;
        }
        else if (readyJump)
        {
            anim.SetBool("Launch", true);
            readyJump = false;
            anim.SetBool("ReadyJump", false);
        }
    }

    public void Launch()
    {
        rb.AddForce(0, jumpSpeed * Mathf.Clamp(jumpEffort, 1, 3), 0);
        anim.SetBool("Launch", false);
        anim.applyRootMotion = false;
    }

    public void Land()
    {
        anim.SetBool("Land", false);
        anim.applyRootMotion = true;
        anim.SetBool("Launch", false);
        jumpEffort = 0;
    }

    void Start()
    {
        anim = this.GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        characterStats = GetComponent<CharacterStats>();
        if (characterStats == null)
        {
            Debug.LogError("CharacterStats component not found on the player.");
        }

        // Initialize Footstep AudioSource
        if (footstepAudioSource == null)
        {
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
            footstepAudioSource.loop = false;
            footstepAudioSource.playOnAwake = false;
        }

        anim = GetComponent<Animator>();
        anim.applyRootMotion = true; // Force root motion

    }

    float groundRayDist = 2f;

    void Update()
    {
        //if (isControlEnabled)
        //{
        //    Move(moveDirection);
        //    Jump(jumpDirection);
        //}

        //if (isControlEnabled)
        //{
        //    Move(moveDirection);
        //    Jump(jumpDirection);
        //}

        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up * groundRayDist * 0.5f, -Vector3.up);
        if (Physics.Raycast(ray, out hit, groundRayDist))
        {
            if (!onGround)
            {
                onGround = true;
                anim.SetFloat("LandingVelocity", rb.linearVelocity.magnitude); // Corrected property
                anim.SetBool("Land", true);
                anim.SetBool("Falling", false);
            }
        }
        else
        {
            onGround = false;
            anim.SetBool("Falling", true);
            anim.applyRootMotion = false;
        }
        Debug.DrawRay(transform.position + Vector3.up * groundRayDist * 0.5f, -Vector3.up * groundRayDist, Color.red);

        
        

        if (isControlEnabled)
        {
            Move(moveDirection);
            Jump(jumpDirection);
        }
        

    }

    // Methods to enable/disable player control
    public void DisablePlayerControl()
    {
        isControlEnabled = false;
    }

    public void EnablePlayerControl()
    {
        isControlEnabled = true;
    }

    #region Footstep Sound Methods

    /// <summary>
    /// Called by animation events to play a footstep sound.
    /// </summary>
    public void PlayFootstepSound()
    {
        if (!onGround || !isMoveInput || !isControlEnabled)
            return;

        if (footstepAudioSource == null || (walkFootstepClips.Count == 0 && runFootstepClips.Count == 0))
        {
            Debug.LogWarning("FootstepAudioSource or footstepClips not assigned.");
            return;
        }

        AudioClip clipToPlay = null;

        if (isSprinting && runFootstepClips.Count > 0)
        {
            // Select a random clip from runFootstepClips
            clipToPlay = runFootstepClips[Random.Range(0, runFootstepClips.Count)];
        }
        else if (walkFootstepClips.Count > 0)
        {
            // Select a random clip from walkFootstepClips
            clipToPlay = walkFootstepClips[Random.Range(0, walkFootstepClips.Count)];
        }

        if (clipToPlay != null)
        {
            footstepAudioSource.PlayOneShot(clipToPlay);
        }
    }


    public string GetCurrentAnimationState()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("AnimationName") ? "AnimationName" : "OtherState";
    }

    public void SetAnimationState(string animationState)
    {
        anim.Play(animationState);
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    public void SetMoveDirection(Vector2 dir)
    {
        moveDirection = dir;

        Debug.Log($"moveDirection = {moveDirection}");
        // Optionally call Move here:
        if (isControlEnabled)
        {
            Move(moveDirection);
        }
    }



    #endregion
}
