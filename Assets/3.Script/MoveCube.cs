using UnityEngine;
using System;


public class MoveCube : MonoBehaviour
{ 
    void Update()
    {
        transform.Rotate(new Vector3(10f, 20f, 300000f) * Time.deltaTime);    
    }
}
