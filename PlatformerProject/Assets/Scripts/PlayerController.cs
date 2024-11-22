using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;
    private float acceleration;
    private float deceleration;
    private Vector2 currentVelocity;

    public float timeToReachMaxSpeed;
    public float timeToDecelerate;
    public float maxSpeed;

    //public float apexHeight;
    //public float apexTime;

    FacingDirection direction;

    private bool didWeJump = false;


    public enum FacingDirection
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        acceleration = maxSpeed / timeToReachMaxSpeed;
        deceleration = maxSpeed / timeToDecelerate;

        rb = GetComponent<Rigidbody2D>();

    }

    private void Update()
    {
        //if grounded and player jumps 
        if (Input.GetKey(KeyCode.UpArrow))
        {
            //do stuff to the current Velocity
            didWeJump = true;
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
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        //alternative to addforce
        //make adjustments to this value 
        //Vector2 currentVelocity = rb.velocity;

        //move player to the left
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            if (rb.velocity.x > - maxSpeed)
            {
                currentVelocity += acceleration * Vector2.left * Time.deltaTime;
            }
        }

        //move player to the right
        //limits to not pass max speed, to stop 
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (rb.velocity.x < + maxSpeed)
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
        }

        //Jump trigger 
        if (didWeJump)
        {
            //jump logic
            //apex height and time

            didWeJump = false;
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
        return false;
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
                
        //else return FacingDirection.right;
 
        
    }
}
