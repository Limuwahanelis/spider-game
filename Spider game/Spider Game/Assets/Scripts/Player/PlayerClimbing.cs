
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// TO DO calculate distance between target and limb and move targt if its too far
// TO DO adjust ik at corners

public class PlayerClimbing : MonoBehaviour
{
    public Action OnStartClimbing;
    public Action<Vector3> OnFoundFloor;

    [Header("Objects")]

    [SerializeField] PlayerMovement _playerMovement;
    [SerializeField] Camera _playerCamera;
    [SerializeField] LayerMask _climbingMask;
    [SerializeField] Rig _playerRig;

    [Header("Transforms")]

    [SerializeField] Transform _player;
    [SerializeField] Transform _spineTarget;
    [SerializeField] Transform _bodyCheck;
    [SerializeField] Transform _startClimbingTran;

    [SerializeField] Transform[] _limbsTransforms=new Transform[8];
    [SerializeField] Transform[] _limbsTargets = new Transform[8];
    [SerializeField] Transform[] _limbsForwardTrans = new Transform[8];

    [Header("Variables")]

    [SerializeField] float _startClimbingSpeed;
    [SerializeField] float _checkLength;
    [SerializeField, Range(0, 1)] float _wallCheckAcceptance;
    [SerializeField] float _climbSpeed;
    [SerializeField] float _checkDistance;
    [SerializeField] float[] _limbsOffsets = new float[8];


    private Vector3[] _allHitNormals;

    private bool[] _allGroundSphereCastHits;
    private RaycastHit hit;
    private float addedHeight = 0.3f;
    private Quaternion _helperRotationOffset;
    Coroutine _climbingCor;

    //////////////////////////////////////
    [SerializeField] float _rayTowardsWall = 1f;
    [SerializeField] float _headHelperOffsetFromFeet = 1.6f;
    [SerializeField] float _spineHelperOffsetFromFeet = 1f;
    [SerializeField] float _rayTowardsMoveDir = 1;
    [SerializeField] float _rotateSpeed = 2f;
    [SerializeField] float _positionOffset;
    [SerializeField] float _offsetFromWall;
    Quaternion _previousRotation;
    Transform _headHelper;
    Transform _helper;
    Transform _spineHelper;
    Vector3 _targetPos;
    Vector3 _startPos;
    bool _isLerping;
    int jumpIterations = 0;
    int climbMovesInJump = 4;
    Vector2 _jumpDirection;
    float t;
    Vector2 _lastClimbingDirection = Vector2.zero;

    bool _isJumping = false;

    ////// Legs
    [SerializeField] float _safeAreaRadius;
    [SerializeField] Transform[] _safeZones;
    float totalTime = 0f;
    [SerializeField] float _lerpSpeed = 1;
    private bool[] _isLimbLerping = new bool[8];
    private Vector3[] _startingLerpPositions= new Vector3[8];
    private Vector3[] _endLerpPositions = new Vector3[8];
    private float[] _lerpValues = new float[8];
    private bool[] _isFirstTimeLerping = new bool[8];
    [SerializeField] Transform[] _safeZoneRaycastOrigins= new Transform[8];
    // Start is called before the first frame update
    void Start()
    {


        _allGroundSphereCastHits = new bool[8];
        _allHitNormals = new Vector3[8];

    }
    public void InitHelper()
    {
        if (_helper != null)
        {
            _helper.position = transform.position;
            _helper.rotation = Quaternion.LookRotation(-transform.up, transform.forward); 
           // _headHelper.rotation = Quaternion.LookRotation(-transform.up, transform.forward);// _helper.rotation;
            _spineHelper.rotation = _helper.rotation;
            _spineHelper.position = _headHelper.position;
            _headHelper.position = _helper.position + _helper.up * _headHelperOffsetFromFeet;
            return;
        }
        _helper = new GameObject().transform;
        _spineHelper = new GameObject().transform;
        _headHelper = new GameObject().transform;
        _helper.name = "climb_helper";
        _spineHelper.name = "climb_spine_helper";
        _headHelper.name = "climb_head_helper";
        _helper.position = transform.position+_offsetFromWall*transform.up;
        _helper.rotation = Quaternion.LookRotation(-transform.up, transform.forward);
        _headHelper.position = _helper.position + _helper.up * _headHelperOffsetFromFeet;
        _helperRotationOffset = _player.rotation * Quaternion.Inverse(_helper.rotation);
        _spineHelper.position = _headHelper.position;
        _spineHelper.rotation = _helper.rotation;
    }

