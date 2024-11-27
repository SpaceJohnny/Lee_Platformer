using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PController : MonoBehaviour
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


    public float accelerationTime;
    public float decelerationTime;
    public float maxSpeed;

    public int health = 10;

    private Rigidbody2D playerRB;
    private float acceleration;
    private bool isJumping = false;


    // Start is called before the first frame update
    void Start()
    {
        playerRB = GetComponent<Rigidbody2D>();
        acceleration = maxSpeed / accelerationTime;
    }

    // Update is called once per frame
    void Update()
    {
        previousCharacterState = currentCharacterState;

        if (IsGrounded() && Input.GetKeyDown(KeyCode.UpArrow))
        {
            isJumping = true;
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

    private void FixedUpdate()
    {
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        Vector2 playerInput = new Vector2();
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            playerInput += Vector2.left;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            playerInput += Vector2.right;
        }
        if (isJumping)
        {
            //Trigger our jump logic
            Debug.Log("Player is jumping woohoo!!");
            isJumping = false;
        }


        MovementUpdate(playerInput);
    }



    private void MovementUpdate(Vector2 playerInput)
    {
        Vector2 velocity = playerRB.velocity;
        Debug.Log(playerInput.ToString());
        if (playerInput.x != 0)
        {
            velocity += playerInput * acceleration * Time.fixedDeltaTime;
        }
        else
        {
            velocity = new Vector2(0, velocity.y);
        }

        playerRB.velocity = velocity;
    }

    public bool IsWalking()
    {
        return false;
    }
    public bool IsGrounded()
    {
        return true;
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    public FacingDirection GetFacingDirection()
    {
        if (playerRB.velocity.x > 0)
        {
            currentFacingDirection = FacingDirection.right;
        }
        else if (playerRB.velocity.x < 0)
        {
            currentFacingDirection = FacingDirection.left;
        }

        return currentFacingDirection;
    }
}


