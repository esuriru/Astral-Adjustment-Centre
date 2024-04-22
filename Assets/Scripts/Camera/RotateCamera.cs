// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    // NOTE - Missing access specifier for both 
    [SerializeField] Transform cameraRotation;

    // NOTE - Remove default comment
    // Update is called once per frame
    void Update()
    {
        transform.rotation = cameraRotation.rotation;
    }
}