    public void Climb(Vector2 direction)
    {
        if (_isJumping)
        {
            Move();
            if (_isLerping == false)
            {
                jumpIterations++;
                PrepareToMove(_jumpDirection);
                if (jumpIterations > climbMovesInJump)
                {
                    _isJumping = false;
                    jumpIterations = 0;
                    _climbSpeed /= 2;
                }
            }
            return;
        }
        if (Vector2.Dot(direction, _lastClimbingDirection) == -1)
        {
            MoveInOppositeDirection(direction);
        }
        else if (_lastClimbingDirection != direction && direction != Vector2.zero)
        {
            _isLerping = false;

        }
        if (!_isLerping)
        {
            PrepareToMove(direction);
        }
        else
        {
            Move();
        }
    }
    #region climb
    private void MoveInOppositeDirection(Vector2 currentDir)
    {
        Vector3 tmp = _startPos;
        _startPos = _helper.position;
        _helper.position = tmp;
        _helper.rotation = _previousRotation;
        _targetPos = _helper.position;
        _lastClimbingDirection = currentDir;
        t = 1 - t;
    }
    private void PrepareToMove(Vector2 direction)
    {
        Vector3 h = _helper.right * direction.x;
        Vector3 v = _helper.up * direction.y;
        Vector3 moveDir = (h + v).normalized;
        if (direction == Vector2.zero) return;
        bool canMove = CanMove(moveDir);
        if (!canMove) return;
        _headHelper.position = _helper.position + _helper.up * _headHelperOffsetFromFeet;
        _headHelper.rotation = _helper.rotation;
        _spineHelper.position = _helper.position + _helper.up * _spineHelperOffsetFromFeet;
        _spineHelper.rotation = _helper.rotation;
        RaycastHit hit;
        if (Physics.Raycast(_spineHelper.position, _spineHelper.forward, out hit, 3f, _climbingMask))
        {
            _spineHelper.rotation = Quaternion.LookRotation(-hit.normal);
        }
        _lastClimbingDirection = direction;
        t = 0;
        _isLerping = true;
        _previousRotation = _helper.rotation;
        _startPos = transform.position;
        _targetPos = _helper.position;
    }
    private void Move()
    {
        t += Time.deltaTime * _climbSpeed;
        if (t > 1)
        {
            t = 1;
            _isLerping = false;
        }
        Vector3 cp = Vector3.Lerp(_startPos, _targetPos, t);

        Debug.DrawRay(_player.position - _player.forward, _player.forward * 3f);
        _playerMovement.PlayerRB.MovePosition(cp);
        //_playerMovement.PlayerRB.rotation = Quaternion.Slerp(_playerMovement.PlayerRB.rotation, _helper.rotation * _helperRotationOffset, Time.deltaTime * _rotateSpeed);
        
        //_spineTarget.rotation = Quaternion.Slerp(_spineTarget.rotation, _spineHelper.rotation, Time.deltaTime);
    }
    #endregion
    public void Jump(Vector2 direction)
    {
        _climbSpeed *= 2;
        _jumpDirection = direction;
        PrepareToMove(_jumpDirection);
        _isJumping = true;
    }
    bool CanMove(Vector3 moveDir)
    {
        Vector3 origin = transform.position;

        float dis = _rayTowardsMoveDir;
        Vector3 dir = moveDir;
        Debug.DrawRay(origin, dir * dis, Color.red);
        RaycastHit hit;

        if (Physics.Raycast(origin, dir, out hit, dis, _climbingMask))// checks if there is wall perpendicualar to us
        {
            _helper.position = PosWithOffset(origin, hit.point);
            _helper.rotation = Quaternion.LookRotation(-hit.normal);
            Debug.Log("perpendicular wall");
            return true;
        }

        //Debug.DrawRay(_headHelper.position, _headHelper.forward * 0.5f, Color.blue);
        //RaycastHit headHelperHit;
        //if (Physics.Raycast(_headHelper.position, _headHelper.forward, out headHelperHit, 0.5f, _climbingMask))
        //{
        //    float angle = Vector3.SignedAngle(headHelperHit.normal, Vector3.up, Vector3.Cross(headHelperHit.normal, Vector3.up));
        //    Debug.Log(angle);
        //    if (angle < 46f)
        //    {

        //        Debug.Log("should vault");
        //        StopClimbing();
        //        OnFoundFloor?.Invoke(headHelperHit.point);
        //        return false;
        //    }
        //}

        //Vector3 headHelperOrigin = _headHelper.position + _headHelper.forward * 0.5f;
        //Debug.DrawRay(headHelperOrigin, Vector3.down * 1f, Color.cyan);
        //if (Physics.Raycast(headHelperOrigin, Vector3.down, out headHelperHit, 1f, _climbingMask))
        //{
        //    float angle = Vector3.SignedAngle(headHelperHit.normal, Vector3.up, Vector3.Cross(headHelperHit.normal, Vector3.up));
        //    Debug.Log(angle);
        //    if (angle < 46f)
        //    {
        //        Debug.Log("should vault cor");
        //        StopClimbing();
        //        OnFoundFloor?.Invoke(headHelperHit.point);
        //        return false;
        //    }

        //}

        origin += moveDir * dis;

        dir = _helper.forward;
        float dis2 = _rayTowardsWall;
        //raycast forwards towards the wall
        Debug.DrawRay(origin, dir * dis2, Color.blue);
        if (Physics.Raycast(origin, dir, out hit, dis2, _climbingMask))
        {
            _helper.position = PosWithOffset(origin, hit.point);
            _helper.rotation = Quaternion.LookRotation(-hit.normal);
            return true;
        }

        //origin = origin + (dir * dis2);
        origin = origin + (dir * dis);
        dir = -moveDir;
        Debug.DrawRay(origin, dir, Color.black);
        // raycast for around corners 
        if (Physics.Raycast(origin, dir, out hit, _rayTowardsWall))
        {
            Debug.Log("coee");
            _helper.position = PosWithOffset(origin, hit.point);
            _helper.rotation = Quaternion.LookRotation(-hit.normal);
            return true;
        }

        origin += dir * dis2;
        dir = -Vector3.up;

        Debug.DrawRay(origin, dir, Color.yellow);
        if (Physics.Raycast(origin, dir, out hit, dis2, _climbingMask))
        {
            if (Vector3.Dot(Vector3.up, hit.normal) >= 0.95)
            {
                RaycastHit hit2;
                if (!Physics.Raycast(origin, Vector3.up, out hit2, 2, _climbingMask))
                {
                    Debug.Log("vault");
                }
            }
            float angle = Vector3.Angle(_helper.up, hit.normal);
            if (angle < 40)
            {
                _helper.position = PosWithOffset(origin, hit.point);
                _helper.rotation = Quaternion.LookRotation(-hit.normal);
                return true;
            }
        }

        return false;
    }
    Vector3 PosWithOffset(Vector3 origin, Vector3 target)
    {
        Vector3 direction = origin - target;
        direction.Normalize();
        Vector3 offset = direction * _offsetFromWall;
        return target + offset;
    }
    #region Limbs
    public void RotateBody()
    {
        //_playerMovement.PlayerRB.rotation = Quaternion.Slerp(_playerMovement.PlayerRB.rotation, _helper.rotation * _helperRotationOffset, Time.deltaTime * _rotateSpeed);
        Vector3 avgNormal = Vector3.zero;
        for(int i=0;i< _allHitNormals.Length; i++)
        {
            avgNormal += _allHitNormals[i];
        }
        avgNormal=avgNormal/_allHitNormals.Length;
        Debug.Log(avgNormal);
        _helper.rotation = Quaternion.LookRotation(-avgNormal);
        _playerMovement.PlayerRB.rotation = Quaternion.Slerp(_playerMovement.PlayerRB.rotation, _helper.rotation*_helperRotationOffset, Time.deltaTime * _rotateSpeed);
        //_playerMovement.PlayerRB.rotation = newRotation;
    }
    public void SetUpLimbs()
    {
        //if (_isLerping) 
            MoveLimbs();
       // MoveLegsProcedural(Vector2.zero);

    }
    private void SetFutherestLimb()
    {
        float furtherestDistance = Vector3.Distance(_limbsTargets[0].position, _safeZones[0].position);
        int futherestLegIndex = -1;
        bool endCheck = false;
        for (int i = 0; i < 8; i++)
        {
            if (_isLimbLerping[i]) endCheck = true;
            if (endCheck) break;
            float distance = Vector3.Distance(_limbsTargets[i].position, _safeZones[i].position);
            if (distance > _safeAreaRadius)
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
                if (!_isLimbLerping[futherestLegIndex])
                {
                    //if (endCheck) continue;
                    _isLimbLerping[futherestLegIndex] = true;
                    _isFirstTimeLerping[futherestLegIndex] = true;
                    _startingLerpPositions[futherestLegIndex] = _limbsTargets[futherestLegIndex].position;
                    Vector3 endLerpPos = _limbsTargets[futherestLegIndex].position + (_safeZones[futherestLegIndex].position - _limbsTargets[futherestLegIndex].position) / 2;
                    endLerpPos += transform.up * 0.5f;
                    _endLerpPositions[futherestLegIndex] = endLerpPos;
                }
            }
        }
    }
    private void LerpLimb(int limbIndex)
    {
        _lerpValues[limbIndex] += Time.deltaTime;
        float totalT = Vector3.Distance(_startingLerpPositions[limbIndex], _endLerpPositions[limbIndex]) / _lerpSpeed;
        float t = _lerpValues[limbIndex] / totalT;
        _limbsTargets[limbIndex].position = Vector3.Lerp(_startingLerpPositions[limbIndex], _endLerpPositions[limbIndex], t);
        if (_isFirstTimeLerping[limbIndex])
        {
            if (t >= 1)
            {
                _isFirstTimeLerping[limbIndex] = false;
                _startingLerpPositions[limbIndex] = _endLerpPositions[limbIndex];
                Debug.DrawRay(_safeZoneRaycastOrigins[limbIndex].position, (_safeZones[limbIndex].position - _safeZoneRaycastOrigins[limbIndex].position));
                if (Physics.Raycast(_safeZoneRaycastOrigins[limbIndex].position, (_safeZones[limbIndex].position - _safeZoneRaycastOrigins[limbIndex].position).normalized, out hit, 3f, _climbingMask))
                {
                    _endLerpPositions[limbIndex] = hit.point;
                }
                else _endLerpPositions[limbIndex] = _safeZones[limbIndex].position;
                _lerpValues[limbIndex] = 0;
            }
        }
        else
        {
            if (t >= 1)
            {
                _limbsTargets[limbIndex].position = _endLerpPositions[limbIndex];
                _isLimbLerping[limbIndex] = false;
                _lerpValues[limbIndex] = 0;
            }
        }
    }
    private void MoveLimbs()
    {
        SetFutherestLimb();
        for (int i = 0; i < 8; i++)
        {
            CastRayForLimb(_limbsTransforms[i], addedHeight, _limbsForwardTrans[i], out Vector3 hitPoint, out _, out Vector3 hitNormal, out _allGroundSphereCastHits[i]);
            _allHitNormals[i] = hitNormal;

            // Adjust feet when not moving
            if (!_isLimbLerping[i])
            {
                if (Vector3.Distance(_limbsTargets[i].position, hitPoint) > 0.2f) _limbsTargets[i].position = hitPoint;
            }
            else
            {
                LerpLimb(i);
            }
        }


    }

    private void CastRayForLimb(Transform origin, float addedHeight, Transform limbForwardTran, out Vector3 hitpoint, out float currentHitDistance, out Vector3 hitNormal, out bool gotCastHit)
    {
        Vector3 startRay = origin.position - limbForwardTran.up * addedHeight;
        Ray limbRay = new Ray(startRay, limbForwardTran.up / 2);
        Debug.DrawRay(startRay, limbForwardTran.up / 2, Color.red);
        RaycastHit hit;
        if (Physics.Raycast(limbRay, out hit, _checkDistance, _climbingMask))
        {
            hitNormal = hit.normal;
            hitpoint = hit.point;
            currentHitDistance = hit.distance + addedHeight;
            gotCastHit = true;
        }
        else
        {
            startRay = origin.position + limbForwardTran.up * 0.15f;
            if (CheckLimbRay(startRay, limbForwardTran.right, out hitpoint, out currentHitDistance, out hitNormal, out gotCastHit)) return;
            if (CheckLimbRay(startRay, -limbForwardTran.right, out hitpoint, out currentHitDistance, out hitNormal, out gotCastHit)) return;
            if (CheckLimbRay(startRay, limbForwardTran.forward, out hitpoint, out currentHitDistance, out hitNormal, out gotCastHit)) return;
            if (CheckLimbRay(startRay, -limbForwardTran.forward, out hitpoint, out currentHitDistance, out hitNormal, out gotCastHit)) return;
            else
            {
                hitNormal = Vector3.zero;
                gotCastHit = false;
                hitpoint = origin.position;
                currentHitDistance = 0;
            }
        }
    }
    private bool CheckLimbRay(Vector3 rayStart, Vector3 rayDirection, out Vector3 hitpoint, out float currentHitDistance, out Vector3 hitNormal, out bool gotCastHit)
    {
        hitNormal = Vector3.zero;
        gotCastHit = false;
        hitpoint = rayStart;
        currentHitDistance = 0;
        Ray limbRay = new Ray(rayStart, rayDirection);
        Debug.DrawRay(rayStart, rayDirection, Color.green);
        if (Physics.Raycast(limbRay, out hit, _checkDistance, _climbingMask))
        {
            hitNormal = hit.normal;
            hitpoint = hit.point;
            currentHitDistance = hit.distance + addedHeight;
            gotCastHit = true;
            return true;
        }
        return false;
    }
    #endregion
    private void MoveLegsProcedural(Vector2 direction)
    {
        // from idle to move
        // from move to move
        // from move to idle
        // o.5 forward
        float upTime = 1f;
        float downTime = 2f;
        float backTime = 6f;
        float step = 0.5f;
        // 0,3,4,7
        // 1,2,5,6
        // 
        for(int i=0;i<8;i++) 
        {
            if(i==0 || i==3 || i==4 || i==7)
            {
                if(totalTime<upTime)
                {
                    _limbsTargets[i].position += Vector3.up * step * Time.deltaTime;
                    _limbsTargets[i].position += Vector3.forward * step * Time.deltaTime;
                }
                else if(totalTime< downTime)
                {
                    _limbsTargets[i].position -= Vector3.up * step * Time.deltaTime;
                    _limbsTargets[i].position += Vector3.forward * step * Time.deltaTime;
                }
                else if(totalTime < backTime)
                {
                    _limbsTargets[i].position -= Vector3.forward * step * Time.deltaTime;
                }
            }
            else
            {
                if (totalTime < upTime)
                {
                    _limbsTargets[i].position += Vector3.up * step * Time.deltaTime;
                    _limbsTargets[i].position -= Vector3.forward * step * Time.deltaTime;
                }
                else if (totalTime < downTime)
                {
                    _limbsTargets[i].position -= Vector3.up * step * Time.deltaTime;
                    _limbsTargets[i].position -= Vector3.forward * step * Time.deltaTime;
                }
                else if (totalTime < backTime)
                {
                    _limbsTargets[i].position += Vector3.forward * step * Time.deltaTime;
                }
            }
        }
        totalTime += Time.deltaTime;
        if(totalTime> backTime)
        {
            totalTime = 0;
        }
    }
    private void OnDrawGizmosSelected()
    {
        for(int i=0;i<8;i++)
        {
            Gizmos.DrawWireSphere(_safeZones[i].position,_safeAreaRadius);
        }
        //Gizmos.DrawLine(_bodyCheck.position, _bodyCheck.position + _bodyCheck.forward * _checkLength);
    }
    ///////////////////



}
