using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[System.Serializable]
public struct HelloWorldData : IAnimationJobData, IHelloWorldConstraintData
{
    public Transform constrainedObject;
    [SyncSceneToStream] public Transform sourceObject;

    public Transform ConstrainedObject { get => constrainedObject; set => constrainedObject = value; }

    public Transform SourceObject { get => sourceObject; set => sourceObject = value; }

     bool IAnimationJobData.IsValid()
    {
        return !(constrainedObject == null || sourceObject == null);
    }

     void IAnimationJobData.SetDefaultValues()
    {
        constrainedObject = null;
        sourceObject = null;
    }
}
public class HelloWorldConstraint : RigConstraint<HelloWorldConstrainJob,HelloWorldData,HelloWorldConstrainJobBinder<HelloWorldData>>
{

}
