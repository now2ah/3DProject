using System;
using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameObject playerStartPointObject;
    
    public bool isRunning = false;

    [SerializeField] private GameObject[] _managers;

    //fix later
    [SerializeField] private GameObject playerPrefab;

    private GameObject _playerObj;
    private PlayerManager _player;
    private Coroutine _spawnPlayerCoroutine;

    public PlayerManager Player { get { return _player; } set { _player = value; } }

    public event EventHandler OnPlayerSpawned;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        _LoadManagers();
        _SpawnPlayer();
        
    }

    void _InitiateGame()
    {
        isRunning = true;
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
        if (FindAnyObjectByType<PlayerManager>() == null)
        {
            if (null != playerPrefab && null != playerStartPointObject)
            {
                _playerObj = Instantiate(playerPrefab, playerStartPointObject.transform.position, Quaternion.identity);
                _player = _playerObj.GetComponent<PlayerManager>();
                OnPlayerSpawned.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
