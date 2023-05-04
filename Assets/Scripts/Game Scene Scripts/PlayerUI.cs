using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public GameObject _menuParent;
    public bool _menuOn;

    [Header("Damage Indicators")]
    public Button _returnToMainMenuButton;
    public Button _settingsButton;
    public Button _quitGameButton;

    [Header("Settings")]
    public bool _settingsOn;
    public GameObject _settingsParent;

    private void Awake()
    {
        InitButtons();
    }

    private void InitButtons()
    {
        _returnToMainMenuButton.onClick.AddListener(() => ReturnToMainMenu());
        _quitGameButton.onClick.AddListener(() => QuitGame());
    }

    private void Start()
    {
        _menuOn = false;
        MenuOn(_menuOn);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            _menuOn = !_menuOn;
            MenuOn(_menuOn);
        }
    }
    public void MenuOn(bool on)
    {
        _menuParent.SetActive(on);
    }

    private void ReturnToMainMenu()
    {
        Loader.Load(Loader.Scene.MenuScene);
    }
    private void QuitGame()
    {
        Application.Quit();
    }
}
