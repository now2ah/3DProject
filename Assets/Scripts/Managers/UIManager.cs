using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum EUIObject
{
    CROSSHAIR,
    RIFLE,
    BULLET
}

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _toMainText;
    [SerializeField] private GameObject _gameSuccessPanel;
    [SerializeField] private TextMeshProUGUI _toMainSuccessText;
    [SerializeField] private GameObject _startTutorial;
    [SerializeField] private GameObject _rifleTutorial;

    [SerializeField] private Slider _hpSlider;
    [SerializeField] private TextMeshProUGUI _hpText;
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
        GameManager.Instance.OnGameStart += OnGameStart;
        GameManager.Instance.OnGameOver += _OnGameOver;
        GameManager.Instance.OnGameSucceeded += _OnGameSucceeded;
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

    public void LoadMainScene()
    {
        SceneManager.LoadScene(0);
    }

    private void OnGameStart(object sender, EventArgs e)
    {
        if (_hpSlider != null)
        {
            _hpSlider.gameObject.SetActive(true);
        }
    }

    void _OnPlayerSpawned(object sender, EventArgs e)
    {
        GameManager.Instance.Player.OnPlayerStatsChange += _OnValueChange;
        GameManager.Instance.Player.OnStartTutorial += _OnStartTutorial;
        GameManager.Instance.Player.OnPickUpLight += _OnPickUpLight;
        GameManager.Instance.Player.OnPickUpRifle += _OnPickUpRifle;
    }

    void _OnValueChange(object sender, PlayerStatsForUI stats)
    {
        _UpdateUIs(stats);
    }

    private void _OnStartTutorial(object sender, EventArgs e)
    {
        if (_startTutorial != null)
        {
            _startTutorial.SetActive(true);
        }
    }

    private void _OnPickUpLight(object sender, EventArgs e)
    {
        if (_startTutorial != null)
        {
            _startTutorial.SetActive(false);
        }
    }
    private void _OnPickUpRifle(object sender, EventArgs e)
    {
        if (_rifleTutorial != null)
        {
            StartCoroutine(RifleTutorialCoroutine());
        }
    }

    void _UpdateUIs(PlayerStatsForUI stats)
    {
        if (_hpSlider != null && _hpText != null)
        {
            float hpRatio = stats.curHP / stats.maxHP;

            _hpSlider.value = hpRatio;

            string hpText = (hpRatio * 100f).ToString("N0") + " %";

            _hpText.text = hpText;
        }

        if (stats.hasRifle)
        {
            SetActiveUI(EUIObject.RIFLE, stats.hasRifle);
            SetActiveUI(EUIObject.BULLET, true);
            _bulletText.text = "Bullet : " + stats.bulletCount;
        }

        if (stats.isAiming)
        {
            SetActiveUI(EUIObject.CROSSHAIR, stats.isAiming);
        }
    }

    void _OnGameOver(object sender, EventArgs e)
    {
        if (null != _gameOverPanel)
        {
            _gameOverPanel.SetActive(true);
        }

        if (null != _hpSlider)
        {
            _hpSlider.gameObject.SetActive(false);
        }

        if (null != _toMainText)
        {
            StartCoroutine(ToMainCountCoroutine(_gameOverPanel, _toMainText));
        }
    }

    void _OnGameSucceeded(object sender, EventArgs e)
    {
        if (null != _gameSuccessPanel)
        {
            _gameSuccessPanel.SetActive(true);
        }

        if (null != _toMainText)
        {
            StartCoroutine(ToMainCountCoroutine(_gameSuccessPanel, _toMainSuccessText));
        }
    }

    IEnumerator ToMainCountCoroutine(GameObject panel, TextMeshProUGUI text)
    {
        int count = 3;
        while(count-- > 0)
        {
            text.text = "To Main " + count;
            yield return new WaitForSeconds(1);
        }
        panel.SetActive(false);
        Destroy(GameManager.Instance.Player.gameObject);
        SceneManager.LoadScene(0);
    }

    IEnumerator RifleTutorialCoroutine()
    {
        if (_rifleTutorial != null)
        {
            _rifleTutorial.SetActive(true);
            yield return new WaitForSeconds(3.0f);
            _rifleTutorial.SetActive(false);
        }
    }
}
