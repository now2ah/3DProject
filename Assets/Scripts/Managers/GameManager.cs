using System;
using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameObject playerStartPointObject;

    [SerializeField] private GameObject[] _managers;
    [SerializeField] private bool isRunning = false;
    [SerializeField] private GameObject playerPrefab;

    public bool IsRunning => isRunning;

    private GameObject _playerObj;
    private Player _player;
    private Coroutine _spawnPlayerCoroutine;

    public Player Player { get { return _player; } set { _player = value; } }

    public event EventHandler OnManagersLoaded;
    public event EventHandler OnPlayerSpawned;
    public event EventHandler OnGameStart;
    public event EventHandler OnGameOver;
    public event EventHandler OnGameSucceeded;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        OnManagersLoaded += _OnManagersLoaded;
        StartCoroutine(LoadManagerCoroutine());
    }

    private void _OnManagersLoaded(object sender, EventArgs e)
    {
        AudioManager.Instance.PlayBgm(AudioManager.eBgm.BGM_MAIN);
    }

    private void OnDisable()
    {
        _UnsubscribeEvents();
    }


    public void GameStart()
    {
        _InitiateGame();
        OnGameStart.Invoke(this, EventArgs.Empty);
        AudioManager.Instance.PlayBgm(AudioManager.eBgm.BGM_AMBIENCE);
    }

    public void GameOver()
    {
        isRunning = false;
        OnGameOver?.Invoke(this, EventArgs.Empty);
        AudioManager.Instance.PlaySfx(AudioManager.ESfx.GAMEOVER);
    }

    public void GameSucceeded()
    {
        isRunning = false;
        OnGameSucceeded?.Invoke(this, EventArgs.Empty);
        AudioManager.Instance.PlaySfx(AudioManager.ESfx.SUCCESS);
    }

    void _UnsubscribeEvents()
    {
        if (OnManagersLoaded != null)
        {
            foreach (var d in OnManagersLoaded.GetInvocationList())
            {
                OnManagersLoaded -= d as EventHandler;
            }
        }

        if (OnPlayerSpawned != null)
        {
            foreach (var d in OnPlayerSpawned.GetInvocationList())
            {
                OnPlayerSpawned -= d as EventHandler;
            }
        }

        if (OnGameStart != null)
        {
            foreach (var d in OnGameStart.GetInvocationList())
            {
                OnGameStart -= d as EventHandler;
            }
        }

        if (OnGameOver != null)
        {
            foreach (var d in OnGameOver.GetInvocationList())
            {
                OnGameOver -= d as EventHandler;
            }
        }

        if (OnGameSucceeded != null)
        {
            foreach (var d in OnGameSucceeded.GetInvocationList())
            {
                OnGameSucceeded -= d as EventHandler;
            }
        }
    }

    void _InitiateGame()
    {
        if (!isRunning)
        {
            isRunning = true;
            _FindPlayerStartObject();
            _SpawnPlayer();
        }
    }

    IEnumerator LoadManagerCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        _LoadManagers();
    }

    void _LoadManagers()
    {
        if (_managers.Length > 0)
        {
            foreach(var obj in _managers)
            {
                Instantiate(obj);
            }
            OnManagersLoaded?.Invoke(this, EventArgs.Empty);
        }
    }

    void _FindPlayerStartObject()
    {
        if (null == playerStartPointObject)
        {
            playerStartPointObject = GameObject.FindGameObjectWithTag("StartPosition");
        }
    }

    void _SpawnPlayer()
    {
        if (null == _spawnPlayerCoroutine)
            _spawnPlayerCoroutine = StartCoroutine(SpawnPlayerCoroutine());
    }

    IEnumerator SpawnPlayerCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        if (FindAnyObjectByType<Player>() == null)
        {
            if (null != playerPrefab && null != playerStartPointObject)
            {
                _playerObj = Instantiate(playerPrefab, playerStartPointObject.transform.position, Quaternion.identity);
                _player = _playerObj.GetComponent<Player>();
                OnPlayerSpawned.Invoke(this, EventArgs.Empty);
                _spawnPlayerCoroutine = null;
            }
        }
    }
}
