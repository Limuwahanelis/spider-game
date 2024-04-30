using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class MyHingeJoint : MonoBehaviour
{
    public float StartingAngle=>_startingAngle;
    public float MinAngle=>_minAngle;
    public float MaxAngle=>_maxAngle;
    public Vector3 RotationAxisGlobal { get => transform.TransformDirection(_rotationAxisLocal); }
    [SerializeField] Vector3 _rotationAxisLocal;
    [SerializeField] float _startingAngle;
    [SerializeField,Range(-180,180)] float _startOrientation;
    [SerializeField, Range(-90, 90)] float _minAngle;
    [SerializeField, Range(-90, 90)] float _maxAngle;

    private float _currentAngle;
    private Transform _angleHelper;
    public void ApplyRotation(float angle)
    {
        angle = angle % 360;
        if(angle ==-180) angle = -angle;
        if(angle>180) angle = -360;
        if (angle < -180) angle += 360;

        angle = Mathf.Clamp(_currentAngle + angle, _minAngle, _maxAngle)- _currentAngle;

        transform.RotateAround(transform.position, GetGlobalRotationAxis(), angle);

        _currentAngle += angle;
    }
    private void Awake()
    {
        //_globalRot = transform.TransformDirection(_rotationAxisLocal);
    }
    public Vector3 GetGlobalRotationAxis()
    {
        Debug.Log(transform.TransformDirection(_rotationAxisLocal));
        return transform.TransformDirection(_rotationAxisLocal);
    }

#if UNITY_EDITOR
    private Vector3 GetGlobalPerpendicularRotationAxis()
    {
        _angleHelper = new GameObject().transform;
        _angleHelper.forward = GetGlobalRotationAxis();
        Vector3 toRet = _angleHelper.up;
        DestroyImmediate(_angleHelper.gameObject);
        return transform.TransformDirection(toRet);
    }
    private Vector3 GetGlobalMinRotationAxis()
    {
        Vector3 toReturn = GetGlobalPerpendicularRotationAxis();
        toReturn = Quaternion.AngleAxis(_minAngle-_currentAngle, GetGlobalRotationAxis()) * toReturn;
        return toReturn;
    }
    void OnDrawGizmosSelected()
    {

        if (!UnityEditor.Selection.Contains(transform.gameObject)) return;


        Vector3 rotationAxis = GetGlobalRotationAxis();
        Vector3 minOrientation = GetGlobalMinRotationAxis();

        //RotAxis
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position+ rotationAxis);

        // Rotation Limit Arc
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawSolidArc(transform.position, rotationAxis, minOrientation, _maxAngle - _minAngle,0.3f);

        // Current Rotation Used Arc
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawSolidArc(transform.position, rotationAxis, minOrientation, _currentAngle - _minAngle, 0.1f);
    }

#endif
}
