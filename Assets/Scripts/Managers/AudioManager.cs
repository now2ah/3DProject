using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public enum eBgm
    {

    }

    public enum ESfx
    {
        BEHIT,
        PICKUP,
        EQUIP,
        FIRE,
    }

    public AudioClip[] audioClips;
    public int sfxChannel = 5;

    private AudioSource _bgmAudioSource;
    private AudioSource[] _sfxAudioSources;

    [SerializeField] private Dictionary<string, AudioClip> _audioDic;

    private void Awake()
    {
        _bgmAudioSource = gameObject.AddComponent<AudioSource>();
        _bgmAudioSource.playOnAwake = false;
        _bgmAudioSource.loop = false;

        _sfxAudioSources = new AudioSource[sfxChannel];
        
        for (int i=0; i<sfxChannel; i++)
        {
            _sfxAudioSources[i] = gameObject.AddComponent<AudioSource>();
            _sfxAudioSources[i].playOnAwake = false;
            _sfxAudioSources[i].loop = false;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _SetAudioDictionary();
    }

    public void PlayBgm(eBgm bgm)
    {
        if (null == _bgmAudioSource)
            return;

        _bgmAudioSource.clip = _audioDic[Enum.GetName(typeof(eBgm), bgm)];

        if (null == _bgmAudioSource.clip) { Debug.Log("can't find bgm audio clip"); return; }

        _bgmAudioSource.Play();
    }

    public void PlaySfx(ESfx sfx)
    {
        AudioSource playAudioSource = null;
        foreach (var audioSource in _sfxAudioSources)
        {
            if (!audioSource.isPlaying)
                playAudioSource = audioSource;
        }

        if (null == playAudioSource)
            return;
        
        playAudioSource.clip = _audioDic[Enum.GetName(typeof(ESfx), sfx)];

        if (null == playAudioSource.clip) { Debug.Log("can't find sfx audio clip"); return; }

        playAudioSource.PlayOneShot(playAudioSource.clip);
    }

    void _SetAudioDictionary()
    {
        if (null == _audioDic && audioClips.Length > 0)
        {
            _audioDic = new Dictionary<string, AudioClip>();

            foreach (var clip in audioClips)
            {
                _audioDic[clip.name] = clip;
            }
        }
    }
}
