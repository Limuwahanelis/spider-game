using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
# if UNITY_EDITOR
using UnityEditor;
#endif
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
    private Vector3 _startLocalOrientation;
    private Vector3 _perpendicularAxisLocal;
    private Vector3 _minOrientationLocal;
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
        SetUp();
        //_globalRot = transform.TransformDirection(_rotationAxisLocal);
    }

    public Vector3 GetGlobalRotationAxis()
    {
        //Debug.Log(transform.TransformDirection(_rotationAxisLocal));
        return transform.TransformDirection(_rotationAxisLocal);
    }
    public void RotateAroundAxis(float angle)
    {
        transform.RotateAround(transform.position, GetGlobalRotationAxis(), angle);
    }
    public void SetRotationAxisAsLocal(Vector3 newAxis)
    {
        _rotationAxisLocal = transform.InverseTransformDirection(newAxis);
    }
    public void SetUp()
    {
        Vector3 up;
        up = transform.up;
        _perpendicularAxisLocal = Quaternion.Euler(_rotationAxisLocal) * transform.InverseTransformDirection(up);
        _startLocalOrientation = Quaternion.AngleAxis(_startOrientation, _rotationAxisLocal) * _perpendicularAxisLocal;
        _minOrientationLocal = Quaternion.AngleAxis(_minAngle - _currentAngle, _rotationAxisLocal) * _startLocalOrientation;

    }
#if UNITY_EDITOR
    [CustomEditor(typeof(MyHingeJoint))]
    public class MyHingeEditor:Editor
    {
        private MyHingeJoint _hinge;
        private void OnEnable()
        {
            _hinge = target as MyHingeJoint;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            if(GUILayout.Button("Rotate right 5 degree")) _hinge.RotateAroundAxis(5f);
            if (GUILayout.Button("Rotate left 5 degree")) _hinge.RotateAroundAxis(-5f);
            if (GUILayout.Button("Rotate right 30 degree")) _hinge.RotateAroundAxis(30f);
            if (GUILayout.Button("Rotate left 30 degree")) _hinge.RotateAroundAxis(-30f);
            if (GUILayout.Button("Set down as axis")) _hinge.SetRotationAxisAsLocal(Vector3.down);
            serializedObject.ApplyModifiedProperties();
        }
    }

    private Vector3 GetGlobalPerpendicularRotationAxis()
    {
        return transform.TransformDirection(_perpendicularAxisLocal);
    }
    private Vector3 GetGlobalMinRotationAxis()
    {
        return transform.TransformDirection(_minOrientationLocal);
    }
    void OnDrawGizmosSelected()
    {

        if (!UnityEditor.Selection.Contains(transform.gameObject)) return;

        SetUp();
        Vector3 rotationAxis = GetGlobalRotationAxis();
        Vector3 minOrientation = GetGlobalMinRotationAxis();

        //RotAxis
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position+ rotationAxis);

        // Rotation Limit Arc
        Handles.color = Color.yellow;
        Handles.DrawSolidArc(transform.position, rotationAxis, minOrientation, _maxAngle - _minAngle,0.3f);

        // Current Rotation Used Arc
        Handles.color = Color.red;
        Handles.DrawSolidArc(transform.position, rotationAxis, minOrientation, _currentAngle - _minAngle, 0.1f);
    }

#endif
}
