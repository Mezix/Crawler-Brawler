using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class MainMenuUI : NetworkBehaviour
{
    public Button _playGameButton;
    public Button _settingsButton;
    public Button _quitGameButton;

    [Header("Lobby Host Join Screen")]
    public bool _lobbyJoinHostOn;
    public InputField _playerNameInputField;
    public GameObject _lobbyJoinHostParent;
    public Button _hostLobby;
    public Button _joinLobby;

    [Header("Join Lobby Screen")]

    public bool _joinLobbyOn;
    public GameObject _joinLobbyParent;
    public InputField _codeInputField;
    public Button _attemptJoinLobby;

    [Header("Lobby Screen")]
    public bool _lobbyOn;
    public Text _lobbyCode;
    public GameObject _lobbyParent;
    public Button _launchGameButton;
    public VerticalLayoutGroup _vLayoutGroup;
    public List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();

    [Header("Character Selection")]
    public List<Sprite> _characterSprites;
    public Text _charName;
    public static NetworkVariable<int> CharIndex;
    public Image _characterImage;
    public Button _nextCharacterButton;
    public Button _previousCharacterButton;

    [Header("MISC")]
    public RelayManager _relayManager;

    private void Awake()
    {
        REF.CurrentScene = Loader.Scene.MenuScene;
        REF.HostMode = 0;
        REF.CharIndex = 0;
    }
    void Start()
    {
        InitPlayerName();
        TrySaveName();
        InitLobbyPlayers();
        ShowLobbyCreationScreen(false);
        ShowLobby(false);
        ShowJoinLobby(false);
        InitButtons();
        _attemptJoinLobby.interactable = false;
        SelectCharacter();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            EscapeBehaviour();
        }
        CheckLaunchPossible();
    }
    private void InitPlayerName()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            _playerNameInputField.text = PlayerPrefs.GetString("PlayerName");
        }
    }
    private void InitButtons()
    {
        _playGameButton.onClick.AddListener(() => ShowLobbyCreationScreen(true));
        _settingsButton.onClick.AddListener(() => SettingsOn());
        _quitGameButton.onClick.AddListener(() => QuitGame());

        _hostLobby.onClick.AddListener(() => HostLobby());
        _joinLobby.onClick.AddListener(() => ShowJoinLobby(true));
        _playerNameInputField.onValueChanged.AddListener(delegate { TrySaveName(); });
        _codeInputField.onValueChanged.AddListener(delegate { InputCodeChanged(); });
        _attemptJoinLobby.onClick.AddListener(() => TryJoinLobbyWithCode());

        _nextCharacterButton.onClick.AddListener(() => NextCharacter());
        _previousCharacterButton.onClick.AddListener(() => PreviousCharacter());

        _launchGameButton.onClick.AddListener(() => LaunchGame());
    }

    private void InputCodeChanged()
    {
        _codeInputField.SetTextWithoutNotify(_codeInputField.text.ToUpper());
        if (_codeInputField.text.Length == 6) _attemptJoinLobby.interactable = true;
        else _attemptJoinLobby.interactable = false;
    }

    private void EscapeBehaviour()
    {
        if (_lobbyOn)
        {
            LeaveLobby();
            return;
        }
        if (_joinLobbyOn)
        {
            ShowJoinLobby(false);
            return;
        }
        if (_lobbyJoinHostOn)
        {
            ShowLobbyCreationScreen(false);
            return;
        }
    }

    private void LeaveLobby()
    {
        _relayManager.StopGame();
        ShowLobby(false);
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

    //  Lobby Creation Screen

    private void TrySaveName()
    {
        if(_playerNameInputField.text.Length > 4)
        {
            PlayerPrefs.SetString("PlayerName", _playerNameInputField.text);
            _hostLobby.interactable = true;
            _joinLobby.interactable = true;
        }
        else
        {
            _hostLobby.interactable = false;
            _joinLobby.interactable = false;
        }
    }
    public void ShowLobbyCreationScreen(bool show)
    {
        _lobbyJoinHostParent.SetActive(show);
        _lobbyJoinHostOn = show;
    }
    public void ShowJoinLobby(bool show)
    {
        _joinLobbyParent.SetActive(show);
        _joinLobbyOn = show;
    }
    private void HostLobby()
    {
        REF.HostMode = 0;
        ClearLobby();
        //lobbyPlayers[0].AddPlayer(_playerNameInputField.text, "100ms");
        lobbyPlayers[0].LobbyPlayerChangedServerRPC(false, _playerNameInputField.text, "100ms", true);
        _relayManager.CreateGame();
        ShowLobby(true);
    }
    private void TryJoinLobbyWithCode()
    {
        REF.HostMode = 1;
        _relayManager.JoinGame(_codeInputField.text);

        //  Check availabilty
        int joinSlot = -1;
        for(int i = 0; i < lobbyPlayers.Count; i++)
        {
            if(!lobbyPlayers[i]._occupied)
            {
                joinSlot = i;
                break;
            }
        }
        if(joinSlot >= 0)
        {
            ShowLobby(true);
            //lobbyPlayers[joinSlot].AddPlayer(_playerNameInputField.text, "100ms");
            lobbyPlayers[joinSlot].LobbyPlayerChangedServerRPC(false, _playerNameInputField.text, "100ms", true);
        }
    }

    //  Lobby Screen

    private void ClearLobby()
    {
        foreach(LobbyPlayer p in lobbyPlayers)
        {
            p.Reset();
        }
    }
    private void InitLobbyPlayers()
    {
        for (int i = 0; i < 4; i++)
        {
            lobbyPlayers[i].Reset();
            // LobbyPlayer player = Instantiate(Resources.Load("Prefabs/LobbyPlayer", typeof(LobbyPlayer)) as LobbyPlayer);
            // GameObject go = Instantiate((GameObject) Resources.Load("Prefabs/LobbyPlayer"));
            //LobbyPlayer p = go.GetComponentInChildren<LobbyPlayer>();
            //p.transform.SetParent(_vLayoutGroup.transform, false);
            //lobbyPlayers.Add(p);
        }
    }
    public override void OnNetworkSpawn()
    {

    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
    public void ShowLobby(bool show)
    {
        _lobbyParent.SetActive(show);
        _lobbyOn = show;
    }

    private void QuitGame()
    {
        Application.Quit();
    }
    private void CheckLaunchPossible()
    {
        //  if we are the client, dont allow us to launch the game
        if (NetworkManager.Singleton.IsClient) { _launchGameButton.interactable = false; return; }

        int OccupyCount = 0;
        int ReadyCount = 0;
        foreach(LobbyPlayer p in lobbyPlayers)
        {
            if(p._occupied)
            {
                OccupyCount++;
                if (p._toggle.isOn) ReadyCount++;
            }
        }
        _launchGameButton.interactable = (OccupyCount == ReadyCount);
    }
    private void LaunchGame()
    {
        Loader.Load(Loader.Scene.GameScene);
    }

    //  Settings
    private void SettingsOn()
    {

    }
}
