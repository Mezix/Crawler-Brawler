using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    public bool gameStarted = false;

    private void Start()
    {
        InitNetworkPrefabs();
    }

    private void InitNetworkPrefabs()
    {
        Singleton.AddNetworkPrefab((GameObject) Resources.Load(GS.Prefabs("PhysicsObject")));
    }

    private void OnGUI()
    {
        if (gameStarted) return;

        if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 2 + 50, 200, 50), "Host"))
        {
            Singleton.StartHost();
            gameStarted = true;
        }
        if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 2, 200, 50), "Client"))
        {
            Singleton.StartClient();
            gameStarted = true;
        }
        //if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 2 - 50, 200, 50), "Server"))
        //{
        //    Singleton.StartServer();
        //    gameStarted = true;
        //}
    }
    
}
