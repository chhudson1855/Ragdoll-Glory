using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.SocialPlatforms;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Configuration")]
    public GameObject Target;
    public float Damage = 10f;
    public float Range = 100f;
    public float ImpactForce = 30f;
    public float FireRate = 15f;
    private float nextTimeToFire = 0f;
    public float Shots = 5;
    public bool auto = false;

    [Header("Charge Configuration")]
    public bool Chargeable = false;
    public float MaxCharge = 5f;
    public float ChargeRate = 1f;
    private float CurrentCharge = 0f;

    [Header("Projectile Configuration")]
    public string VFXClassName;
    object VFXClass;
    private KeyCode AttackKey;

    private GameObject localPlayer;

    void LoadVFXclass(string className)
    {
        if (string.IsNullOrEmpty(className)) return;
        Type myType = Type.GetType(className);
        VFXClass = Activator.CreateInstance(myType);

        if (VFXClass.GetType().GetMethod("Initialize") != null)
        {
            MethodInfo initMethod = VFXClass.GetType().GetMethod("Initialize");
            initMethod.Invoke(VFXClass, new object[] { this.gameObject });
        }
    }

    void Start()
    {
        LoadVFXclass(VFXClassName);
    }
    void Update()
    {
        // Fire weapon when W is pressed
        if (auto && Input.GetKey(AttackKey) && Time.time >= nextTimeToFire)
        {
            Debug.Log("Firing weapon");
            nextTimeToFire = Time.time + 1f / FireRate;
            Shoot();
        }
        else if (Input.GetKeyDown(AttackKey) && Time.time >= nextTimeToFire)
        {
            Debug.Log("Firing weapon");
            nextTimeToFire = Time.time + 1f / FireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        // Perform raycast from weapon's position forward
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.right, Range);

        // Create the line
        var lr = new GameObject("ShotLine").AddComponent<LineRenderer>();
        lr.startWidth = lr.endWidth = 0.02f;

        // If the ray hit something, end at the hit point
        if (hit.collider != null)
        {
            lr.SetPositions(new Vector3[] { transform.position, new Vector3(hit.point.x, hit.point.y, transform.position.z) });

            // Apply impact if it's a valid target
            if (hit.transform.parent != null && hit.transform.CompareTag("Player"))
            {
                hit.transform.parent.Find("Body").GetComponent<Rigidbody2D>()
                    .AddForce(-hit.normal * ImpactForce, ForceMode2D.Impulse);
            }
        }
        else
        {
            // If no hit, draw the full-length beam
            lr.SetPositions(new[] { transform.position, transform.position - transform.right * Range });
        }

        Destroy(lr.gameObject, 0.05f); // Remove the line after a short time

        Shots--;

        if (Shots <= 0)
        {
            Destroy(this.gameObject);
        }
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object colliding is the player
        if (collision.gameObject.CompareTag("Player") && transform.parent == null)
        {
            GameObject LeftArm = collision.transform.parent.Find("Lower Left Arm").gameObject;
            Debug.Log("Player picked up weapon!");

            if (LeftArm.transform.childCount > 0)
            {
                return;
            }

            // Parent the weapon to the player
            transform.SetParent(LeftArm.transform, true);

            transform.GetComponent<HingeJoint2D>().useConnectedAnchor = true;
            transform.GetComponent<HingeJoint2D>().connectedBody = LeftArm.GetComponent<Rigidbody2D>();

            transform.localPosition = new Vector3(0, .7f, 0); // adjust offset
            transform.localRotation = Quaternion.Euler(0f, 0f, -90f);

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in players)
            {
                if (player.name != transform.parent.parent.name && player.transform.parent == null)
                {
                    transform.GetComponent<Weapon>().Target = player.transform.Find("Body").gameObject;
                    transform.GetComponent<Aim>().Target = player.transform.Find("Body");
                    Debug.Log("Target set to: " + player.name);
                }
            }

            transform.GetComponent<Aim>().UpperArm = collision.transform.parent.Find("Upper Left Arm");
            transform.GetComponent<Aim>().LowerArm = collision.transform.parent.Find("Lower Left Arm");
            transform.GetComponent<Aim>().Body = collision.transform.parent.Find("Body");
            transform.GetComponent<Rigidbody2D>().simulated = false;

            AttackKey = collision.transform.parent.GetComponent<Config>().AttackKey;
            Debug.Log("Attack key set to: " + AttackKey.ToString());
        }
    }
}