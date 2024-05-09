
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

    [SerializeField] LimbStepperManager _man;

    [SerializeField] PlayerMovement _playerMovement;
    [SerializeField] Camera _playerCamera;
    [SerializeField] LayerMask _climbingMask;
    [SerializeField] Rig _playerRig;

    [Header("Transforms")]

    [SerializeField] Transform _player;
    [SerializeField] Transform _spineTarget;
    [SerializeField] Transform _bodyCheck;
    [SerializeField] Transform _startClimbingTran;

    [Header("Variables")]
    [SerializeField] float _climbSpeed;


    private Vector3[] _allHitNormals;

    private Quaternion _helperRotationOffset;

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
    private Transform _rotationHelper;
    // Start is called before the first frame update
    void Start()
    {
    }
    public void InitHelper()
    {
        if (_helper != null)
        {
            _helper.position = transform.position;
            _helper.rotation = Quaternion.LookRotation(-transform.up, transform.forward); 
            _spineHelper.rotation = _helper.rotation;
            _spineHelper.position = _headHelper.position;
            _headHelper.position = _helper.position + _helper.up * _headHelperOffsetFromFeet;
            return;
        }
        _helper = new GameObject().transform;
        _spineHelper = new GameObject().transform;
        _headHelper = new GameObject().transform;
        _rotationHelper = new GameObject().transform;
        _rotationHelper.name = "rotation_helper";
        _helper.name = "climb_helper";
        _spineHelper.name = "climb_spine_helper";
        _headHelper.name = "climb_head_helper";
        _helper.position = transform.position+_offsetFromWall*transform.up;
        _helper.rotation = Quaternion.LookRotation(-transform.up, transform.forward);
        _rotationHelper.rotation = _helper.rotation;
        
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
        Vector3 tmp= _helper.position;
        bool canMove = CanMove(moveDir);
        Debug.Log((_helper.position - tmp).magnitude);
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

        if (Physics.Raycast(origin, dir, out hit, dis, _climbingMask))// checks if there is wall in front of us
        {
            _helper.position = PosWithOffset(origin, hit.point);
            _helper.rotation = Quaternion.LookRotation(-hit.normal);
            Debug.Log("Wall in front of us");
            return true;
        }

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
    public void RotateBody()
    {
        //Debug.Log(_man.AvgHeightDifference);
        Quaternion RotX= Quaternion.AngleAxis(_man.AvgHeightDifference * _man.HeightToAngleRatio,transform.right);

        //_playerMovement.PlayerRB.rotation = Quaternion.Slerp(_playerMovement.PlayerRB.rotation, _helper.rotation * _helperRotationOffset, Time.deltaTime * _rotateSpeed);

        _rotationHelper.rotation = Quaternion.LookRotation(-_man.avgNormals,transform.forward);
        _playerMovement.PlayerRB.rotation = Quaternion.Slerp(_playerMovement.PlayerRB.rotation, _rotationHelper.rotation * _helperRotationOffset, Time.deltaTime * _rotateSpeed);
        Quaternion toRot = RotX ;
        _playerMovement.PlayerRB.rotation = Quaternion.Slerp(_playerMovement.PlayerRB.rotation, toRot, Time.deltaTime * _rotateSpeed);
        //_playerMovement.PlayerRB.rotation = newRotation;
    }



}
