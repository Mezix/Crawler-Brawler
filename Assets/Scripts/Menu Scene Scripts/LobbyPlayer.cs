using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyPlayer : MonoBehaviour
{
    public Toggle _toggle;
    public Text _name;
    public Text _ping;
    public bool _occupied;

    private void Awake()
    {
        _toggle.onValueChanged.AddListener(delegate { PlayerIsReady(_toggle.isOn); });
    }

    public void Reset()
    {
        _toggle.SetIsOnWithoutNotify(false);
        _name.text = "EMPTY";
        _ping.text = "----";
        _occupied = false;
    }

    public void AddPlayer(string name, string ping)
    {
        _toggle.SetIsOnWithoutNotify(false);
        _name.text = name;
        _ping.text = ping;
        _occupied = false;
    }
    private void PlayerIsReady(bool on)
    {
        Debug.Log(_name.text + " : Ready : " +  on);
    }

    [ServerRpc]
    public void LobbyPlayerChangedServerRPC(bool toggleState, string name, string ping, bool occupied)
    {
        LobbyPlayerChangedClientRPC(toggleState, name, ping, occupied);
    }

    [ClientRpc]
    private void LobbyPlayerChangedClientRPC(bool toggleState, string name, string ping, bool occupied)
    {
        _toggle.isOn = toggleState;
        _name.text = name;
        _ping.text = ping;
        _occupied = occupied;

        print(this.name);
    }
}
