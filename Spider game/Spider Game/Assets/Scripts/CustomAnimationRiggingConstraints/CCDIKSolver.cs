using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using static UnityEngine.GraphicsBuffer;

public static class CCDIKSolver
{
    public static void SolveCCDIK(ref List<Transform>  joints,Transform endEffector, Transform target,ref List<float> angles,float minAngle,float maxAngle)
    {
        int k = 0;
        for (int i = joints.Count; i >= 0; i--)
        {
            Transform t = joints[k];
            float angle = angles[k];
            SolveCCDIKStep(ref t, endEffector, target, ref angle, minAngle, maxAngle);
            k = i;
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
        Quaternion aa = Quaternion.AngleAxis(angle, rotationAxis);

        //Quaternion.
        //joint.rotation *= aa;
        Vector3 pos = joint.position;
        Quaternion rot = Quaternion.AngleAxis(angle, rotationAxis);
        Vector3 direction = pos-joint.position;
        direction = rot * direction;
        joint.position = joint.position+direction;
        Quaternion myRot = joint.rotation;
        joint.rotation *= Quaternion.Inverse(myRot)*rot*myRot;

        //joint.RotateAround(joint.position,rotationAxis,angle);
        totalAngle += angle;
        
    }
    public static void SolveCCDIK(AnimationStream stream, float tolerance, int maxIterations, ref NativeArray<ReadWriteTransformHandle> chain, ReadWriteTransformHandle target, ref NativeArray<Vector3> jointsAxis,
       ref NativeArray<float> jointsCurrentAngle, NativeArray<float> jointsMinAngle, NativeArray<float> jointsMaxAngle)
    {
        ReadWriteTransformHandle endEffector = chain[chain.Length - 1];
        int iterations = 0;
        float distance = Vector3.Distance(endEffector.GetPosition(stream), target.GetPosition(stream));
        while (iterations <= maxIterations && distance > tolerance)
        {
            int k = 0;
            for (int i = chain.Length - 1; i >= 0;)
            {
                var handle = chain[k];
                float currentAngle = jointsCurrentAngle[k];
                SolveCCDIKStep(stream,ref handle, endEffector, target, jointsAxis[k], ref currentAngle, jointsMinAngle[k], jointsMaxAngle[k]);
                jointsCurrentAngle[k] = currentAngle;
                i--;
                k = i;
            }
            iterations++;
        }
        Debug.Log(jointsCurrentAngle[0]);
    }
    public static void SolveCCDIKStep(AnimationStream stream,ref ReadWriteTransformHandle joint, ReadWriteTransformHandle endEffector, ReadWriteTransformHandle target,Vector3 rotationAxis ,ref float totalAngle, float minAngle, float maxAngle)
    {
        //Vector3 rotationAxis = joint.GetComponent<MyHingeJoint>().GetGlobalRotationAxis();

        Vector3 directionToEffector = (endEffector.GetPosition(stream) - joint.GetPosition(stream));
        Vector3 directionToGoal = (target.GetPosition(stream) - joint.GetPosition(stream));

        // Vector are projected on plane with the same normal to correctly calculate signed angles between them
        Vector3 toEffectorProjected = Vector3.ProjectOnPlane(directionToEffector, rotationAxis);
        Vector3 toTargetProjected = Vector3.ProjectOnPlane(directionToGoal, rotationAxis);

        Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + toEffectorProjected, Color.gray);
        Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + toTargetProjected, Color.green);

        Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + directionToEffector, Color.red);
        Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + directionToGoal);

        if (directionToGoal == Vector3.zero || directionToEffector == Vector3.zero) return;
        float angle = Vector3.SignedAngle(toEffectorProjected, toTargetProjected, rotationAxis);
        angle = Mathf.Clamp(totalAngle + angle, minAngle, maxAngle) - totalAngle;
        Quaternion aa = Quaternion.AngleAxis(angle,rotationAxis);

        Vector3 pos = joint.GetPosition(stream);
        Quaternion rot = Quaternion.AngleAxis(angle, rotationAxis);
        Vector3 direction = pos - joint.GetPosition(stream);
        direction = rot * direction;
        joint.SetPosition(stream,joint.GetPosition(stream) + direction);
        Quaternion myRot = joint.GetRotation(stream);
        //joint.rotation *= Quaternion.Inverse(myRot) * rot * myRot;

        joint.SetRotation(stream,joint.GetRotation(stream)* Quaternion.Inverse(myRot) * rot * myRot);
        //joint.RotateAround(joint.GetPosition(stream), rotationAxis, angle);
        totalAngle += angle;
        
    }
    private static int mod(int n, int m)
    {
        return ((n % m) + m) % m;
    }
}
