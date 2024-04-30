using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CCDIKSolver
{
    public static void SolveCCDIK(ref List<Transform>  joints,Transform endEffector, Transform target)
    {
        for (int i = joints.Count - 1; i >= 0; i--)
        {
            Vector3 directionToEffector = (endEffector.position - joints[i].position);
            Vector3 directionToGoal = (target.position - joints[i].position);
            Quaternion aa = new Quaternion();
            aa.SetFromToRotation(directionToEffector, directionToGoal);
            joints[i].rotation = aa;
        }
    }

    public static void SolveCCDIKStep(ref Transform joint, Transform endEffector, Transform target,ref float totalAngle,float minAngle,float maxAngle)
    {
        Vector3 rotationAxis = joint.GetComponent<MyHingeJoint>().GetGlobalRotationAxis();

        Vector3 directionToEffector = (endEffector.position - joint.position);
        Vector3 directionToGoal = (target.position - joint.position);

        // Vector are projected on plane with the same normal to correctly calculate signed angles between them
        Vector3 toEffectorProjected = Vector3.ProjectOnPlane(directionToEffector, rotationAxis);
        Vector3 toTargetProjected = Vector3.ProjectOnPlane(directionToGoal, rotationAxis);

        Debug.DrawLine(joint.position, joint.position + toEffectorProjected, Color.gray);
        Debug.DrawLine(joint.position, joint.position + toTargetProjected, Color.green);

        Debug.DrawLine(joint.position, joint.position + directionToEffector, Color.red);
        Debug.DrawLine(joint.position, joint.position + directionToGoal);

        float angle = Vector3.SignedAngle(toEffectorProjected, toTargetProjected, rotationAxis);
        angle = Mathf.Clamp(totalAngle + angle, minAngle, maxAngle) - totalAngle;
        joint.RotateAround(joint.position,rotationAxis,angle);
        totalAngle += angle;
        
    }
}
