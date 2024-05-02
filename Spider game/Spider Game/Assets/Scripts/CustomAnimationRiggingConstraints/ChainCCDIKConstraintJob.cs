using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public struct ChainCCDIKConstraintJob : IWeightedAnimationJob
{
    public FloatProperty jobWeight { get; set; }

    public NativeArray<Vector3> jointsAxis;
    public NativeArray<float> jointsCurrentAngle;
    public NativeArray<float> jointsMinAngle;
    public NativeArray<float> jointsMaxAngle;
    public FloatProperty gg;
    public FloatProperty hh;
    public FloatProperty angle;
    public int index;
    /// <summary>An array of Transform handles that represents the Transform chain.</summary>
    public NativeArray<ReadWriteTransformHandle> chain;

    /// <summary>An array of positions for Transforms in the chain.</summary>
    public NativeArray<Vector3> linkPositions;

    /// <summary>The Transform handle for the target Transform.</summary>
    public ReadWriteTransformHandle target;

    /// <summary> Tip of the chain which is used to determine distance from target </summary>
    public ReadWriteTransformHandle endEffector;

    /// <summary>The offset applied to the target transform if maintainTargetPositionOffset or maintainTargetRotationOffset is enabled.</summary>
    public AffineTransform targetOffset;

    /// <summary>An array of length in between Transforms in the chain.</summary>
    public NativeArray<float> linkLengths;

    public NativeArray<Quaternion> chainRotations;

    /// <summary>An array of length in between Transforms in the chain.</summary>
    public Vector3Property angles;

    public IntProperty numberOfhingesToUpdate;

    /// <summary>CacheIndex to ChainIK tolerance value.</summary>
    /// <seealso cref="AnimationJobCache"/>
    public UnityEngine.Animations.Rigging.CacheIndex toleranceIdx;
    /// <summary>CacheIndex to ChainIK maxIterations value.</summary>
    /// <seealso cref="AnimationJobCache"/>
    public UnityEngine.Animations.Rigging.CacheIndex maxIterationsIdx;
    /// <summary>Cache for static properties in the job.</summary>
    public AnimationJobCache cache;

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        float w = jobWeight.Get(stream);
        if (w > 0f)
        {
            //for(int j=0;j<iteran)
            //Debug.Log($"Axis in job {jointsAxis[0]}");
            //for (int i = 0; i < chain.Length; ++i)
            //
            var handle2 = endEffector;
            Vector3 aa = handle2.GetPosition(stream);
            endEffector = handle2;
            Debug.Log("bef " + aa);
            Debug.Log("bef h" + handle2.GetPosition(stream));
            for(int i=0;i<chain.Length;i++)
            {
                chain[i].SetRotation(stream, chainRotations[i]);
            }
            //}
            CCDIKSolver.SolveCCDIKFrame(ref stream, ref chain, target,ref endEffector, ref jointsAxis, ref jointsCurrentAngle, jointsMinAngle, jointsMaxAngle, index);
            ReadWriteTransformHandle handle = chain[0];
            //CCDIKSolver.RotateAroundAxis(ref stream, ref handle, angle.Get(stream), jointsAxis[0], endEffector);
            //CCDIKSolver.Test2(ref stream, ref chain, angles.Get(stream), jointsAxis);
            //CCDIKSolver.SolveCCDIK(ref stream, cache.GetRaw(toleranceIdx), (int)cache.GetRaw(maxIterationsIdx), ref chain, target, chain[chain.Length-1] ,ref jointsAxis, ref jointsCurrentAngle, jointsMinAngle, jointsMaxAngle);
            //index--;
            //if (index < 0) index = chain.Length - 2;


            //Quaternion rot = Quaternion.AngleAxis(angle.Get(stream), jointsAxis[0]);
            //Quaternion myRot = chain[0].GetRotation(stream);
            //chain[0].SetRotation(stream, chain[0].GetRotation(stream) * Quaternion.Inverse(myRot) * rot * myRot);
            //NativeArray<ReadWriteTransformHandle> sub = chain.GetSubArray(2, 1);
            //CCDIKSolver.Test(ref stream, cache.GetRaw(toleranceIdx), (int)cache.GetRaw(maxIterationsIdx), ref chain, target, ref jointsAxis, ref jointsCurrentAngle, jointsMinAngle, jointsMaxAngle, index, chain[chain.Length - 1]);
            //Debug.Log(chain[3].GetPosition(stream));
            //chain[]
            
            Debug.Log($"{jointsCurrentAngle[0]} {jointsCurrentAngle[1]} {jointsCurrentAngle[2]} {jointsCurrentAngle[3]}");
            index--;
            for(int i=0;i<chainRotations.Length;i++)
            {
                chainRotations[i] = chain[i].GetRotation(stream);
            }
            if (index < 0) index = chain.Length - 1;
            for (int i = 0; i < chain.Length ; i++)
            {

                Debug.DrawLine(chain[i].GetPosition(stream), chain[i].GetPosition(stream) - jointsAxis[i] * 2, Color.cyan);
            }
            //Debug.Log(endEffector.GetPosition(stream));
            //endEffector.SetPosition(stream, endEffector.GetPosition(stream));
            //Debug.DrawLine(chain[0].GetPosition(stream), chain[0].GetPosition(stream) + (chain[chain.Length - 1].GetPosition(stream)-chain[0].GetPosition(stream)  ), Color.red);
        }
        else 
        {
            for (int i = 0; i < chain.Length; ++i)
                AnimationRuntimeUtils.PassThrough(stream, chain[i]);
            AnimationRuntimeUtils.PassThrough(stream, target);
        }
    }


}
public interface IChainCCDIKConstraintData
{
    /// <summary>The root Transform of the ChainCCDIK hierarchy.</summary>
    Transform Root { get; }
    /// <summary>The tip Transform of the ChainCCDIK hierarchy. The tip needs to be a descendant/child of the root Transform.</summary>
    Transform Tip { get; }
    /// <summary>The ChainCCDIK target Transform.</summary>
    Transform Target { get; }

