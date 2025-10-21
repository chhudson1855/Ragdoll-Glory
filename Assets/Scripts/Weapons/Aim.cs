using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.U2D.IK;

public class Aim : MonoBehaviour
{
    public Transform Target;       // The target to aim at
    public Transform UpperArm;     // The upper arm GameObject
    public Transform LowerArm;     // The lower arm GameObject
    public Transform Body;         // The body GameObject
    private SpriteRenderer sr;
    private HingeJoint2D UpperArmJoint;
    private HingeJoint2D LowerArmJoint;

    public float MotorSpeed = 10f;       // Speed of the motor driving the joints
    public float MaxMotorTorque = 100f;  // Max torque the motor can apply

    void OnDestroy()
    {
        if (UpperArmJoint != null)
        {
            UpperArmJoint.useLimits = false;
            LowerArmJoint.useLimits = false;

            UpperArm.GetComponent<Rigidbody2D>().mass = 1;
            LowerArm.GetComponent<Rigidbody2D>().mass = 1;
        }
    }

    void SetLimits(HingeJoint2D Limb, float value)
    {
        JointAngleLimits2D limits = Limb.limits;
        limits.min = value;
        limits.max = value;
        Limb.limits = limits;
    }

    void FixedUpdate()
    {
        if (Target == null || UpperArmJoint == null || LowerArmJoint == null)
        {
            if (UpperArm == null || LowerArm == null || Body == null)
            {
                return;
            }

            UpperArmJoint = UpperArm.GetComponent<HingeJoint2D>();
            LowerArmJoint = LowerArm.GetComponent<HingeJoint2D>();

            if (UpperArmJoint != null)
            {
                sr = GetComponent<SpriteRenderer>();

                UpperArmJoint.useLimits = true;
                LowerArmJoint.useLimits = true;

                UpperArm.GetComponent<Rigidbody2D>().mass = 0;
                LowerArm.GetComponent<Rigidbody2D>().mass = 0;

                SetLimits(LowerArmJoint, 0);
            }
            
            return;
        }

        Vector2 directionToTarget = Target.position - Body.position;
        float targetAngle = Mathf.Atan2(-directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        if (targetAngle < 0)
            targetAngle += 360;

        targetAngle = (targetAngle + 180) % 360;
        float currentAngle = UpperArmJoint.jointAngle;
        float delta = Mathf.DeltaAngle(currentAngle, targetAngle);
        float smoothedAngle = currentAngle + delta;

        SetLimits(UpperArmJoint, smoothedAngle);

        if (targetAngle > 80 && targetAngle < 270)
        {
            sr.flipY = true;
        }
        else
        {
            sr.flipY = false;
        }
    }
}