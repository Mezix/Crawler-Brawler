using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Button _launchGameButton;
    public Button _quitGameButton;

    [Header ("Hosting")]
    public Button _nextHostMode;
    public Button _previousHostMode;
    public Text _hostModeText;

    [Header("Character Selection")]
    public List<Sprite> _characterSprites;
    public Text _charName;
    public Image _characterImage;
    public Button _nextCharacterButton;
    public Button _previousCharacterButton;
    private void Awake()
    {
        REF.HostMode = 0;
        SelectHostMode();
        REF.CharIndex = 0;
        SelectCharacter();
    }
    void Start()
    {
        InitButtons();
    }


    void Update()
    {

    }
    private void InitButtons()
    {
        _launchGameButton.onClick.AddListener(() => LaunchGame());
        _quitGameButton.onClick.AddListener(() => QuitGame());

        _nextHostMode.onClick.AddListener(() => NextHostMode());
        _previousHostMode.onClick.AddListener(() => PreviousHostMode());

        _nextCharacterButton.onClick.AddListener(() => NextCharacter());
        _previousCharacterButton.onClick.AddListener(() => PreviousCharacter());
    }


    private void LaunchGame()
    {
        Loader.Load(Loader.Scene.GameScene);
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    //  Character
    private void PreviousCharacter()
    {
        REF.CharIndex--;
        if (REF.CharIndex < 0) REF.CharIndex = _characterSprites.Count-1;
        SelectCharacter();
    }

    private void NextCharacter()
    {
        REF.CharIndex++;
        if (REF.CharIndex > _characterSprites.Count - 1) REF.CharIndex = 0;
        SelectCharacter();
    }

    private void SelectCharacter()
    {
        _characterImage.sprite = _characterSprites[REF.CharIndex];
        if (REF.CharIndex == 0) _charName.text = "ELIZA";
        else if (REF.CharIndex == 1) _charName.text = "ROBINETTE";
        else if (REF.CharIndex == 2) _charName.text = "JOE";
        else if (REF.CharIndex == 3) _charName.text = "MICHELANGELO";
        else if (REF.CharIndex == 4) _charName.text = "GENNEFER";
        else if (REF.CharIndex == 5) _charName.text = "ROLBUNCTOR";
        else _charName.text = "!!MISSING NO!!";
    }

    //  Host Mode

    private void PreviousHostMode()
    {
        REF.HostMode--;
        if (REF.HostMode < 0) REF.HostMode = 2;
        SelectHostMode();
    }
    private void NextHostMode()
    {
        REF.HostMode++;
        if (REF.HostMode > 2) REF.HostMode = 0;
        SelectHostMode();
    }
    private void SelectHostMode()
    {
        if (REF.HostMode == 0) _hostModeText.text = "HOST";
        else if (REF.HostMode == 1) _hostModeText.text = "SERVER";
        else if (REF.HostMode == 2) _hostModeText.text = "CLIENT";
        else _hostModeText.text = "!!ERROR!!";
    }
}