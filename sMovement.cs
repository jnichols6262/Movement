using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class sMovement : MonoBehaviour
{
    

    [Header("Character Attachments")]
    public Transform cam;
    public Transform groundCheck;
    public Spline spline;
    public Rigidbody rb;
    public GameManager gm;
    public Score sc;
    
    

    [Header("Ground Check")]
    public float groundDistance = 1f;
    public LayerMask groundMask;
    public bool isGrounded;
    
    float height;
    RaycastHit slopeHit;
    public Vector3 lastPos;

    [Header("Ground Movement")]
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    public float groundDrag = 1f;
    public float movementMultiplier = 6f;
    float turnSmoothVelocity;
    Vector3 direction;

    [Header("Air Movement")]
    public float airDrag = 0.8f;
    public float aerialMultipler = 4f;
    public float maxJumpHeight;
    public float jumpHeight = 4f;
    public float superJumpMultiplier;
    public float gravity = -9.81f;
    float jumpTimer;
    
    float jumpBufferTime = 0.2f;
    float jumpBufferCounter;

    public float FallingThreshold = -1f;
    public bool isFalling = false;
    public bool isJumping;

    [Header("More Jump Movements")]
    public float jumpSpeed;
    public float ySpeed;
    private float? jumpButtonPressedTime;
    

    

    public float playerVelocity;

    [Header("Ramp Movement")]
    public float rampMultiplier = 8f;

    [Header ("Animation")]
    public Animator animator;
    public AnimationManager animatorManager;
    public float timer;

    public bool isInteracting;
    public float inAirTimer;

    public LayerMask groundLayer;
    public float rayCastHeightOffSet = 0.5f;

    [Header("Respawn Position")]
    [SerializeField]
    private Transform player;
    [SerializeField]
    private Transform respawnPoint;

    private void Awake()
    {
        gm = GameManager.gm;
        sc = Score.scoreInstance;
        
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        height = GetComponent<Collider>().bounds.size.y / 2f;
        animatorManager = GetComponent<AnimationManager>();
        
        ControllerManager.controllerManager.playerControls.Gameplay.Movement.Enable();
        ControllerManager.controllerManager.playerControls.Gameplay.Jump.Enable();
        ControllerManager.controllerManager.playerControls.Gameplay.SuperJump.Enable();
        ControllerManager.controllerManager.playerControls.Gameplay.JumpStartAnim.Enable();
        ControllerManager.controllerManager.playerControls.Gameplay.Movement.performed += PlayerMove;
        ControllerManager.controllerManager.playerControls.Gameplay.JumpStartAnim.performed += JumpTimer;
        ControllerManager.controllerManager.playerControls.Gameplay.Jump.canceled += Jump;
        //playerControls.Gameplay.SuperJump.canceled += SuperJump;
        ControllerManager.controllerManager.playerControls.Gameplay.Jump.performed += JumpStartAnim;

       

    }
    public void EnableSkateBoardControls() //Enables the skateboard ground movement
    {
        ControllerManager.controllerManager.playerControls.Gameplay.Movement.Enable();
    }
    public void DisableSkateBoardControls() //Disables the skateboard ground movement
    {
        ControllerManager.controllerManager.playerControls.Gameplay.Movement.Disable();
    }
    public void AttachToRail() // This is where the player is attached to the grindcube, speed is reset, and gravity is turned off
    {
        gm = GameManager.gm;
        gm.s_Audio.Play(19);
        rb.useGravity = false;
        DisableSkateBoardControls();
        transform.SetParent(spline.grindCube.transform);
        transform.localPosition = new Vector3(0, .65f, 0);
        spline.isAttached = true;
        rb.velocity = Vector3.zero;
    }
    public void DeattachRail() // This is where the player is dettach
    {
        gm = GameManager.gm;
        gm.s_Audio.Stop(19);
        spline.isAttached = false;
        transform.SetParent(null);
        rb.useGravity = true;
        spline.DestroyGrindCube();
        EnableSkateBoardControls();
    }
    void JumpTimer(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            jumpTimer += Time.deltaTime;
        }
    }
    bool OnSlope() //Checks for slope
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, height / 2 + 5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                Debug.Log("slope hit");
                return true;
            }
            else
            {
                return false;

            }

        }
        return false;
    }
    void ControlDrag() //Controls the change for drag
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
            rb.drag = airDrag;
    }
    public void PlayerMove(InputAction.CallbackContext context) //X,Z movement
    {
        Vector2 inputVector = context.ReadValue<Vector2>();
        direction = new Vector3(inputVector.x, 0f, inputVector.y).normalized;
        if (timer == 0f | timer <= 0f)
        {
           animator.SetBool("isCoasting", false);
        }
        timer -= Time.deltaTime;
    }
    void JumpStartAnim(InputAction.CallbackContext context)
    {
        if(isGrounded)
        {
            gm = GameManager.gm;
            gm.s_Audio.Play(0);
            animator.SetTrigger("OlliePrep");
        }
        else
        {
            gm = GameManager.gm;
        }
    }
    public void Jump(InputAction.CallbackContext context)//Y movement
    {
        if(jumpTimer <= 0.001f)
        {
            gm = GameManager.gm;
            gm.s_Audio.Stop(0);
            ySpeed = jumpSpeed;
            jumpBufferCounter = jumpBufferTime;
            jumpButtonPressedTime = Time.time;

            if (isGrounded && jumpBufferCounter > 0f)
            {
                gm.s_Audio.Play(9);
                animator.SetTrigger("IsJumpingTrig");
                isJumping = true;
                rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
                jumpBufferCounter = 0f;
                isGrounded = false;
            }
            jumpTimer = 0f;
            
        }
    }
    float JumpHeightCalc()
    {
        float jumpHeightHolder;

        jumpHeightHolder = jumpHeight * superJumpMultiplier;

        if (jumpHeightHolder > maxJumpHeight)
            return maxJumpHeight;
        else
            return jumpHeightHolder;
    }
    
    /*public void SuperJump(InputAction.CallbackContext context)
    {
        if(jumpTimer >= 0.001f)
        {
            gm = GameManager.gm;
            jumpBufferCounter = jumpBufferTime;
            Debug.Log("JUMP! " + animator);
            animator.SetTrigger("Ollie");
            gm.s_Audio.Stop(0);
            if (isGrounded && jumpBufferCounter > 0f)
            {
                gm.s_Audio.Play(9);
                rb.AddForce(transform.up * JumpHeightCalc(), ForceMode.Impulse);
                //coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
            }
            jumpTimer = 0f;
        }
    }*/
    
    public void isCoasting()
    {
        if (transform.position != lastPos)
        {
            animator.SetBool("isCoasting", true);
            timer = .8f;
            timer -= Time.deltaTime;
        }
        else
        {
            if (timer == 0f | timer <= 0)
            {
                animator.SetBool("isCoasting", false);
            }
        }
        lastPos = transform.position;
    }
    private void Update()
    {
        ControlDrag();
        isCoasting();
        jumpBufferCounter -= Time.deltaTime;

        playerVelocity = rb.velocity.y;
        isFalling = playerVelocity < -4;
        Debug.Log("is falling is " + isFalling);
        animator.SetBool("IsFalling", playerVelocity < -4);

        if (isGrounded == true)
        {
            animator.SetBool("IsGrounded", true);
        } else { animator.SetBool("IsGrounded", false); }
    }
    private void FixedUpdate()
    {

        RaycastHit hit;
        if (Physics.Raycast(groundCheck.position, -transform.up, out hit, groundDistance, groundMask))//checks to see if player is on the ground;
        {
            isGrounded = true;
        }
        else
            isGrounded = false;

        if (spline != null)
        {
            if (spline.isAttached == false)
            {
                if (direction.magnitude >= 0.1f)
                {

                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                    transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    Vector3 moveDir;


                    moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    if (isGrounded && !OnSlope())
                        rb.AddForce(moveDir.normalized * speed * movementMultiplier, ForceMode.Acceleration);
                    else if (isGrounded && OnSlope())
                        rb.AddForce(moveDir.normalized * speed * rampMultiplier, ForceMode.Acceleration);
                    else if (!isGrounded)
                        rb.AddForce(moveDir.normalized * speed * aerialMultipler, ForceMode.Acceleration);
                }
                transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation; //Changes the player angle depending on the surface
            }
        }
        else
        {
            if (direction.magnitude >= 0.1f)
            {

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir;


                moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                if (isGrounded && !OnSlope())
                    rb.AddForce(moveDir.normalized * speed * movementMultiplier, ForceMode.Acceleration);
                else if (isGrounded && OnSlope())
                    rb.AddForce(moveDir.normalized * speed * rampMultiplier, ForceMode.Acceleration);
                else if (!isGrounded)
                    rb.AddForce(moveDir.normalized * speed * aerialMultipler, ForceMode.Acceleration);
            }
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;//Changes the player angel depending on the surface
        }
        
    }

    public void ResetPlayer()
    {
        sc = Score.scoreInstance;
        Score.wantedLevel = 0;
        Score.scoreInstance.ResetScore();
        player.transform.position = respawnPoint.transform.position;

       foreach (GameObject wantedStar in sc.wantedStarsLevel)
       {
            wantedStar.SetActive(false);
       }
    }

}



