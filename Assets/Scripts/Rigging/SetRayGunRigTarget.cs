using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRayGunRigTarget : MonoBehaviour
{
    [SerializeField] Transform ArmTarget;
    [SerializeField] Transform ArmHint;

    [SerializeField] private Transform GunHandlePos;
    [SerializeField] private Transform GunHintPos;
    
    void Update()
    {
        if (GunHandlePos == null)
        {
            GunHandlePos = GameObject.FindGameObjectWithTag("RayGunHandle").transform;
        }

        if (GunHintPos == null)
        {
            GunHintPos = GameObject.FindGameObjectWithTag("RayGunHint").transform;
        }
        
        // ArmTarget.position = Vector3.Lerp(ArmTarget.position, GunHandlePos.position, Time.deltaTime * 50);
        // ArmHint.position = Vector3.Lerp(ArmHint.position, GunHintPos.position, Time.deltaTime * 50);

        ArmTarget.position = GunHandlePos.position;
        ArmHint.position = GunHintPos.position;


    }

}
