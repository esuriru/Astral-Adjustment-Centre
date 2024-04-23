// NOTE - Remove superfluous usings
using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using EntityKey = PlayFab.GroupsModels.EntityKey;
using UnityEngine;
using PlayFab.GroupsModels;

// NOTE - This class does nothing but hold values, maybe call it
// PlayFabGlobalData instead?
public class PlayFabManager : MonoBehaviour
{
    // NOTE - Make this private
    public static PlayFabManager instance = null;

    // NOTE - What's the point of having an instance, if everything is static
    // anyway?

    public static string currPlayFabID;
    public static string currTitleID;
    public static string currPlayFabDN;
    public static EntityKey currGuildID;
    public static string currGuildName;
    public static string currGuildRole;

    public static bool isNewPlayer;
    public static int runsCompleted;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(base.gameObject); 
        }
        else
        {
            Destroy(base.gameObject);
        }
    }
}
