using System;
using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameObject playerStartPointObject;

    [SerializeField] private GameObject[] _managers;
    [SerializeField] private bool isRunning = false;
    //fix later
    [SerializeField] private GameObject playerPrefab;

    private GameObject _playerObj;
    private Player _player;
    private Coroutine _spawnPlayerCoroutine;

    public Player Player { get { return _player; } set { _player = value; } }

    public event EventHandler OnPlayerSpawned;
    public event EventHandler OnGameOver;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        _LoadManagers();
        _InitiateGame();
    }

    public void GameOver()
    {
        isRunning = false;
        OnGameOver?.Invoke(this, EventArgs.Empty);
    }

    void _InitiateGame()
    {
        if (!isRunning)
        {
            isRunning = true;
            _SpawnPlayer();
        }
    }

    void _LoadManagers()
    {
        if (_managers.Length > 0)
        {
            foreach(var obj in _managers)
            {
                Instantiate(obj);
            }
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
            }
        }
    }
}
