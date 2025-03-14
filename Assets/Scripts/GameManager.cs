using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public bool isRunning = false;

    [SerializeField] private GameObject[] _managers;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
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
        }
    }

    void _InitiateGame()
    {
        isRunning = true;
    }
}
