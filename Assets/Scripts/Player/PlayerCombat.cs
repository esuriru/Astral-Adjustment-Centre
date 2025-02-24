using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCombat : MonoBehaviour, IDamageable
{
    [SerializeField] public PlayerData playerData;
    private bool isDead = false;

    void Start()
    {
        playerData.ResetValues();
    }

    public void Damage(float damage)
    {
        if (isDead)
        return;

        playerData.currentHealth -= damage;

        if (playerData.currentHealth <= 0)
        {
            playerData.currentHealth = 0;
            Despawn();
        }
    }

    public void Despawn()
    {
        isDead = true;

        GameObject.FindWithTag("CameraHolder").GetComponent<MoveCamera>().enabled = false;
        TimelineManager.Instance.StartCoroutine(TimelineManager.Instance.PlayCutscene("Lose", "MenuScene"));
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
