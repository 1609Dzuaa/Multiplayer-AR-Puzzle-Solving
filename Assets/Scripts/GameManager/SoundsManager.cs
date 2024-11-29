using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static GameEnums;

public class SoundsManager : BaseSingleton<SoundsManager>
{
    //Clip - tệp âm thanh
    //Source - cái để phát tệp đó cũng như điều chỉnh linh tinh

    [SerializeField]
    Sounds[] sfxSounds, musicSounds;

    [SerializeField] AudioSource _sfxSource, _musicSource;

    public AudioSource SFXSource => _sfxSource;

    public AudioSource MusicSource => _musicSource;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayBackGroundMusic();
    }

    private void PlayBackGroundMusic()
    {
        PlayMusic(ESoundName.BGM);
    }

    public void PlaySfx(ESoundName sfxName, float volumeScale = 1.0f)
    {
        //Tìm hiểu về Lambda expression
        Sounds s = Array.Find(sfxSounds, x => x.SoundName == sfxName);
        if (s == null)
            Debug.Log(sfxName + " Not Found");
        else
        {
            _sfxSource.clip = s.SoundAudioClip;
            if (volumeScale >= 1.0f) _sfxSource.PlayOneShot(_sfxSource.clip);
            else _sfxSource.PlayOneShot(_sfxSource.clip, volumeScale);
            //Debug.Log("Sfx Played: " + sfxName);
        }
    }

    public void PlayMusic(ESoundName musicName)
    {
        Sounds s = Array.Find(musicSounds, x => x.SoundName == musicName);
        if (s == null)
            Debug.Log(musicName + " Not Found");
        else
        {
            _musicSource.clip = s.SoundAudioClip;
            _musicSource.Play();
        }
    }

    public void ChangeSourceVolume(float para, bool isSFXSource = false)
    {
        if (!isSFXSource)
            _musicSource.volume = para;
        else
            _sfxSource.volume = para;
    }
}

[System.Serializable]
public class Sounds
{
    [SerializeField] ESoundName _soundName;

    //Nhận vào audioClip, đỡ phải tạo AS
    [SerializeField] AudioClip _soundAudioClip;

    public ESoundName SoundName { get { return _soundName; } }

    public AudioClip SoundAudioClip { get { return _soundAudioClip; } }
}