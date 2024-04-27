using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DistanceCalculator : MonoBehaviour
{
    public float distance;
    [SerializeField] Transform _objectTomeasure;
    private void Update()
    {
        distance = Vector3.Distance(transform.position, _objectTomeasure.position);
    }
}
