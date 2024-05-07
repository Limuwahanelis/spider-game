using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LimbStepperManager : MonoBehaviour
{
    public float AvgHeightDifference=>_avgHeightDifference;
    public float HeightToAngleRatio => _heightToAngleRatio;
    public Vector3 avgNormals;
    [SerializeField] LayerMask _climbingMask;
    [SerializeField] LimbStepper[] _steppers = new LimbStepper[8];
    [SerializeField] float _checkDistance;
    [SerializeField] float _heightToAngleRatio;
    private float _avgHeightDifference;
    private void Start()
    {
       
    }
    private void Update()
    {
        
    }
    private void DD(int limbIndex)
    {
        MoveLimb(limbIndex);
    }
    public void SetUpLimbs()
    {
        Vector3 avgNormal = Vector3.zero;
        for (int i = 0; i < _steppers.Length; i++)
        {
            avgNormal += _steppers[i].LimbNormal;
        }
        avgNormal = avgNormal / _steppers.Length;
        avgNormals = avgNormal;
        _avgHeightDifference = 0;
        Vector3 tmp;
        for (int i = 0; i < _steppers.Length; i++)
        {
            //tmp=Vector3.ProjectOnPlane(_steppers[i].LimbTip.position - transform.position, transform.up);
            tmp = transform.rotation * (_steppers[i].LimbTip.position - transform.position);
            _avgHeightDifference +=(_steppers[i].IsFrontLeg?-1:1)* tmp.y;
        }
        _avgHeightDifference = _avgHeightDifference / _steppers.Length;
    }
    private void MoveLimb(int index)
    {
        if (_steppers[index].IsLerping) return;
        _steppers[index].lerp();
    }
    //private void SetFutherestLimb()
    //{
    //    float furtherestDistance = _steppers[0].Distance;
    //    int futherestLegIndex = -1;
    //    bool endCheck = false;
    //    for (int i = 0; i < 8; i++)
    //    {
    //        if (_steppers[i].IsLerping) endCheck = true;
    //        if (endCheck) break;
    //        float distance = _steppers[i].Distance;
    //        if (_steppers[i].ShouldLerp)
    //        {
    //            if (distance >= furtherestDistance)
    //            {
    //                furtherestDistance = distance;
    //                futherestLegIndex = i;
    //            }
    //        }
    //    }
    //    if (!endCheck)
    //    {
    //        if (futherestLegIndex != -1)
    //        {
    //            _steppers[futherestLegIndex].MoveLimb();
    //        }
    //    }
    //}
    public void SubscribeToJobEvent(ref ChainCCDIKConstraintJob job)
    {
        job.OnNewTargetRequired += DD;
    }
    public void UnsubscribeFromJobEvent(ChainCCDIKConstraintJob job)
    {
        job.OnNewTargetRequired -= DD;
        job.OnJobEnded -= UnsubscribeFromJobEvent;
        Debug.Log("Unsub");
    }
    private void OnDestroy()
    {
    }
}
