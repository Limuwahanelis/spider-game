using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbStepperManager : MonoBehaviour
{
    public Vector3 avgNormals;
    [SerializeField] LayerMask _climbingMask;
    [SerializeField] LimbStepper[] _steppers = new LimbStepper[8];

    private bool[] _isFirstTimeLerping = new bool[8];
    [SerializeField] Transform[] _safeZoneRaycastOrigins = new Transform[8];
    [SerializeField] float _checkDistance;
    private void Start()
    {

    }
    //public void RotateBody()
    //{
    //    //_playerMovement.PlayerRB.rotation = Quaternion.Slerp(_playerMovement.PlayerRB.rotation, _helper.rotation * _helperRotationOffset, Time.deltaTime * _rotateSpeed);
    //    Vector3 avgNormal = Vector3.zero;
    //    for (int i = 0; i < _allHitNormals.Length; i++)
    //    {
    //        avgNormal += _allHitNormals[i];
    //    }
    //    avgNormal = avgNormal / _allHitNormals.Length;
    //    Debug.Log(avgNormal);
    //    //_rotationHelper.rotation = Quaternion.LookRotation(-avgNormal);
    //   // _playerMovement.PlayerRB.rotation = Quaternion.Slerp(_playerMovement.PlayerRB.rotation, _rotationHelper.rotation * _helperRotationOffset, Time.deltaTime * _rotateSpeed);
    //    //_playerMovement.PlayerRB.rotation = newRotation;
    //}
    public void SetUpLimbs()
    {
        //if (_isLerping) 
        MoveLimbs();
        Vector3 avgNormal = Vector3.zero;
        for (int i = 0; i < _steppers.Length; i++)
        {
            avgNormal += _steppers[i].LimbNormal;
        }
        avgNormal = avgNormal / _steppers.Length;
        Debug.Log(avgNormal);
        avgNormals = avgNormal;
        // MoveLegsProcedural(Vector2.zero);

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
                //if (!_isLimbLerping[futherestLegIndex])
                //{
                //    //if (endCheck) continue;
                //    _isLimbLerping[futherestLegIndex] = true;
                //    _isFirstTimeLerping[futherestLegIndex] = true;
                //    _startingLerpPositions[futherestLegIndex] = _limbsTargets[futherestLegIndex].position;
                //    Vector3 endLerpPos = _limbsTargets[futherestLegIndex].position + (_safeZones[futherestLegIndex].position - _limbsTargets[futherestLegIndex].position) / 2;
                //    endLerpPos += transform.up * 0.5f;
                //    _endLerpPositions[futherestLegIndex] = endLerpPos;
                //}
            }
        }
    }
    //private void LerpLimb(int limbIndex)
    //{
    //    _lerpValues[limbIndex] += Time.deltaTime;
    //    float totalT = Vector3.Distance(_startingLerpPositions[limbIndex], _endLerpPositions[limbIndex]) / _lerpSpeed;
    //    float t = _lerpValues[limbIndex] / totalT;
    //    _limbsTargets[limbIndex].position = Vector3.Lerp(_startingLerpPositions[limbIndex], _endLerpPositions[limbIndex], t);
    //    if (_isFirstTimeLerping[limbIndex])
    //    {
    //        if (t >= 1)
    //        {
    //            _isFirstTimeLerping[limbIndex] = false;
    //            _startingLerpPositions[limbIndex] = _endLerpPositions[limbIndex];
    //            Debug.DrawRay(_safeZoneRaycastOrigins[limbIndex].position, (_safeZones[limbIndex].position - _safeZoneRaycastOrigins[limbIndex].position).normalized * 3f);
    //            if (Physics.Raycast(_safeZoneRaycastOrigins[limbIndex].position, (_safeZones[limbIndex].position - _safeZoneRaycastOrigins[limbIndex].position).normalized, out hit, 3f, _climbingMask))
    //            {
    //                _endLerpPositions[limbIndex] = hit.point;
    //                _safeZones[limbIndex].position = hit.point;
    //            }
    //            else _endLerpPositions[limbIndex] = _safeZones[limbIndex].position;
    //            _lerpValues[limbIndex] = 0;
    //        }
    //    }
    //    else
    //    {
    //        if (t >= 1)
    //        {
    //            _limbsTargets[limbIndex].position = _endLerpPositions[limbIndex];
    //            _isLimbLerping[limbIndex] = false;
    //            _lerpValues[limbIndex] = 0;
    //        }
    //    }
    //}
    private void MoveLimbs()
    {
        SetFutherestLimb();
        //for (int i = 0; i < 8; i++)
        //{
        //    CastRayForLimb(_limbsTransforms[i], addedHeight, _limbsForwardTrans[i], out Vector3 hitPoint, out _, out Vector3 hitNormal, out _allGroundSphereCastHits[i]);
        //    _allHitNormals[i] = hitNormal;

        //    // Adjust feet when not moving
        //    if (!_isLimbLerping[i])
        //    {
        //        if (Vector3.Distance(_limbsTargets[i].position, hitPoint) > 0.2f) _limbsTargets[i].position = hitPoint;
        //    }
        //    else
        //    {
        //        LerpLimb(i);
        //    }
        //}


    }

    //private void CastRayForLimb(Transform origin, float addedHeight, Transform limbForwardTran, out Vector3 hitpoint, out float currentHitDistance, out Vector3 hitNormal, out bool gotCastHit)
    //{
    //    Vector3 startRay = origin.position - limbForwardTran.up * addedHeight;
    //    Ray limbRay = new Ray(startRay, limbForwardTran.up / 2);
    //    Debug.DrawRay(startRay, limbForwardTran.up / 2, Color.red);
    //    RaycastHit hit;
    //    if (Physics.Raycast(limbRay, out hit, _checkDistance, _climbingMask))
    //    {
    //        hitNormal = hit.normal;
    //        hitpoint = hit.point;
    //        currentHitDistance = hit.distance + addedHeight;
    //        gotCastHit = true;
    //    }
    //    else
    //    {
    //        startRay = origin.position + limbForwardTran.up * 0.15f;
    //        if (CheckLimbRay(startRay, limbForwardTran.right, out hitpoint, out currentHitDistance, out hitNormal, out gotCastHit)) return;
    //        if (CheckLimbRay(startRay, -limbForwardTran.right, out hitpoint, out currentHitDistance, out hitNormal, out gotCastHit)) return;
    //        if (CheckLimbRay(startRay, limbForwardTran.forward, out hitpoint, out currentHitDistance, out hitNormal, out gotCastHit)) return;
    //        if (CheckLimbRay(startRay, -limbForwardTran.forward, out hitpoint, out currentHitDistance, out hitNormal, out gotCastHit)) return;
    //        else
    //        {
    //            hitNormal = Vector3.zero;
    //            gotCastHit = false;
    //            hitpoint = origin.position;
    //            currentHitDistance = 0;
    //        }
    //    }
    //}
    //private bool CheckLimbRay(Vector3 rayStart, Vector3 rayDirection, out Vector3 hitpoint, out float currentHitDistance, out Vector3 hitNormal, out bool gotCastHit)
    //{
    //    hitNormal = Vector3.zero;
    //    gotCastHit = false;
    //    hitpoint = rayStart;
    //    currentHitDistance = 0;
    //    Ray limbRay = new Ray(rayStart, rayDirection);
    //    Debug.DrawRay(rayStart, rayDirection, Color.green);
    //    if (Physics.Raycast(limbRay, out hit, _checkDistance, _climbingMask))
    //    {
    //        hitNormal = hit.normal;
    //        hitpoint = hit.point;
    //        currentHitDistance = hit.distance + addedHeight;
    //        gotCastHit = true;
    //        return true;
    //    }
    //    return false;
    //}
    //private void OnDrawGizmosSelected()
    //{
    //    for (int i = 0; i < 8; i++)
    //    {
    //        Gizmos.DrawWireSphere(_safeZones[i].position, _safeAreaRadius);
    //    }
    //    //Gizmos.DrawLine(_bodyCheck.position, _bodyCheck.position + _bodyCheck.forward * _checkLength);
    //}
}
