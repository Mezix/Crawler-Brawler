using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        //SpawnCharacter();
    }

    private void SpawnCharacter()
    {
        if(REF.HostMode == 0) NetworkManager.Singleton.StartHost();
        else if (REF.HostMode == 1) NetworkManager.Singleton.StartServer();
        else if (REF.HostMode == 2) NetworkManager.Singleton.StartClient();
    }
}
