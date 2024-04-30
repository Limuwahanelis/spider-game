using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public class MyHingeJointProperty : IAnimatableProperty<MyHingeJoint>
{
    public PropertyStreamHandle value;

    public MyHingeJoint Get(AnimationStream stream)
    {
        throw new System.NotImplementedException();
    }

    public void Set(AnimationStream stream, MyHingeJoint value)
    {
        throw new System.NotImplementedException();
    }
}
