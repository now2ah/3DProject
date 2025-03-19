using System;
using TMPro;
using UnityEngine;

public enum EUIObject
{
    CROSSHAIR,
    RIFLE,
    BULLET
}

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject _crossHairUI;
    [SerializeField] private GameObject _rifleUI;
    [SerializeField] private GameObject _bulletUI;
    [SerializeField] private TextMeshProUGUI _bulletText;

    public TextMeshProUGUI BulletText { get { return _bulletText; } set { _bulletText = value; } }

    private void Awake()
    {
        //Transform canvasTransform = gameObject.transform.GetChild(0);
        //_crossHairUI = canvasTransform.GetChild(0).gameObject;
        //_rifleUI = canvasTransform.GetChild(1).gameObject;
        //_bulletUI = canvasTransform.GetChild(2).gameObject;
        //_bulletText = _bulletUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerSpawned += _OnPlayerSpawned;
    }

    public void SetActiveUI(EUIObject ui, bool isOn)
    {
        if (ui == EUIObject.CROSSHAIR)
        {
            _crossHairUI.SetActive(isOn);
        }
        else if (ui == EUIObject.RIFLE)
        {
            _rifleUI.SetActive(isOn);
        }
        else if (ui == EUIObject.BULLET)
        {
            _bulletUI.SetActive(isOn);
        }
    }

    void _OnPlayerSpawned(object sender, EventArgs e)
    {
        GameManager.Instance.Player.OnPlayerStatsChange += _OnValueChange;
    }

    void _OnValueChange(object sender, PlayerStatsForUI stats)
    {
        _UpdateUIs(stats);
    }

    void _UpdateUIs(PlayerStatsForUI stats)
    {
        if (stats.hasRifle)
        {
            SetActiveUI(EUIObject.RIFLE, stats.hasRifle);
        }

        if (stats.isAiming)
        {
            SetActiveUI(EUIObject.CROSSHAIR, stats.isAiming);
        }


    }
}
