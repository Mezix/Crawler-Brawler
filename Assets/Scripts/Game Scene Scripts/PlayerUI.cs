using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Top Level Settings")]
    public GameObject _menuParent;
    public bool _menuOn;

    public Button _returnToMainMenuButton;
    public Button _openSettingsButton;
    public Button _quitGameButton;

    [Header("Settings")]
    public GameObject _settingsParent;
    public bool _settingsOn;

    public Button _keyboardModeButton;
    public Button _controllerModeButton;
    public Button _closeSettingsButton;

    private void Awake()
    {
        InitButtons();
    }

    private void InitButtons()
    {
        //  Top Level Menu
        _returnToMainMenuButton.onClick.AddListener(() => ReturnToMainMenu());
        _quitGameButton.onClick.AddListener(() => QuitGame());
        _openSettingsButton.onClick.AddListener(() => TurnOnSettings(true));

        //  Settings Menu
        _closeSettingsButton.onClick.AddListener(() => TurnOnSettings(false));
        _controllerModeButton.onClick.AddListener(() => ControllerMode(true));
        _keyboardModeButton.onClick.AddListener(() => ControllerMode(false));
    }

    private void Start()
    {
        _menuOn = false;
        TurnOnMenu(_menuOn);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            _menuOn = !_menuOn;
            TurnOnMenu(_menuOn);
        }
    }
    //  Top Level UI
    public void TurnOnMenu(bool on)
    {
        _menuParent.SetActive(on);
        if(!on)
        {
            TurnOnSettings(false);
        }
    }
    private void ReturnToMainMenu()
    {
        Loader.Load(Loader.Scene.MenuScene);
    }
    private void QuitGame()
    {
        Application.Quit();
    }

    //  Settings
    public void TurnOnSettings(bool on)
    {
        _settingsOn = on;
        _settingsParent.SetActive(on);
    }
    public void ControllerMode(bool controllerOn)
    {
        REF.PCon._controllerOn = controllerOn;
    }
}
