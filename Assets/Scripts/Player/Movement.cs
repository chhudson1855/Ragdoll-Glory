using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

public class Movement : MonoBehaviour
{
    [Header("Player Configuration")]

    [Header("Body Parts")]
    public GameObject leftLeg;
    public GameObject rightLeg;
    public GameObject StrongLeft;
    public GameObject StrongRight;
    public GameObject Torso;
    public string MovementType;

    [Header("Movement Parameters")]
    [SerializeField] float speed = 2f;
    [SerializeField] float jumpHeight = 2f;
    [SerializeField] float legWait = .5f;
    [SerializeField] float PunchCharge = 0f;

    [Header("Ground Check")]
    public Transform leftFoot;
    public Transform rightFoot;
    [SerializeField] float groundCheckDistance = 0.1f;
    public LayerMask jumpableLayer;

    private Rigidbody2D leftLegRB;
    private Rigidbody2D rightLegRB;
    private Rigidbody2D TorsoRB;

    private float pressCharge;
    private bool pressing;

    private Animator anim;

    public GameObject targetEnemy;

    private Dictionary<string, Dictionary<string, KeyCode>> MovementKeys = new Dictionary<string, Dictionary<string, KeyCode>>()
    {
        { "WASD", new Dictionary<string, KeyCode> {{"Left", KeyCode.A}, {"Right", KeyCode.D}, {"Jump", KeyCode.S}, {"Attack", KeyCode.W} } },
        { "Arrow", new Dictionary<string, KeyCode> {{"Left", KeyCode.LeftArrow}, {"Right", KeyCode.RightArrow}, {"Jump", KeyCode.DownArrow}, {"Attack", KeyCode.UpArrow} } }
    };

    void Start()
    {
        leftLegRB = leftLeg.GetComponent<Rigidbody2D>();
        rightLegRB = rightLeg.GetComponent<Rigidbody2D>();
        TorsoRB = Torso.GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        pressCharge = 1;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovementAnimations();
        HandleJumpInput();
        HandleLaunch();
    }

    void HandleLaunch()
    {
        if (Input.GetKey(MovementKeys[MovementType]["Attack"]))
        {
            PunchCharge += 20f * Time.deltaTime;
        }

        if (Input.GetKeyUp(MovementKeys[MovementType]["Attack"]))
        {
            Debug.Log(PunchCharge);

            Vector2 directionToEnemy = (Vector2)targetEnemy.transform.position - TorsoRB.position;
            float angle = Mathf.Atan2(directionToEnemy.y, directionToEnemy.x) * Mathf.Rad2Deg;

            //rb.MoveRotation(Mathf.LerpAngle(rb.rotation, angle - 90, math.min(Charge * force, 50f) * Time.deltaTime));
            TorsoRB.AddForce(directionToEnemy.normalized * math.min(PunchCharge * 100, 50f), ForceMode2D.Impulse);

            targetEnemy.transform.Find("Torso").GetComponent<Rigidbody2D>().AddForce(-directionToEnemy.normalized * math.min((PunchCharge * 50)*100, 25f), ForceMode2D.Impulse);
            PunchCharge = 0;
        }
    }

    void HandleMovementAnimations()
    {
        if (Input.GetKey(MovementKeys[MovementType]["Right"]))
        {
            anim.Play("WalkLeft");
            StartCoroutine(MoveRight(legWait));
        }
        else if (Input.GetKey(MovementKeys[MovementType]["Left"]))
        {
            anim.Play("WalkRight");
            StartCoroutine(MoveLeft(legWait));
        }
        else
        {
            anim.Play("idle");
        }
    }

    void HandleJumpInput()
    {
        if (Input.GetKey(MovementKeys[MovementType]["Jump"]) && IsGrounded)
            {
                if (pressing)
                {
                    TorsoRB.AddForce(Vector2.down * 2);
                    pressCharge += 0.0005f;
                }
                pressing = true;
            }
            else
            {
                if (Input.GetKeyUp(MovementKeys[MovementType]["Jump"]) && IsGrounded)
                {
                    TorsoRB.AddForce(((((StrongLeft.transform.up + StrongRight.transform.up) / 2) * (jumpHeight * 2000)) * Math.Min(pressCharge, 2f)));
                }
                pressCharge = 1f;
                pressing = false;
            }
    }

    IEnumerator MoveRight(float seconds)
    {
        rightLegRB.AddForce(Vector2.right * (speed * 1000) * Time.deltaTime);
        yield return new WaitForSeconds(seconds);
        leftLegRB.AddForce(Vector2.right * (speed * 1000) * Time.deltaTime);
    }

    IEnumerator MoveLeft(float seconds)
    {
        rightLegRB.AddForce(Vector2.left * (speed * 1000) * Time.deltaTime);
        yield return new WaitForSeconds(seconds);
        leftLegRB.AddForce(Vector2.left * (speed * 1000) * Time.deltaTime);
    }

    // NEW Ground Check
    public bool IsGrounded
    {
        get
        {
            // Check if either foot is touching the ground
            bool leftGrounded = Physics2D.OverlapCircle(leftFoot.position, groundCheckDistance, jumpableLayer);
            bool rightGrounded = Physics2D.OverlapCircle(rightFoot.position, groundCheckDistance, jumpableLayer);

            return leftGrounded || rightGrounded;
        }
    }

    //For visualization
    private void OnDrawGizmosSelected()
    {
        if (leftFoot != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(leftFoot.position, groundCheckDistance);
        }

        if (rightFoot != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(leftFoot.position, groundCheckDistance);
        }
    }
}