// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager instance;

    // NOTE - Missing access specifier, prefer: private
    [SerializeField] TMP_Text tooltipText;

    // NOTE - Missing access specifier, prefer: private
    void Awake()
    {
        if (instance != null && instance != this)
        {
            // NOTE - Remove `this`
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    // NOTE - Missing access specifier, prefer: private
    void Start()
    {
        gameObject.SetActive(false);
    }

    // NOTE - Missing access specifier, prefer: private
    void Update()
    {
        transform.position = Input.mousePosition;
    }

    public void SetAndShowToolTip(string text)
    {
        gameObject.SetActive(true);
        tooltipText.text = text;
    }

    public void HideToolTip()
    {
        gameObject.SetActive(false);
        tooltipText.text = string.Empty;
    }
}
