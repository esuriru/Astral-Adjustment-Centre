// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// NOTE - Why is this a singleton..?
// NOTE - Also, why does this manage inventory input as well?
// NOTE - This amount of searching objects is alot, maybe
// consider refactoring
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;
    // NOTE - Put these private fields below the public fields
    private GameObject selectedBorder;
    private GridLayoutGroup invGridLayoutGroup;

    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject invGridElementPrefab;
    [SerializeField] private LayerMask targetableLayer;
    [SerializeField] private int invSlots;

    // NOTE - prefer name: cameraTransform
    private Transform cam;

    // NOTE - Unneeded list initialization 
    [SerializeField] private List<Sprite> weaponIcons = new List<Sprite>();

    // NOTE - Why have other static members when you already have a singleton 
    public static GameObject selectBorder;
    public static List<Sprite> invWeaponIcons = new List<Sprite>();
    public static List<GameObject> invWeapons = new List<GameObject>();
    public static List<GameObject> invUISlots = new List<GameObject>();

    public static int maxInvSlots;
    public static int selectedWeaponIndex = 0;
    public static int currWeaponIndex = 0;
    public static int prevWeaponIndex = 0;

    private static bool swapWeapon = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != null)
        {
            Destroy(this);
        }
    }

    public void SetInventory()
    {
        if (GameManager.Instance.currSceneName == "MenuScene")
        {
            inventoryPanel.SetActive(false);

            selectBorder = null;
            invWeaponIcons = new List<Sprite>();
            invWeapons = new List<GameObject>();
            invUISlots = new List<GameObject>();

            maxInvSlots = 0;
            selectedWeaponIndex = 0;
            currWeaponIndex = 0;
            prevWeaponIndex = 0;
        }

        if (SceneManager.GetActiveScene().name == "LevelScene")
        {
            inventoryPanel.SetActive(true);
            
            if (invSlots >= maxInvSlots)
            {
                maxInvSlots = invSlots;
            }

            int newWidth = 150 * maxInvSlots;
            selectedBorder = GameObject.FindGameObjectWithTag("SelectedBorder");
            invGridLayoutGroup = GameObject.FindGameObjectWithTag("InventoryPanel").GetComponent<GridLayoutGroup>();
            cam = GameObject.FindGameObjectWithTag("CameraHolder").transform;
            RectTransform invGridLayoutGroupTransform = invGridLayoutGroup.gameObject.GetComponent<RectTransform>();
            invGridLayoutGroupTransform.sizeDelta = new Vector2(newWidth, 150);

            if (invUISlots.Count < maxInvSlots)
            {
                for (int i = invUISlots.Count; i < maxInvSlots; i++)
                {
                    GameObject newSlot = Instantiate(invGridElementPrefab);
                    newSlot.transform.SetParent(invGridLayoutGroup.transform);
                    newSlot.transform.localScale = new Vector3(1, 1, 1);

                    invUISlots.Add(newSlot);
                }
            }

            // NOTE - Remove this log 
            Debug.Log(maxInvSlots);

            invWeaponIcons = weaponIcons;
            selectBorder = selectedBorder;
            selectedBorder.transform.localPosition = invUISlots[0].transform.localPosition;

            SetUI();
        }
    }
    
    // NOTE - Missing access specifier
    void Update()
    {
        if (SceneManager.GetActiveScene().name != "LevelScene")
        {
            return;
        }

        InputChangeWeapon();
        InputDropWeapon();
        InputSwapWeapon();
    }

    private void InputChangeWeapon()
    {
        // NOTE - Unnest these, also, the code seems very similiar between both
        // if statements
        if (invWeapons.Count > 1)
        {
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            {
                // NOTE - Remove the <RangedWeapon>
                if (invWeapons[currWeaponIndex].TryGetComponent<RangedWeapon>(out RangedWeapon rangedWeapon2))
                {
                    // NOTE - Can be combined into one if statement
                    if (rangedWeapon2.rangedWeaponData.reloading)
                    {
                        return;
                    }
                }

                // NOTE - Bring line 141 out, and turn the last line in the if
                // statement into a ternary operator 
                if (currWeaponIndex >= invWeapons.Count - 1)
                {
                    prevWeaponIndex = currWeaponIndex;
                    currWeaponIndex = 0;
                }
                else
                {
                    prevWeaponIndex = currWeaponIndex;
                    currWeaponIndex++;
                }
                
                invWeapons[prevWeaponIndex].SetActive(false);
                invWeapons[currWeaponIndex].SetActive(true);

                // NOTE - Remove the <RangedWeapon>
                if (invWeapons[currWeaponIndex].TryGetComponent<RangedWeapon>(out RangedWeapon rangedWeapon))
                {
                    if (rangedWeapon.rangedWeaponData.infiniteAmmo)
                    {
                        rangedWeapon.ClearText();
                    }
                    else
                    {
                        rangedWeapon.UpdateAmmo();
                    }
                }

                // NOTE - I'm assuming RangedWeapon is derived off of Weapon. If
                // so, if the previous if statement is true, just use that
                // rangedWeapon that is provided instead of using GetComponent again 
                selectedWeaponIndex = invWeapons[currWeaponIndex].GetComponent<Weapon>().inventoryPosition;
                selectedBorder.transform.localPosition = invUISlots[selectedWeaponIndex].transform.localPosition;

                // Set can attack here
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {
                // NOTE - Bring first line out, and turn the last line in the if
                // statement into a ternary operator 
                if (currWeaponIndex <= 0)
                {
                    prevWeaponIndex = currWeaponIndex;
                    currWeaponIndex = invWeapons.Count - 1;
                }
                else
                {
                    prevWeaponIndex = currWeaponIndex;
                    currWeaponIndex--;
                }
                
                invWeapons[prevWeaponIndex].SetActive(false);
                invWeapons[currWeaponIndex].SetActive(true);

                // NOTE - Remove the <RangedWeapon>
                if (invWeapons[currWeaponIndex].TryGetComponent<RangedWeapon>(out RangedWeapon rangedWeapon))
                {
                    if (rangedWeapon.rangedWeaponData.infiniteAmmo)
                    {
                        rangedWeapon.ClearText();
                    }
                    else
                    {
                        rangedWeapon.UpdateAmmo();
                    }
                }

                // NOTE - Read above
                selectedWeaponIndex = invWeapons[currWeaponIndex].GetComponent<Weapon>().inventoryPosition;
                selectedBorder.transform.localPosition = invUISlots[selectedWeaponIndex].transform.localPosition;

                // Set can attack here
                // Can set get weapon stats here too if need be
            }
        }
    }

    private void InputDropWeapon()
    {
        // NOTE - Unnest this.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            selectedWeaponIndex = invWeapons[currWeaponIndex].GetComponent<Weapon>().inventoryPosition;
            GameObject.FindWithTag("Player").GetComponent<PlayerWeaponDrop>().DropWeapon(invWeapons[currWeaponIndex]);

            // NOTE - Seems to me as the invUISlots should just hold a list of
            // images. You can maybe make a monobehaviour that caches all of the
            // components that are frequently queried
            invUISlots[selectedWeaponIndex].GetComponent<Image>().sprite = null;

            if (currWeaponIndex != 0)
            {
                currWeaponIndex--;
            }

            // NOTE - Unnest this.            
            if (invWeapons.Count > 0)
            {
                invWeapons[currWeaponIndex].SetActive(true);

                // NOTE - Remove <RangedWeapon>, also similar function above.
                if (invWeapons[currWeaponIndex].TryGetComponent<RangedWeapon>(out RangedWeapon rangedWeapon))
                {
                    if (rangedWeapon.rangedWeaponData.infiniteAmmo)
                    {
                        rangedWeapon.ClearText();
                    }
                    else
                    {
                        rangedWeapon.UpdateAmmo();
                    }
                }

                selectedWeaponIndex = invWeapons[currWeaponIndex].GetComponent<Weapon>().inventoryPosition;
                selectedBorder.transform.localPosition = invUISlots[selectedWeaponIndex].transform.localPosition;
            }
        }
    }

    private void InputSwapWeapon()
    {
        if (Input.GetKeyDown(KeyCode.F) && invWeapons.Count == maxInvSlots)
        {
            Vector3 direction = cam.forward;

            GameObject hitObj = GetLOSObject(direction, false, targetableLayer);

            if (hitObj != null && !swapWeapon)
            {
                swapWeapon = true;

                // NOTE - This is used frequently. Maybe make the Player an
                // observer of PlayerInventory instead of directly calling it
                // from there.
                GameObject.FindWithTag("Player").GetComponent<PlayerWeaponDrop>().DropWeapon(invWeapons[currWeaponIndex]);
                GetComponent<PlayerWeaponPickup>().PickUpWeapon(hitObj);
                
                invWeapons[currWeaponIndex].SetActive(true);

                // NOTE - Remove <RangedWeapon>, also similar function above.
                if (invWeapons[currWeaponIndex].TryGetComponent<RangedWeapon>(out RangedWeapon rangedWeapon))
                {
                    if (rangedWeapon.rangedWeaponData.infiniteAmmo)
                    {
                        rangedWeapon.ClearText();
                    }
                    else
                    {
                        rangedWeapon.UpdateAmmo();
                    }
                }
                else
                {
                    GameObject.FindGameObjectWithTag("UpdateAmmoText").GetComponent<UpdateAmmoText>().ClearText();
                }

                selectedWeaponIndex = invWeapons[currWeaponIndex].GetComponent<Weapon>().inventoryPosition;
                selectedBorder.transform.localPosition = invUISlots[selectedWeaponIndex].transform.localPosition;
            }
        }
    }

    private GameObject GetLOSObject(Vector3 dir, bool ignoreLayers, LayerMask mask)
    {
        // NOTE - Combine into one whole ternary operator that uses a ternary
        // operator for the mask as well.
        if (!ignoreLayers)
        {
            if (Physics.Raycast(cam.position, dir, out RaycastHit hit, 1000, mask))
            {
                return hit.collider.gameObject;
            }
        }
        else
        {
            if (Physics.Raycast(cam.position, dir, out RaycastHit hit, 1000, ~mask))
            {
                return hit.collider.gameObject;
            }
        }

        return null;
    }

    public static bool CheckWeapon(GameObject weapon)
    {
        // NOTE - Unnest this
        if (invWeapons.Count < maxInvSlots)
        {
            if (swapWeapon)
            {
                invWeapons.Insert(currWeaponIndex, weapon);
                weapon.GetComponent<Weapon>().inventoryPosition = currWeaponIndex;

                // NOTE - Read note above. tl;dr: Store a list of iamges instead maybe
                invUISlots[currWeaponIndex].GetComponent<Image>().sprite = invWeaponIcons[(int)weapon.GetComponent<Weapon>().type];
                selectBorder.transform.localPosition = invUISlots[currWeaponIndex].transform.localPosition;

                // NOTE - Remove <RangedWeapon> Also similar function above,
                // consider making a method for this
                if (invWeapons[currWeaponIndex].TryGetComponent<RangedWeapon>(out RangedWeapon rangedWeapon))
                {
                    if (rangedWeapon.rangedWeaponData.infiniteAmmo)
                    {
                        rangedWeapon.ClearText();
                    }
                    else
                    {
                        rangedWeapon.UpdateAmmo();
                    }
                }

                // NOTE - Remove debug log
                Debug.Log("Swap Weapon : " + weapon.name);
                swapWeapon = false;
            }
            else
            {
                invWeapons.Add(weapon);
                
                for (int i = 0; i < invWeapons.Count; i++)
                {
                    // NOTE - Read above
                    if (invUISlots[i].GetComponent<Image>().sprite == null)
                    {
                        weapon.GetComponent<Weapon>().inventoryPosition = i;

                        invUISlots[i].GetComponent<Image>().sprite = invWeaponIcons[(int)weapon.GetComponent<Weapon>().type];

                        break;
                    }
                }
                Debug.Log("Added Weapon : " + weapon.name);
            }

            if (invWeapons.Count == 1)
            {
                currWeaponIndex = 0;
                weapon.SetActive(true);
                selectBorder.transform.localPosition = invUISlots[currWeaponIndex].transform.localPosition;


                // NOTE - Remove <RangedWeapon>
                if (invWeapons[currWeaponIndex].TryGetComponent<RangedWeapon>(out RangedWeapon rangedWeapon))
                {
                    if (rangedWeapon.rangedWeaponData.infiniteAmmo)
                    {
                        rangedWeapon.ClearText();
                    }
                    else
                    {
                        // NOTE - This looks insane. Read top of file for thoughts.
                        GameObject.FindGameObjectWithTag("UpdateAmmoText").GetComponent<UpdateAmmoText>().UpdateAmmo(rangedWeapon.rangedWeaponData.currentAmmo, rangedWeapon.rangedWeaponData.magazineSize);
                        // NOTE - ok
                        Debug.Log("ok");
                    }
                }
            }
            else
            {
                weapon.SetActive(false);
            }

            return true;
        }
        
        return false;
    }

    private void SetUI()
    {
        // NOTE - Seems to me like this can be a foreach loop
        for (int i = 0; i < invWeapons.Count; i++)
        {
            // NOTE - Cache the retrieved component
            int pos = invWeapons[i].GetComponent<Weapon>().inventoryPosition;

            // NOTE - Read above. Use a list of images
            invUISlots[pos].GetComponent<Image>().sprite = invWeaponIcons[(int)invWeapons[i].GetComponent<Weapon>().type];
            invWeapons[i].GetComponent<Weapon>().StartFunctionality();
        }

        if (invWeapons.Count >= 1)
        {
            selectBorder.transform.localPosition = invUISlots[currWeaponIndex].transform.localPosition;
            invWeapons[currWeaponIndex].SetActive(true);
        }
    }

    public static void DropWeapon(GameObject selected)
    {
        invWeapons.Remove(selected);
    }
}
