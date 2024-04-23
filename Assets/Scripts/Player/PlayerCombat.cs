// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCombat : MonoBehaviour, IDamageable
{
    // NOTE - Make this private
    [SerializeField] public PlayerData playerData;
    private bool isDead = false;

    // NOTE - Missing access specifier
    void Start()
    {
        playerData.ResetValues();
    }

    public void Damage(float damage)
    {
        // NOTE - One line if missing braces 
        if (isDead)
        return;

        playerData.currentHealth -= damage;

        if (playerData.currentHealth <= 0)
        {
            // NOTE - line below seems redundant
            playerData.currentHealth = 0;
            Despawn();
        }
    }

    public void Despawn()
    {
        isDead = true;

        // NOTE - Cache this object
        GameObject.FindWithTag("CameraHolder").GetComponent<MoveCamera>().enabled = false;

        // NOTE - The coroutine should be called started internally in TimelineManager
        TimelineManager.Instance.StartCoroutine(TimelineManager.Instance.PlayCutscene("Lose", "MenuScene"));
        
        // NOTE - A CursorManager or something else should be handling this
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
