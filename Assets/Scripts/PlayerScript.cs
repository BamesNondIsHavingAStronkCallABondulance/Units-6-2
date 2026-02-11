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
    SprintJump,
};

public class PlayerScript : MonoBehaviour
{
    States state;

    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;

    private CharacterController controller;
    [SerializeField] private Transform camera;

    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float jumpHeight = 1.1f;

    private float vertVelocity;

    public Animator anim;
    public bool grounded;
    private bool sprintToggle = false;

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
        DoLogic();
    }

    private void LateUpdate()
    {
        grounded = false;
    }


    void DoLogic()
    {
        print("State=" + state + "  Grounded=" + controller.isGrounded + "  yv=" + vertVelocity);

        if (state == States.Idle)
        {
            PlayerIdle();
            DoGravity();
            //VerticalForceCalc();

        }

        if (state == States.Jump)
        {
            DoGravity();
            CheckForLanding();

        }

        if (state == States.Walk)
        {
            PlayerWalk();
        }

        if (state == States.Sprint)
        {
            PlayerSprint();
        }

        if (state == States.SprintJump)
        {
            PlayerSprintJump();
            DoSprintGravity();
            CheckForLanding();
        }
    }

    void PlayerSprint()
    {
        anim.SetBool("isWalk", false);
        anim.SetBool("isSprint", true);
        anim.SetBool("isIdle", false);
        anim.SetBool("isJump", false);
        anim.SetBool("isSprintJump", false);

        PlayerSprintCalc();
        VerticalSprintForceCalc();

        if (sprintAction.IsPressed())
        {
            sprintToggle = false;
            state = States.Walk;
        }

        if (moveAction.ReadValue<Vector2>().magnitude < 0.1f)
        {
            state = States.Idle;
        }

        VerticalSprintForceCalc();
        DoGravity();

    }

    void PlayerIdle()
    {
        vertVelocity = -1;

        anim.SetBool("isWalk", false);
        anim.SetBool("isJump", false);
        anim.SetBool("isSprint", false);
        anim.SetBool("isIdle", true);

        if (jumpAction.IsPressed() && controller.isGrounded)
        {
            vertVelocity = Mathf.Sqrt(jumpHeight * gravity);

            anim.SetBool("isWalk", false);
            anim.SetBool("isSprint", false);
            anim.SetBool("isIdle", false);
            anim.SetBool("isJump", true);

            state = States.Jump;
        }

        if (moveAction.ReadValue<Vector2>().magnitude > 0.1f )
        {
            state = States.Walk;
            return;
 
        }

        if ( sprintAction.IsPressed())
        {
            state = States.Sprint;
            return;
        }

        DoGravity();
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
                if (state == States.Walk)
                {
                    anim.SetBool("isSprintJump", true);
                }
                state = States.Jump;
            }
        }
        else
        {
            vertVelocity -= gravity * Time.deltaTime;
        }
        return vertVelocity;
    }

    private void VerticalSprintForceCalc()
    {
        if (controller.isGrounded)
        {
            vertVelocity = -1f;

            if (jumpAction.IsPressed())
            {

                anim.SetBool("isWalk", false);
                anim.SetBool("isSprint", false);
                anim.SetBool("isIdle", false);
                anim.SetBool("isSprintJump", true);

                state = States.SprintJump;
            }
        }
        else
        {
            vertVelocity -= gravity * Time.deltaTime;
        }
        //return vertVelocity;
    }

    void PlayerSprintJump()
    {
        anim.SetBool("isSprint", false);
        anim.SetBool("isSprintJump", true);


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
        controller.Move(move * Time.deltaTime);
    }

    void PlayerWalk()
    {
        anim.SetBool("isWalk", true);
        anim.SetBool("isIdle", false);
        anim.SetBool("isJump", false);
        anim.SetBool("isSprint", false);

        PlayerWalkCalc();
        VerticalForceCalc();

        if(moveAction.ReadValue<Vector2>().magnitude < 0.1f )
        {
            state = States.Idle;
        }

        if (sprintAction.IsPressed())
        {
            state = States.Sprint;
            return;
        }
    }


    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Floor")
        {
            grounded = true;
            print("landed!");
        }
    }

    void DoGravity()
    {
        vertVelocity -= gravity * Time.deltaTime;

        Vector3 vel = new Vector3(controller.velocity.x, vertVelocity, controller.velocity.z);

        controller.Move(vel * Time.deltaTime);
    }

    void DoSprintGravity()
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
            anim.SetBool("isSprintJump", false);

            state = States.Idle;
            vertVelocity = -1;
        }
    }

}