using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChecks : MonoBehaviour
{
    [Header("Debug"),SerializeField] bool _showDebug;
    [SerializeField] Transform _groundCheckPos;
    [SerializeField] Transform _groundCheckFromClimb;
    [SerializeField] Vector3 _groundCheckHalfExtents;
    [SerializeField] LayerMask _groundMask;
    [SerializeField] SlopeDetection _slopeDetection;
    [SerializeField] float _groundFromClimbCheckLength;
    public bool IsTouchingGround => isTouchingGround;
   // public bool IsNearGround => _slopeDetection.IsNearGround;
    public bool IsNearGroundFromClimb => _isNearGroundFromClimb;
    public float FloorAngle;
    private bool isTouchingGround;
    private bool _isNearGroundFromClimb;
    // Start is called before the first frame update
    void Start()
    {
        //_slopeDetection.SetGroundMask(_groundMask);
    }

    // Update is called once per frame
    void Update()
    {
        isTouchingGround = Physics.CheckBox(_groundCheckPos.position, _groundCheckHalfExtents, Quaternion.identity, _groundMask);
        _isNearGroundFromClimb = Physics.Raycast(_groundCheckFromClimb.position, _groundCheckFromClimb.forward, _groundFromClimbCheckLength, _groundMask);
        FloorAngle = _slopeDetection.SlopeDetect();
    }
    public Vector3 GetFloorNormal()
    {
        return _slopeDetection.GetFloorNormal();
    }
    private void OnDrawGizmos()
    {
        if (_showDebug)
        {
            Gizmos.DrawWireCube(_groundCheckPos.position, _groundCheckHalfExtents * 2);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_groundCheckFromClimb.position, _groundCheckFromClimb.position + _groundCheckFromClimb.forward * _groundFromClimbCheckLength);
        }
    }

}
