using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using static UnityEngine.GraphicsBuffer;
[System.Serializable]
public struct ChainCCDIKData : IAnimationJobData, IChainCCDIKConstraintData
{


    [SerializeField] Transform _root;
    [SyncSceneToStream,SerializeField] Transform _tip;

    [SerializeField] List<MyHingeJoint> _joints;


    [SyncSceneToStream, SerializeField] Transform _target;

    [SyncSceneToStream, SerializeField, Range(0f, 1f)] float _chainRotationWeight;
    [SyncSceneToStream, SerializeField, Range(0f, 1f)] float _tipRotationWeight;

    [NotKeyable, SerializeField, Range(1, 50)] int _maxIterations;
    [NotKeyable, SerializeField, Range(0, 0.01f)] float _tolerance;

    public Transform Root { get => _root; set => _root = value; }

    public Transform Tip { get => _tip; set => _tip = value; }


    public Transform Target { get => _target; set => _target = value; }

    public int MaxIterations { get => _maxIterations; set => _maxIterations = value; }

    public float Tolerance { get => _tolerance; set => _tolerance = value; }

    public List<MyHingeJoint> joints { get => _joints; set => _joints = value; }

    bool IAnimationJobData.IsValid()
    {
        if (_root == null || _tip == null || _target == null)
            return false;

        int count = 1;
        Transform tmp = _tip;
        while (tmp != null && tmp != _root)
        {
            tmp = tmp.parent;
            ++count;
        }

        return (tmp == _root && count > 2);
    }

    void IAnimationJobData.SetDefaultValues()
    {
        _root = null;
        _tip = null;
        _target = null;
        _chainRotationWeight = 1f;
        _tipRotationWeight = 1f;
        _maxIterations = 15;
        _tolerance = 0.0001f;
    }
}
public class ChainCCDIKConstraint : RigConstraint<
    ChainCCDIKConstraintJob,
    ChainCCDIKData,
    ChainCCDIKConstraintJobBinder<ChainCCDIKData>>
{
    protected override void OnValidate()
    {
        base.OnValidate();
        if(data.joints == null) data.joints = new List<MyHingeJoint>();
        if( data.joints.Count == 0)
        {
            data.joints.Clear();
            Transform[] chain = ConstraintsUtils.ExtractChain(data.Root, data.Tip);
            for (int i=0;i<chain.Length-1;i++)
            {
                data.joints.Add(chain[i].GetComponent<MyHingeJoint>());
            }
        }
    }
}
