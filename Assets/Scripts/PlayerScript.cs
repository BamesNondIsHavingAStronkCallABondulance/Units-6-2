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
};

public class PlayerScript : MonoBehaviour
{
    States state;

    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;

    private CharacterController controller;
    [SerializeField] private Transform camera;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float jumpHeight = 2f;

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
        if(controller.isGrounded == true)
        {
            if (sprintAction.IsPressed() && sprintToggle == false)
            {
                state = States.Sprint;
                sprintToggle = true;
            }
        }
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
            PlayerJump();
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
            if (sprintAction.IsPressed())
            {
                sprintToggle = false;
                state = States.Idle;
            }
        }
    }

    void PlayerJump()
    {
        anim.SetBool("isWalk", false);
        anim.SetBool("isIdle", false);
        anim.SetBool("isJump", true);


    }

    void PlayerSprint()
    {
        float h, v;
        h = moveAction.ReadValue<Vector2>().x;
        v = moveAction.ReadValue<Vector2>().y;

        Vector3 move = new Vector3(h, 0, v);
        move = camera.transform.TransformDirection(move);
        move.y = vertVelocity;
        move *= walkSpeed;
        controller.Move(move * 2 * Time.deltaTime);

        if (controller.isGrounded)
        {
        vertVelocity = -1f;

        if (jumpAction.IsPressed())
            {
                state = States.Jump;
            }
        }
    }

    void PlayerIdle()
    {
        vertVelocity = -1;

        anim.SetBool("isWalk", false);
        anim.SetBool("isJump", false);
        anim.SetBool("isIdle", true);

        if (jumpAction.IsPressed() && controller.isGrounded)
        {
            state = States.Jump;
            vertVelocity = Mathf.Sqrt(jumpHeight * gravity);
            return;
        }

        if (moveAction.ReadValue<Vector2>().magnitude > 0.1f )
        {
            state = States.Walk;
            return;
 
        }

        DoGravity();
    }

    private float VerticalForceCalc()
    {
        if(controller.isGrounded  )
        {
            vertVelocity = -1f;

            if(jumpAction.IsPressed())
            {
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

    void PlayerWalk()
    {
        anim.SetBool("isWalk", true);
        anim.SetBool("isIdle", false);
        anim.SetBool("isJump", false);

        PlayerWalkCalc();
        VerticalForceCalc();

        if(moveAction.ReadValue<Vector2>().magnitude < 0.1f )
        {
            state = States.Idle;
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

        Vector3 vel = new Vector3(0, vertVelocity, 0);
        controller.Move(vel * Time.deltaTime);
    }

    void CheckForLanding()
    { 
        if( controller.isGrounded && vertVelocity <= 0 )
        {
            state = States.Idle;
            vertVelocity = -1;
        }
    }

}