    Vector3 Angles { get; }

    /// <summary>The maximum number of iterations allowed for the ChainCCDIK algorithm to converge to a solution.</summary>
    int MaxIterations { get; }
    /// <summary>
    /// The allowed distance between the tip and target Transform positions.
    /// When the distance is smaller than the tolerance, the algorithm has converged on a solution and will stop.
    /// </summary>
    float Tolerance { get; }

    List<MyHingeJoint> joints { get; set; }

}

public class ChainCCDIKConstraintJobBinder<T> : AnimationJobBinder<ChainCCDIKConstraintJob, T>
    where T : struct, IAnimationJobData, IChainCCDIKConstraintData
{
    public override ChainCCDIKConstraintJob Create(Animator animator, ref T data, Component component)
    {
        Transform[] chain = ConstraintsUtils.ExtractChain(data.Root, data.Tip);
        //data.joints = new List<MyHingeJoint>();
        
        ChainCCDIKConstraintJob job = new ChainCCDIKConstraintJob();
        job.chain = new NativeArray<ReadWriteTransformHandle>(chain.Length-1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.jointsAxis = new NativeArray<Vector3>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.jointsMinAngle = new NativeArray<float>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.jointsMaxAngle = new NativeArray<float>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.jointsCurrentAngle = new NativeArray<float>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.angle = FloatProperty.Bind(animator, component, "m_Data.AngleToRot");
        job.target = ReadWriteTransformHandle.Bind(animator, data.Target);
        job.endEffector = ReadWriteTransformHandle.Bind(animator, chain[chain.Length-1]);
        job.linkPositions = new NativeArray<Vector3>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.angles = Vector3Property.Bind(animator, component, "m_Data.anglesToRotate");
        job.chainRotations =new NativeArray<Quaternion>(chain.Length-1,Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.hh = FloatProperty.Bind(animator, component, "m_Data.hh");
        for (int i = 0; i < job.chain.Length; i++)
        {
            MyHingeJoint joint = chain[i].GetComponent<MyHingeJoint>();
            //job.gg = FloatProperty.Bind(animator, joint, "MaxAngle");
            //data.joints.Add(joint);
            string objectName = joint.name;
            job.chain[i] = ReadWriteTransformHandle.Bind(animator, chain[i]);
            job.jointsAxis[i] = chain[i].GetComponent<MyHingeJoint>().GetGlobalRotationAxis();//Vector3Property.Bind(animator, data.joints[i], "RotationAxisGlobal");  
            job.jointsMaxAngle[i] = chain[i].GetComponent<MyHingeJoint>().MaxAngle;
            job.jointsMinAngle[i] = chain[i].GetComponent<MyHingeJoint>().MinAngle;
            job.jointsCurrentAngle[i] = job.jointsMaxAngle[i] = chain[i].GetComponent<MyHingeJoint>().StartingAngle;
            job.chainRotations[i] = joint.transform.rotation;
            //job.joints[i] = animator.BindStreamProperty(chain[i], typeof(MyHingeJoint),);
        }
        job.index = 0;
        var cacheBuilder = new AnimationJobCacheBuilder();
        job.maxIterationsIdx = cacheBuilder.Add(data.MaxIterations);
        job.toleranceIdx = cacheBuilder.Add(data.Tolerance);
        job.cache = cacheBuilder.Build();
        return job;
    }

    public override void Update(ChainCCDIKConstraintJob job, ref T data)
    {
        job.cache.SetRaw(data.MaxIterations, job.maxIterationsIdx);
        job.cache.SetRaw(data.Tolerance, job.toleranceIdx);
        for (int i = 0; i < data.joints.Count; i++)
        {
            job.jointsAxis[i] = data.joints[i].GetGlobalRotationAxis();
            //if (i == 0) Debug.Log(job.jointsAxis[i]);
        }
    }

    public override void Destroy(ChainCCDIKConstraintJob job)
    {
        job.chain.Dispose();
        job.jointsAxis.Dispose();
        job.jointsMaxAngle.Dispose();
        job.jointsMinAngle.Dispose();
        job.jointsCurrentAngle.Dispose();
        job.cache.Dispose();
    }
}
