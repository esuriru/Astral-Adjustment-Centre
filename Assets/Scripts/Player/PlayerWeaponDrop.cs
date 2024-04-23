// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponDrop : MonoBehaviour
{
    [SerializeField] private GameObject player;

    // NOTE - prefer name: cameraTransform, also, below serialized fields
    private Transform cam;

    [SerializeField] private float dropForwardForce, dropUpwardForce;

    // NOTE - prefer not to shorten rigidbody
    private Rigidbody rb;

    private void Awake()
    {
        player = gameObject;

        // NOTE - Remove unused code
        //cam = Camera.main;
    }

    private void Start()
    {
        // NOTE - Why not just put this in Awake? Unless that proves
        // problematic, just remove this function and put this in Awake
		cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    public void DropWeapon(GameObject weapon)
    {
        PlayerInventory.DropWeapon(weapon);

        weapon.transform.parent = null;

        // NOTE - Maybe weapon should just store Rigidbody and Animator so you
        // don't have to query for those. Also this can be shortened into one line.
        weapon.GetComponent<Weapon>().enabled = false;
        weapon.GetComponent<Rigidbody>().isKinematic = false;
        weapon.GetComponent<Animator>().enabled = false;
        rb = weapon.GetComponent<Rigidbody>();

        // NOTE - Remove <RangedWeapon>
        if (weapon.TryGetComponent<RangedWeapon>(out RangedWeapon rangedWeapon))
        {
            rangedWeapon.ClearText();
        }

        // NOTE - Just store the player as a Rigidbody
        rb.velocity = player.GetComponent<Rigidbody>().velocity;

        rb.AddForce(cam.forward * dropForwardForce, ForceMode.Impulse);

        float random = Random.Range(-1, 1);

        rb.AddTorque(new Vector3(random, random, random) * 10);

        StartCoroutine(AllowPickUp(weapon));

        weapon.layer = LayerMask.NameToLayer("DroppedWeapons");
        foreach (Transform child in weapon.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("DroppedWeapons");

            // NOTE - Missing braces in one-liner
            foreach (Transform secondaryChild in child.transform)
                secondaryChild.gameObject.layer = LayerMask.NameToLayer("DroppedWeapons");
        }
    }

    private IEnumerator AllowPickUp(GameObject weapon)
    {
        yield return new WaitForSeconds(1);
        
        weapon.GetComponent<Weapon>().inInventory = false;
    }
}
