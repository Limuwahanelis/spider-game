using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeDetection : MonoBehaviour
{
    [Header("Debug")] bool _showDebug;
    [SerializeField] Transform _mainSlopeDetectorTrans;
    [SerializeField] Transform _frontSlopeDetectorTrans;
    [SerializeField] float _mainSlopeDetectionRayLength;
    [SerializeField] float _frontSlopeDetectionRayLength;
    [SerializeField] LayerMask _groundMask;
    private Vector3 _floorNormal;
    public bool IsNearGround => _isNearGround;
    private bool _isNearGround;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        SlopeDetect();
    }
    public void SetGroundMask(LayerMask mask)
    {
        _groundMask = mask;
    }
    public float SlopeDetect()
    {
        RaycastHit hit;
        RaycastHit hitFront;
        float angle = 0;
        float rawAngle = 0;
        if (rawAngle <= -75f) return 0;
        if (Physics.Raycast(_mainSlopeDetectorTrans.position, _mainSlopeDetectorTrans.forward, out hit, _mainSlopeDetectionRayLength, _groundMask))
        {
            angle = CalculateWalkAngle(hit.point, hit.normal, out rawAngle);
            _isNearGround = true;
            _floorNormal = hit.normal;

        }
        else _isNearGround = false;
        if (Physics.Raycast(_frontSlopeDetectorTrans.position, _frontSlopeDetectorTrans.forward, out hitFront, _frontSlopeDetectionRayLength, _groundMask))
        {
            angle = CalculateWalkAngle(hitFront.point, hitFront.normal, out rawAngle);
            _floorNormal = hitFront.normal;
        }
        if (rawAngle <= -75f) return 0;
        return angle;
    }
    private float CalculateWalkAngle(Vector3 hitPos, Vector3 normal, out float rawAngle)
    {
        Debug.DrawRay(hitPos, normal, Color.green);
        rawAngle = Vector3.SignedAngle(transform.up, normal, transform.right);
        Vector3 cross = Vector3.Cross(normal, transform.up).normalized; // axis around which slope is rotated
        Debug.DrawRay(transform.position, cross, Color.blue);
        float dot = Vector3.Dot(cross, transform.forward);
        Debug.Log(string.Format("{0} % from {1} = {2}", (dot >= 0 ? (1 - dot) : (1 + dot)) * 100, rawAngle, (dot >= 0 ? (1 - dot) : (1 + dot)) * rawAngle));
        float angle = (dot >= 0 ? (1 - dot) : (1 + dot)) * rawAngle; // how aligned are we with a slope
        return angle;
    }
    public Vector3 GetFloorNormal()
    {
        return _floorNormal;
    }

    private void OnDrawGizmos()
    {
        if (_showDebug)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_mainSlopeDetectorTrans.position, _mainSlopeDetectorTrans.position + _mainSlopeDetectorTrans.forward * _mainSlopeDetectionRayLength);
            Gizmos.DrawLine(_frontSlopeDetectorTrans.position, _frontSlopeDetectorTrans.position + _frontSlopeDetectorTrans.forward * _frontSlopeDetectionRayLength);
        }
    }

}
