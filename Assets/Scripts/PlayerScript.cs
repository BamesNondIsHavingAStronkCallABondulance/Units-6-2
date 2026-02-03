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
    Dead,
};

public class PlayerScript : MonoBehaviour
{
    States state;

    InputAction moveAction;
    InputAction jumpAction;

    public Animator anim;
    Rigidbody rb;
    public bool grounded;

    public float waiting = 3f;
    public bool deathCooldown = true;

    // Start is called before the first frame update
    void Start()
    {
        state = States.Idle;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        DoLogic();
    }

    private void LateUpdate()
    {
        grounded = false;

    }


    void DoLogic()
    {
        if (state == States.Idle)
        {
            PlayerIdle();
        }

        if (state == States.Jump)
        {
            PlayerJumping();
        }

        if (state == States.Walk)
        {
            PlayerWalk();
        }

        if (state == States.Dead)
        {
            PlayerDead();
        }
    }


    void PlayerIdle()
    {
        anim.SetBool("isWalk", false);
        anim.SetBool("isDead", false);
        anim.SetBool("isIdle", true);
        deathCooldown = true;
        waiting = 3f;

        if (jumpAction.IsPressed())
        {
            // simulate jump
            state = States.Jump;
            rb.linearVelocity = new Vector3(0, 5, 0);
        }

        if (moveAction.IsPressed())
        {
            transform.Rotate(0, 0.5f, 0, Space.Self);

        }
        if (moveAction.IsPressed())
        {
            transform.Rotate(0, -0.5f, 0, Space.Self);
        }

        if (moveAction.IsPressed())
        {
            state = States.Walk;
        }
    }

    void PlayerJumping()
    {
        // player is jumping, check for hitting the ground
        if (grounded == true)
        {
            //player has landed on floor
            state = States.Idle;
        }
    }

    void PlayerWalk()
    {
        Vector3 vel;
        anim.SetBool("isWalk", true);
        anim.SetBool("isIdle", false);

        //magnitude = the player's speed
        float magnitude = rb.linearVelocity.magnitude;

        //move forward and preserve original y velocity

        if (moveAction.IsPressed())
        {
            vel = transform.forward * 5f;
        }
        else
        {
            vel = transform.forward * 0.5f;
        }

        rb.linearVelocity = new Vector3(vel.x, rb.linearVelocity.y, vel.z);

        if (magnitude <= 0.5f)
        {
            state = States.Idle;
        }

        if (jumpAction.IsPressed())
        {
            // simulate jump
            state = States.Jump;
            rb.linearVelocity = new Vector3(vel.x, 5, vel.z);
        }

    }

    void PlayerDead()
    {
        if (deathCooldown == true)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalk", false);
            anim.SetBool("isDead", true);
        }


        waiting -= Time.deltaTime;
        deathCooldown = false;


        if (waiting <= 0)
        {
            state = States.Idle;
            transform.position = new Vector3(9, 3, 5);
            rb.angularVelocity = Vector3.zero;
            print("reset transform");
        }


    }


    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Floor")
        {
            grounded = true;
            print("landed!");
        }

        if (col.gameObject.tag == "Enemy")
        {
            state = States.Dead;
        }
    }


    //Output debug info to canvas
    private void OnGUI()
    {
        float mag = rb.linearVelocity.magnitude;

        mag = Mathf.Round(mag * 100) / 100;

        //debug text
        string text = "Left/Right arrows = Rotate\nSpace = Jump\nUp Arrow = Forward\nCurrent state=" + state;
        text += "\nmag=" + mag;

        // define debug text area
        GUILayout.BeginArea(new Rect(10f, 450f, 1600f, 1600f));
        GUILayout.Label($"<size=16>{text}</size>");
        GUILayout.EndArea();
    }
}