using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;
using Unity.Mathematics;

public struct HelloWorldConstrainJob : IWeightedAnimationJob
{
    public ReadWriteTransformHandle constrained;
    public ReadWriteTransformHandle source;
    public FloatProperty jobWeight { get; set; }

    public void ProcessAnimation(AnimationStream stream) 
    {
        float w = jobWeight.Get(stream);
        if (w > 0f)
        {
            constrained.SetPosition(stream, math.lerp(constrained.GetPosition(stream), -source.GetPosition(stream), w));
        }
    }

    public void ProcessRootMotion(AnimationStream stream){ } // don't need to care about that, just have to implement that for it to work.
}
public interface IHelloWorldConstraintData
{
    Transform ConstrainedObject { get; }
    Transform SourceObject { get; }
}
public class HelloWorldConstrainJobBinder<T> : AnimationJobBinder<HelloWorldConstrainJob, T>
    where T : struct, IAnimationJobData, IHelloWorldConstraintData
{
    public override HelloWorldConstrainJob Create(Animator animator, ref T data, Component component)
    {
        return new HelloWorldConstrainJob()
        {
            constrained = ReadWriteTransformHandle.Bind(animator, data.ConstrainedObject),
            source = ReadWriteTransformHandle.Bind(animator, data.SourceObject)
        };
    }

    public override void Destroy(HelloWorldConstrainJob job)
    {
        
    }
}
