using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solvertest : MonoBehaviour
{
    [SerializeField] List<Transform> transforms = new List<Transform>();
    [SerializeField] Transform target;
    [SerializeField] Transform endEffector;
    [SerializeField] float _tolerance = 0.01f;
    List<float> _angles=new List<float>() {0,0, 0f };
    int index=0;
    // Start is called before the first frame update
    void Start()
    {
        index = transforms.Count - 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(endEffector.position, target.position) <= _tolerance) return;
        //CCDIKSolver.SolveCCDIK(ref transforms, endEffector, target);
        Transform aa = transforms[index];
        float angle = _angles[index];
        CCDIKSolver.SolveCCDIKStep(ref aa, endEffector, target,ref angle,-45,45);
        Debug.DrawLine(aa.transform.position, aa.position + (endEffector.position - aa.position),Color.blue);
        _angles[index] = angle;
        index = index - 1;
        if(index<0) index= transforms.Count - 1;
    }
}
