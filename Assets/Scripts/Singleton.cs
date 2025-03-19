using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance = null;

    public static T Instance
    {
        get
        {
            if (null == _instance)
            {
                _instance = FindAnyObjectByType<T>();

                if (null == _instance)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    private void OnEnable()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    private void Awake()
    {
        //DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
    }
}
