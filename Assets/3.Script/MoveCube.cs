using UnityEngine;
using System;


public class MoveCube : MonoBehaviour
{ 
    void Update()
    {
        transform.Rotate(new Vector3(12f, 40f, 25f) * Time.deltaTime);    
    }
}
