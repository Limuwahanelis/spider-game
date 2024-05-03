using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbStepperManager : MonoBehaviour
{
    public Vector3 avgNormals;
    [SerializeField] LayerMask _climbingMask;
    [SerializeField] LimbStepper[] _steppers = new LimbStepper[8];
    [SerializeField] float _checkDistance;
    private void Start()
    {
       
    }
    private void Update()
    {
        
    }
    private void DD()
    {
        Debug.Log("Fired");
            
    }
    public void SetUpLimbs()
    {
        MoveLimbs();
        Vector3 avgNormal = Vector3.zero;
        for (int i = 0; i < _steppers.Length; i++)
        {
            avgNormal += _steppers[i].LimbNormal;
        }
        avgNormal = avgNormal / _steppers.Length;
        Debug.Log(avgNormal);
        avgNormals = avgNormal;

    }
    private void SetFutherestLimb()
    {
        float furtherestDistance = _steppers[0].Distance;
        int futherestLegIndex = -1;
        bool endCheck = false;
        for (int i = 0; i < 8; i++)
        {
            if (_steppers[i].IsLerping) endCheck = true;
            if (endCheck) break;
            float distance = _steppers[i].Distance;
            if (_steppers[i].ShouldLerp)
            {
                if (distance >= furtherestDistance)
                {
                    furtherestDistance = distance;
                    futherestLegIndex = i;
                }
            }
        }
        if (!endCheck)
        {
            if (futherestLegIndex != -1)
            {
                _steppers[futherestLegIndex].MoveLimb();
            }
        }
    }
    private void MoveLimbs()
    {
        SetFutherestLimb();
    }
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
