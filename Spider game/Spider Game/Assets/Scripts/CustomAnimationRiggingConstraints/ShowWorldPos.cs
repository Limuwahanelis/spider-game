using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowWorldPos : MonoBehaviour
{
    public Vector3 worldPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        worldPos = transform.position;
    }
}
