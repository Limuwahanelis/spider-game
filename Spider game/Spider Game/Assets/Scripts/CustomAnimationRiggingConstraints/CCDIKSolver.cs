using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public static class CCDIKSolver
{
    #region normal transforms
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
        Debug.DrawLine(joint.position, joint.position + rotationAxis * 2, Color.yellow);

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
    #endregion
    #region Animation rigging
    public static bool SolveCCDIK(ref AnimationStream stream, float tolerance, int maxIterations, ref NativeArray<ReadWriteTransformHandle> chain, ReadOnlyTransformHandle target, ReadWriteTransformHandle endEffector,ref NativeArray<Vector3> jointsAxis,
       ref NativeArray<float> jointsCurrentAngle, NativeArray<float> jointsMinAngle, NativeArray<float> jointsMaxAngle, bool debug = false)
    {
        int iterations = 0;
        float distance = Vector3.Distance(endEffector.GetPosition(stream), target.GetPosition(stream));
        Quaternion[] oldRotations = new Quaternion[chain.Length];


        while (iterations <= maxIterations && distance > tolerance)
        {

            oldRotations[0] = chain[0].GetRotation(stream);
            int k = 0;
            for (int i = chain.Length; i > 0;)
            {
                if(i==chain.Length-1)
                {
                    for(int j=0;j<chain.Length;j++)
                    {
                        oldRotations[j]= chain[j].GetRotation(stream);
                    }
                }
                var handle = chain[k];
                float currentAngle = jointsCurrentAngle[k];
                Vector3 jointAxis = jointsAxis[k];
                SolveCCDIKStep(ref stream, ref handle, endEffector, target,ref jointAxis, ref currentAngle, jointsMinAngle[k], jointsMaxAngle[k], oldRotations[k],debug);
                jointsAxis[k] = jointAxis;
                jointsCurrentAngle[k] = currentAngle;
                i--;
                k = i;

            }
            distance = Vector3.Distance(endEffector.GetPosition(stream), target.GetPosition(stream));
            iterations++;
        }
        
        if (iterations >= maxIterations && distance > tolerance) return false;
        return true;
    }
    public static void SolveCCDIKStep(ref AnimationStream stream,ref ReadWriteTransformHandle joint, ReadWriteTransformHandle endEffector, ReadOnlyTransformHandle target,ref Vector3 rotationAxis ,ref float totalAngle, float minAngle, float maxAngle,Quaternion oldRotation,bool debug)
    {
        Vector3 directionToEffector = (endEffector.GetPosition(stream) - joint.GetPosition(stream));
        Vector3 directionToGoal = (target.GetPosition(stream) - joint.GetPosition(stream));
        Quaternion diff = joint.GetRotation(stream)*Quaternion.Inverse(oldRotation);
        Vector3 rotatedAxis = diff * rotationAxis;
        //Debug.Log($"diff in ax: {rotatedAxis - rotationAxis}");
        rotationAxis = rotatedAxis;
        // Vector are projected on plane with the same normal in order to correctly calculate signed angles between them
        Vector3 toEffectorProjected = Vector3.ProjectOnPlane(directionToEffector, rotatedAxis);
        Vector3 toTargetProjected = Vector3.ProjectOnPlane(directionToGoal, rotatedAxis);

        if (debug)
        {
            Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + toEffectorProjected, Color.magenta);
            Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + toTargetProjected, Color.green);

            Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + rotatedAxis * 2, Color.yellow);
            Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + directionToEffector, Color.red);
            Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + directionToGoal);
        }

        if (directionToGoal == Vector3.zero || directionToEffector == Vector3.zero) return;
        float angle = Vector3.SignedAngle(toEffectorProjected, toTargetProjected, rotatedAxis);
        angle = Mathf.Clamp(totalAngle + angle, minAngle, maxAngle) - totalAngle;

        Quaternion rot = Quaternion.AngleAxis(angle, rotatedAxis);
        Quaternion myRot = joint.GetRotation(stream);

        joint.SetRotation(stream,rot * myRot);
        totalAngle += angle;
    }
    public static void SolveCCDIKFrame(ref AnimationStream stream, ref NativeArray<ReadWriteTransformHandle> chain, ReadWriteTransformHandle target, ref ReadWriteTransformHandle endEffector, ref NativeArray<Vector3> jointsAxis,
       ref NativeArray<float> jointsCurrentAngle, NativeArray<float> jointsMinAngle, NativeArray<float> jointsMaxAngle, int k)
    {
        var handle = chain[k];
        float currentAngle = jointsCurrentAngle[k];
        Vector3 axis = jointsAxis[k];
        SolveCCDIKStepFrame(ref stream, ref handle, ref endEffector, target, ref axis, ref currentAngle, jointsMinAngle[k], jointsMaxAngle[k]);
        jointsCurrentAngle[k] = currentAngle;
        chain[k] = handle;
    }
    public static void SolveCCDIKStepFrame(ref AnimationStream stream, ref ReadWriteTransformHandle joint, ref ReadWriteTransformHandle endEffector, ReadWriteTransformHandle target, ref Vector3 rotationAxis, ref float totalAngle, float minAngle, float maxAngle)
    {
        //Vector3 rotationAxis = joint.GetComponent<MyHingeJoint>().GetGlobalRotationAxis();

        Vector3 directionToEffector = (endEffector.GetPosition(stream) - joint.GetPosition(stream));
        Vector3 directionToGoal = (target.GetPosition(stream) - joint.GetPosition(stream));

        // Vector are projected on plane with the same normal to correctly calculate signed angles between them
        Vector3 toEffectorProjected = Vector3.ProjectOnPlane(directionToEffector, rotationAxis);
        Vector3 toTargetProjected = Vector3.ProjectOnPlane(directionToGoal, rotationAxis);

        Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + toEffectorProjected, Color.magenta);
        Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + toTargetProjected, Color.green);

        Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + rotationAxis * 2, Color.yellow);
        Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + directionToEffector, Color.red);
        Debug.DrawLine(joint.GetPosition(stream), joint.GetPosition(stream) + directionToGoal);

        if (directionToGoal == Vector3.zero || directionToEffector == Vector3.zero) return;
        float angle = Vector3.SignedAngle(toEffectorProjected, toTargetProjected, rotationAxis);
        angle = Mathf.Clamp(totalAngle + angle, minAngle, maxAngle) - totalAngle;

        Quaternion rot = Quaternion.AngleAxis(angle, rotationAxis);
        Quaternion myRot = joint.GetRotation(stream);
        joint.SetRotation(stream, joint.GetRotation(stream) * Quaternion.Inverse(myRot) * rot * myRot);
        totalAngle += angle;
    }
    #endregion
}
