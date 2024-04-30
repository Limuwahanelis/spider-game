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
    /// <summary>An array of Transform handles that represents the Transform chain.</summary>
    public NativeArray<ReadWriteTransformHandle> chain;
    /// <summary>The Transform handle for the target Transform.</summary>
    public ReadWriteTransformHandle target;

    /// <summary> Tip of the chain which is used to determine distance from target </summary>
    //public ReadOnlyTransformHandle endEffector;

    /// <summary>The offset applied to the target transform if maintainTargetPositionOffset or maintainTargetRotationOffset is enabled.</summary>
    public AffineTransform targetOffset;

    /// <summary>An array of length in between Transforms in the chain.</summary>
    public NativeArray<float> linkLengths;

    /// <summary>An array of positions for Transforms in the chain.</summary>
    public NativeArray<Vector3> linkPositions;

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
            CCDIKSolver.SolveCCDIK(stream,cache.GetRaw(toleranceIdx) ,(int)cache.GetRaw(maxIterationsIdx),ref chain, target,ref jointsAxis,ref jointsCurrentAngle, jointsMinAngle, jointsMaxAngle);

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
        data.joints = new List<MyHingeJoint>();
        
        ChainCCDIKConstraintJob job = new ChainCCDIKConstraintJob();
        job.chain = new NativeArray<ReadWriteTransformHandle>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.jointsAxis = new NativeArray<Vector3>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.jointsMinAngle = new NativeArray<float>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.jointsMaxAngle = new NativeArray<float>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.jointsCurrentAngle = new NativeArray<float>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        
        job.target = ReadWriteTransformHandle.Bind(animator, data.Target);

        job.hh = FloatProperty.Bind(animator, component, "m_Data.hh");
        for (int i = 0; i < job.chain.Length; i++)
        {
            MyHingeJoint joint = chain[i].GetComponent<MyHingeJoint>();
            //job.gg = FloatProperty.Bind(animator, joint, "MaxAngle");
            data.joints.Add(joint);
            string objectName = joint.name;
            job.chain[i] = ReadWriteTransformHandle.Bind(animator, chain[i]);
            job.jointsAxis[i] = chain[i].GetComponent<MyHingeJoint>().GetGlobalRotationAxis();//Vector3Property.Bind(animator, data.joints[i], "RotationAxisGlobal");  
            job.jointsMaxAngle[i] = chain[i].GetComponent<MyHingeJoint>().MaxAngle;
            job.jointsMinAngle[i] = chain[i].GetComponent<MyHingeJoint>().MinAngle;
            job.jointsCurrentAngle[i] = job.jointsMaxAngle[i] = chain[i].GetComponent<MyHingeJoint>().StartingAngle;
            //job.joints[i] = animator.BindStreamProperty(chain[i], typeof(MyHingeJoint),);
        }

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
        for(int i=0;i<data.joints.Count;i++) 
        {
            job.jointsAxis[i] = data.joints[i].GetGlobalRotationAxis();
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
