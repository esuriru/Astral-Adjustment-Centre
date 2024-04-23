// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup subtitlePanel;
    [SerializeField] private TMP_Text playerSubtitleText, enemySubtitleText;
    [SerializeField] private List<string> playerSubtitles, enemySubtitles;
    [SerializeField] private List<string> playerVoicelines, enemyVoicelines;

    // NOTE - Consider not shortening current
    private int playerCurrLine;
    private int enemyCurrLine;

    // NOTE - Missing access specifier
    void Awake()
    {
        subtitlePanel.gameObject.SetActive(false);
        subtitlePanel.alpha = 0;
        playerSubtitleText.text = string.Empty;
        enemySubtitleText.text = string.Empty;
        playerCurrLine = 0;
    }

    public void ResetText()
    {
        playerSubtitleText.text = string.Empty;
        enemySubtitleText.text = string.Empty;
    }

    public void PlayerNextLine()
    {
        // NOTE - Literally only line 47 needs to be in this if. 
        // Possible solution:
        //
        // if (playerSubtitles.Count >= playerCurrLine)
        // {
        //     playerCurrLine = 0;
        // }

        // playerSubtitleText.text = playerSubtitles[playerCurrLine];
        // AudioManager.Instance.PlayVL(playerVoicelines[playerCurrLine]);
        // playerCurrLine++;

        if (playerSubtitles.Count >= playerCurrLine)
        {
            playerSubtitleText.text = playerSubtitles[playerCurrLine];

            AudioManager.Instance.PlayVL(playerVoicelines[playerCurrLine]);

            playerCurrLine++;
        }
        else
        {
            playerCurrLine = 0;

            playerSubtitleText.text = playerSubtitles[playerCurrLine];

            AudioManager.Instance.PlayVL(playerVoicelines[playerCurrLine]);

            playerCurrLine++;
        }
    }

    public void EnemyNextLine()
    {
        // NOTE - Same as above. 
        if (enemySubtitles.Count >= enemyCurrLine)
        {
            enemySubtitleText.text = enemySubtitles[enemyCurrLine];

            AudioManager.Instance.PlayVL(enemyVoicelines[enemyCurrLine]);

            enemyCurrLine++;
        }
        else
        {
            enemyCurrLine = 0;

            enemySubtitleText.text = enemySubtitles[enemyCurrLine];

            AudioManager.Instance.PlayVL(enemyVoicelines[enemyCurrLine]);

            enemyCurrLine++;
        }
    }
}
