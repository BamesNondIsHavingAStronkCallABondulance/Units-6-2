using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;


public enum States // used by all logic
{
    None,
    Idle,
    Walk,
    Jump,
    Sprint,
    Interact, 
};

public class PlayerScript : MonoBehaviour
{
    States state;
    States lastState;

    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;

    private CharacterController controller;
    [SerializeField] private Transform camera;

    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 3f;
    [SerializeField] private float crouchSpeed = 0.75f;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float jumpHeight = 1.1f;

    private float vertVelocity;

    public Animator anim;
    public bool grounded;

    public float waiting = 3f;
    public bool deathCooldown = true;

    // Start is called before the first frame update
    void Start()
    {
        state = States.Idle;
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");

    }

    // Update is called once per frame
    void Update()
    {
        StateLogic();

        if (InteractableObjects.openChest == true)
        {
            state = States.Interact;
        }
    }

    private void LateUpdate()
    {
        grounded = false;
    }


    void StateLogic()
    {

        switch (state)
        {
            case States.Idle:
                IdleState();
                break;

            case States.Jump:
                JumpState();
                break;

            case States.Walk:
                WalkState();
                break;

            case States.Interact:
                InteractState();
                break;

        }
    }

    void IdleState()
    {
        PlayerIdle();
        DoGravity();


    }

    void JumpState()
    {
        DoGravity();
        CheckForLanding();
    }

    void WalkState()
    {
        PlayerWalk();
    }

    void InteractState()
    {
        PlayerInteract();
    }

    void PlayerInteract()
    {
        if (InteractableObjects.openChest == true)
        {
            anim.SetBool("isInteract", true);
            anim.SetBool("isWalk", false);
            anim.SetBool("isJump", false);
            anim.SetBool("isSprint", false);
            anim.SetBool("isIdle", false);
        }
        else
        {
            state = States.Idle;
        }
    }

    void PlayerIdle()
    {
        vertVelocity = -1;

        anim.SetBool("isWalk", false);
        anim.SetBool("isJump", false);
        anim.SetBool("isSprint", false);
        anim.SetBool("isInteract", false);
        anim.SetBool("isIdle", true);

        if (jumpAction.IsPressed() && controller.isGrounded)
        {
            vertVelocity = Mathf.Sqrt(jumpHeight * gravity);

            anim.SetBool("isWalk", false);
            anim.SetBool("isSprint", false);
            anim.SetBool("isIdle", false);
            anim.SetBool("isJump", true);
            anim.SetBool("isInteract", false);

            state = States.Jump;
        }

        if (moveAction.ReadValue<Vector2>().magnitude > 0.1f )
        {
            state = States.Walk;
 
        }

        VerticalForceCalc();
    }

    private float VerticalForceCalc()
    {
        if(controller.isGrounded)
        {
            vertVelocity = -1f;

            if(jumpAction.IsPressed())
            {
                vertVelocity = Mathf.Sqrt(jumpHeight * gravity);
                state = States.Jump;
            }
        }
        else
        {
            vertVelocity -= gravity * Time.deltaTime;
        }
        return vertVelocity;
    }

    void PlayerWalkCalc()
    {
        float h, v;
        h = moveAction.ReadValue<Vector2>().x;
        v = moveAction.ReadValue<Vector2>().y;

        Vector3 move = new Vector3(h, 0, v);
        move = camera.transform.TransformDirection(move);
        move.y = vertVelocity;
        move *= walkSpeed;
        controller.Move(move * Time.deltaTime);
    }

    void PlayerSprintCalc()
    {
        float h, v;
        h = moveAction.ReadValue<Vector2>().x;
        v = moveAction.ReadValue<Vector2>().y;

        Vector3 move = new Vector3(h, 0, v);
        move = camera.transform.TransformDirection(move);
        move.y = vertVelocity;
        move *= runSpeed;
        controller.Move(move / 2 * Time.deltaTime);
    }

    void PlayerWalk()
    {

        anim.SetBool("isWalk", true);
        anim.SetBool("isIdle", false);
        anim.SetBool("isJump", false);
        anim.SetBool("isInteract", false);



        PlayerWalkCalc();
        VerticalForceCalc();

        if(moveAction.ReadValue<Vector2>().magnitude < 0.1f )
        {
            state = States.Idle;
            return;
        }

        if (sprintAction.IsPressed())
        {
            //state = States.Sprint;
            anim.SetBool("isSprint", true);
            anim.SetBool("isWalk", false);

            PlayerSprintCalc();
        }
        else
        {
            anim.SetBool("isSprint", false);
            anim.SetBool("isWalk", true);

        }
    }

    void DoGravity()
    {
        vertVelocity -= gravity * Time.deltaTime;

        Vector3 vel = new Vector3(controller.velocity.x, vertVelocity, controller.velocity.z);

        controller.Move(vel * Time.deltaTime);
    }

    void CheckForLanding()
    { 
        if( controller.isGrounded && vertVelocity <= 0 )
        {
            anim.SetBool("isWalk", true);

            state = States.Idle;
            vertVelocity = -1;
        }
    }

}