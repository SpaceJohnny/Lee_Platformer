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
    public FacingDirection currentFacingDirection = FacingDirection.right;

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

    public float apexHeight;
    public float apexTime;

    [SerializeField] FacingDirection direction;

    private bool jumpOn = false;

    public int health = 10;


    //assign in inspector
    public GameObject groundRayObject;
    public LayerMask mask;


    // Start is called before the first frame update
    void Start()
    {
        acceleration = maxSpeed / timeToReachMaxSpeed;
        deceleration = maxSpeed / timeToDecelerate;

        rb = GetComponent<Rigidbody2D>();

    }

    private void Update()
    {
        previousCharacterState = currentCharacterState;

        if (IsGrounded() && Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpOn = true;
        }


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
                if (IsWalking())
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

        //bros not using playerInput

        Vector2 playerInput = new Vector2();
        MovementUpdate(playerInput);

        IsWalking();
        GetFacingDirection();

        IsGrounded();
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        //alternative to addforce
        //make adjustments to this value 
        //Vector2 currentVelocity = rb.velocity;

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

        //Jump trigger 
        if (jumpOn)
        {
            //jump logic
            //apex height and time

            jumpOn = false;
        }


        //character falls to slow 
        //set horizontal velocity to current velocity
        //set vertical velocity to current value - does not change
        rb.velocity = new Vector2(currentVelocity.x, rb.velocity.y);

    }

    public bool IsWalking()
    {
        if (rb.velocity.x > 0)
        {
            return true;
        }
        else return false;
    }


    public bool IsGrounded()
    {
        RaycastHit2D hitGround = Physics2D.Raycast(groundRayObject.transform.position, transform.position + Vector3.down, 1f, mask);
        //Debug.DrawRay(groundRayObject.transform.position, Vector2.down * hitGround.distance, Color.yellow);

        if (hitGround)
        {
            Debug.Log("hit");
            Debug.DrawRay(groundRayObject.transform.position, Vector2.down * hitGround.distance, Color.yellow);
            return true;

        }
        else
        {
            Debug.Log("i didn't hit her officer");
            return false;
        }
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    public FacingDirection GetFacingDirection()
    {
        if (rb.velocity.x > 0)
        {
            direction = FacingDirection.right;
            Debug.Log("right");
            return FacingDirection.right;
        }
        else if (rb.velocity.x < 0)
        {
            direction = FacingDirection.left;
            Debug.Log("left");
            return FacingDirection.left;
        }
        else return direction;

        //else return FacingDirection.right;


    }
}
