using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        _occupied = true;
    }
    private void PlayerIsReady(bool on)
    {
        Debug.Log(_name.text + " : Ready : " +  on);
    }
}
