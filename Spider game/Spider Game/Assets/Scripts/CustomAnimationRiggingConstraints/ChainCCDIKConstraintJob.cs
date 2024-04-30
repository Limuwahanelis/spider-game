using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public struct ChainCCDIKConstraintJob : IWeightedAnimationJob
{
    public FloatProperty jobWeight { get; set; }

    public NativeArray<PropertyStreamHandle> joints;

    /// <summary>An array of Transform handles that represents the Transform chain.</summary>
    public NativeArray<ReadWriteTransformHandle> chain;
    /// <summary>The Transform handle for the target Transform.</summary>
    public ReadOnlyTransformHandle target;

    /// <summary>The offset applied to the target transform if maintainTargetPositionOffset or maintainTargetRotationOffset is enabled.</summary>
    public AffineTransform targetOffset;

    /// <summary>An array of length in between Transforms in the chain.</summary>
    public NativeArray<float> linkLengths;

    /// <summary>An array of positions for Transforms in the chain.</summary>
    public NativeArray<Vector3> linkPositions;
    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        float w = jobWeight.Get(stream);
        if (w > 0f)
        {
            for (int i = 0; i < chain.Length; ++i)
            {
                var handle = chain[i];
                linkPositions[i] = handle.GetPosition(stream);
                chain[i] = handle;
            }

            int tipIndex = chain.Length - 1;
            

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
    /// <summary>This is used to maintain the current position offset from the tip Transform to target Transform.</summary>
}

public class ChainCCDIKConstraintJobBinder<T> : AnimationJobBinder<ChainCCDIKConstraintJob, T>
    where T : struct, IAnimationJobData, IChainCCDIKConstraintData
{
    public override ChainCCDIKConstraintJob Create(Animator animator, ref T data, Component component)
    {
        Transform[] chain = ConstraintsUtils.ExtractChain(data.Root, data.Tip);

        ChainCCDIKConstraintJob job = new ChainCCDIKConstraintJob();
        //job.joints = new NativeArray<handle>(job.chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        for (int i = 0; i < job.chain.Length; i++)
        {
            //job.joints[i] = animator.BindStreamProperty(chain[i], typeof(MyHingeJoint),);
        }
        return job;
    }

    public override void Destroy(ChainCCDIKConstraintJob job)
    {
        throw new System.NotImplementedException();
    }
}
