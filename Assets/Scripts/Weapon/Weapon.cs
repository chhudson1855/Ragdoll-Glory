using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using System.Reflection;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Configuration")]
    public GameObject Target;
    public float Damage = 10f;
    public float Range = 100f;
    public float ImpactForce = 30f;
    public float FireRate = 15f;
    private float nextTimeToFire = 0f;

    [Header("Charge Configuration")]
    public bool Chargeable = false;
    public float MaxCharge = 5f;
    public float ChargeRate = 1f;
    private float CurrentCharge = 0f;

    [Header("Projectile Configuration")]
    public string VFXClassName;
    object VFXClass;

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
        if (Input.GetKey(KeyCode.UpArrow) && Time.time >= nextTimeToFire)
        {
            Debug.Log("Firing weapon");
            nextTimeToFire = Time.time + 1f / FireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        // Perform raycast from weapon's position forward
        Debug.DrawRay(transform.position, -transform.right * 100, Color.red, 10f);
        var lr = new GameObject("ShotLine").AddComponent<LineRenderer>(); lr.SetPositions(new[] { transform.position, transform.position - transform.right * Range }); lr.startWidth = lr.endWidth = 0.02f; Destroy(lr.gameObject, 0.05f);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.right, Range, LayerMask.GetMask("Default"));
        if (hit.collider != null)
        {
            Debug.Log("Hit: " + hit.transform.name);

            // Apply force if object has a rigidbody and is not JumpableGround
            if (hit.rigidbody != null && hit.transform.tag != "JumpableGround")
            {
                hit.rigidbody.AddForce(-hit.normal * ImpactForce, ForceMode2D.Impulse);
            }

            // OPTIONAL: Apply damage if target has a health script
            // var health = hit.transform.GetComponent<Health>();
            // if (health != null)
            // {
            //     health.TakeDamage(Damage);
            // }
        }
    }
}