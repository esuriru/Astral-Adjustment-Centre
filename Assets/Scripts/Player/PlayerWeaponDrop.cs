using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponDrop : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Transform cam;

    [SerializeField] private float dropForwardForce, dropUpwardForce;

    private Rigidbody rb;

    private void Awake()
    {
        player = gameObject;
        //cam = Camera.main;
    }

    private void Start()
    {
		cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    public void DropWeapon(GameObject weapon)
    {
        PlayerInventory.DropWeapon(weapon);

        weapon.transform.parent = null;

        weapon.GetComponent<Weapon>().enabled = false;
        weapon.GetComponent<Rigidbody>().isKinematic = false;
        weapon.GetComponent<Animator>().enabled = false;
        rb = weapon.GetComponent<Rigidbody>();

        if (weapon.TryGetComponent<RangedWeapon>(out RangedWeapon rangedWeapon))
        {
            rangedWeapon.ClearText();
        }

        rb.velocity = player.GetComponent<Rigidbody>().velocity;

        rb.AddForce(cam.forward * dropForwardForce, ForceMode.Impulse);

        float random = Random.Range(-1, 1);

        rb.AddTorque(new Vector3(random, random, random) * 10);

        StartCoroutine(AllowPickUp(weapon));

        weapon.layer = LayerMask.NameToLayer("DroppedWeapons");
        foreach (Transform child in weapon.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("DroppedWeapons");

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
