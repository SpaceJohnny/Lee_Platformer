using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection
    {
        left, right
    }
    public FacingDirection currentFacingDirection; 

    public enum CharacterState
    {
        idle, walk, jump, die
    }
    public CharacterState currentCharacterState = CharacterState.idle;
    public CharacterState previousCharacterState = CharacterState.idle;

    private Rigidbody2D rb;
    private float acceleration;
    private float deceleration;
    private Vector2 currentVelocity;

    public float timeToReachMaxSpeed;
    public float timeToDecelerate;
    public float maxSpeed;

    [SerializeField] FacingDirection direction;

    public bool jumping = false;

    //death animation
    public int health = 10;

    //for jumping
    public float apexHeight;
    public float apexTime;
    private float gravity;
    private float jumpVelocity;

    //to check isGrounded 
    public GameObject groundRayObject;
    public LayerMask mask;
    public float rayDistance;

    public float terminalSpeed;

    //time limit for coyote jump to occur
    public float coyoteTime;
    public float airTime;
    public float TimeSinceLastJump;

    //dashing
    public float dashSpeed = 15;
    public float distanceToStop;
    public bool isDashing = false;

    public Vector2 overlapBoxSize = new Vector2(1.75f,0.8f);
    public Vector3 stopPosition = new Vector3();
    public LayerMask wallLayerMask;

    //wall jump
    public GameObject horizontalRayObject;
    public float sideRayDistance;

    //pebble jump
    public float pebbleJumpVelocity;
    private int timesJumped = 0;

    // Start is called before the first frame update
    void Start()
    {
        acceleration = maxSpeed / timeToReachMaxSpeed;
        deceleration = maxSpeed / timeToDecelerate;

        rb = GetComponent<Rigidbody2D>();

        gravity = -2 * apexHeight / (apexTime * apexTime);
        jumpVelocity = 2 * apexHeight / apexTime;

    }

    private void Update()
    {
        previousCharacterState = currentCharacterState;
        TimeSinceLastJump += Time.deltaTime;

        Debug.Log(currentCharacterState);
        rb.velocity = currentVelocity;

        switch (currentCharacterState)
        {
            case CharacterState.die:

                break;
            case CharacterState.jump:

                if (IsGrounded())
                {
                    //We know we need to make a transition because we're not grounded anymore
                    if (IsWalking())
                    {
                        currentCharacterState = CharacterState.walk;
                    }
                    else
                    {
                        currentCharacterState = CharacterState.idle;
                    }
                }

                break;
            case CharacterState.walk:
                if (!IsWalking())
                {
                    currentCharacterState = CharacterState.idle;
                }
                //Are we jumping?
                if (!IsGrounded())
                {
                    currentCharacterState = CharacterState.jump;
                }
                break;
            case CharacterState.idle:
                //Are we walking?
                if (currentVelocity.x != 0)
                {
                    currentCharacterState = CharacterState.walk;
                }
                //Are we jumping?
                if (!IsGrounded())
                {
                    currentCharacterState = CharacterState.jump;
                }

                break;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        Vector2 playerInput = new Vector2();
        MovementUpdate(playerInput);

        IsWalking();
        GetFacingDirection();
        IsGrounded();

        Dash();
        WallJump();
        applyDash();

        Jump();
        PebbleJump();

        if (!IsGrounded())
        {
            currentVelocity.y += gravity * Time.deltaTime;

            airTime += Time.deltaTime;
            if (currentVelocity.y < terminalSpeed)
            {
                Debug.Log(rb.velocity);
                currentVelocity.y = terminalSpeed;
            }
            //0.3sec since last jump (off ground) 
            if (airTime < coyoteTime && TimeSinceLastJump > 0.3 && Input.GetKeyDown(KeyCode.UpArrow))
            {
                TimeSinceLastJump = 0;
                jumping = true;
                Debug.Log("jump");
                currentVelocity.y = jumpVelocity;

            }
        }

        if (IsGrounded())
        {
            currentVelocity.y = Mathf.Max(rb.velocity.y, currentVelocity.y);
            airTime = 0;
            jumping = false;

            //for the pebble jump
            timesJumped = 0;
            pebbleJumpVelocity = jumpVelocity;
        }

    }

    private void Jump()
    {
        if (IsGrounded() && Input.GetKeyDown(KeyCode.UpArrow))
        {
            TimeSinceLastJump = 0;
            jumping = true;
            currentVelocity.y = jumpVelocity;

            timesJumped++;

        }
    }

    private void PebbleJump()
    {
        if (!IsGrounded() && Input.GetKey(KeyCode.Space))
        {
            if (timesJumped == 0)
            {
                TimeSinceLastJump = 0;
                jumping = true;
                currentVelocity.y = jumpVelocity;

                timesJumped++;
            }
            else if (timesJumped >= 1)
            {
                halvedJumpHeight();

                TimeSinceLastJump = 0;
                jumping = true;
                currentVelocity.y = pebbleJumpVelocity;

                timesJumped++;
            }
        }
    }

    private void halvedJumpHeight()
    {
        pebbleJumpVelocity = Mathf.Sqrt(pebbleJumpVelocity * pebbleJumpVelocity) * 0.5f;
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        //move player to the left
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (rb.velocity.x > -maxSpeed)
            {
                currentVelocity += acceleration * Vector2.left * Time.deltaTime;
            }
        }

        //move player to the right
        //limits to not pass max speed, to stop 
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (rb.velocity.x < +maxSpeed)
            {
                currentVelocity += acceleration * Vector2.right * Time.deltaTime;
            }
        }

        if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            if (rb.velocity.x > 0)
            {
                currentVelocity -= deceleration * Vector2.right * Time.deltaTime;
            }
            if (rb.velocity.x < 0)
            {
                currentVelocity -= deceleration * Vector2.left * Time.deltaTime;
            }
            if (rb.velocity.x < 0.1f && rb.velocity.x > -0.1f)
            {
                currentVelocity = new Vector2(0, currentVelocity.y);
            }

        }
        //character falls to slow 
        //set horizontal velocity to current velocity
        //set vertical velocity to current value - does not change
        rb.velocity = new Vector2(currentVelocity.x, currentVelocity.y);
    }

    public bool IsWalking()
    {
        if (rb.velocity.x != 0)
        {
            return true;
        }
        else return false;
    }

    public bool IsGrounded()
    {
        RaycastHit2D hitGround = Physics2D.Raycast(groundRayObject.transform.position, Vector2.down, rayDistance, mask);
        //Debug.DrawRay(groundRayObject.transform.position, Vector2.down * hitGround.distance, Color.yellow);

        if (hitGround)
        {
            //Debug.Log("hit");
            Debug.DrawRay(groundRayObject.transform.position, Vector2.down * rayDistance, Color.yellow);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    private void Dash()
    {
        //start dash
        if (Input.GetKey(KeyCode.D))
        {
            isDashing = true;
            if (direction == FacingDirection.right)
            {
                stopPosition = new Vector3(transform.position.x + distanceToStop, transform.position.y); 
                if(transform.position.x >= stopPosition.x)
                {
                    isDashing = false;

                }
            }
            else if (direction == FacingDirection.left)
            {
                stopPosition = new Vector3(transform.position.x + -distanceToStop, transform.position.y);
                if (transform.position.x <= stopPosition.x)
                {
                    isDashing = false;

                }
            }
        }

        if (Physics2D.OverlapBox(transform.position,overlapBoxSize,0,wallLayerMask))
        {
            isDashing = false;
        }
    }

    private void applyDash()
    {
        if (isDashing) 
        {
            if (direction == FacingDirection.left)
            {
                currentVelocity.x = -1;
            }
            if (direction == FacingDirection.right)
            {
                currentVelocity.x = 1;
            }
            currentVelocity.x = currentVelocity.x * dashSpeed;
        }
    }

    private void WallJump()
    {
        RaycastHit2D hitRightSide = Physics2D.Raycast(horizontalRayObject.transform.position, Vector2.right, sideRayDistance, mask);
        RaycastHit2D hitLeftSide = Physics2D.Raycast(horizontalRayObject.transform.position, Vector2.left, sideRayDistance, mask); 

        if (hitRightSide)
        {
            Debug.DrawRay(horizontalRayObject.transform.position, Vector2.right * sideRayDistance, Color.magenta);
        }

        if (hitLeftSide)
        {
            Debug.DrawRay(horizontalRayObject.transform.position, Vector2.left * sideRayDistance, Color.magenta);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //Debug.Log("wall jump key press");
            if (!IsGrounded() && hitLeftSide)
            {
                Debug.Log("hit left wall jump");
                currentVelocity.x = jumpVelocity;
                currentVelocity.y = jumpVelocity;
            }

            if (!IsGrounded() && hitRightSide)
            {
                Debug.Log("hit right wall jump");
                currentVelocity.x = -jumpVelocity;
                currentVelocity.y = jumpVelocity;
            }
        }
    }

    public FacingDirection GetFacingDirection()
    {
        if (rb.velocity.x > 0)
        {
            direction = FacingDirection.right;
            return FacingDirection.right;
        }
        else if (rb.velocity.x < 0)
        {
            direction = FacingDirection.left;
            return FacingDirection.left;
        }
        else return direction;
    }
}
