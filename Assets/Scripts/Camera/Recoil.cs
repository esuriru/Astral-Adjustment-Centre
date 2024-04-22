// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoil : MonoBehaviour
{
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;
    
    // Rotations
    private Vector3 currentRotation;
    private Vector3 targetRotation;
   
    void Update()
    {
        // NOTE - Keep consistent gaps between LHS and equal sign
        targetRotation  = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        // NOTE - Don't use fixedDeltaTime in Update
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void GunRecoil(Vector3 recoil)
    {
        targetRotation += new Vector3(recoil.x, Random.Range(-recoil.y, recoil.y), Random.Range(-recoil.z, recoil.z));
    }
}
