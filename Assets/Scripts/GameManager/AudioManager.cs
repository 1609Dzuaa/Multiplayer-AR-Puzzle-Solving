using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : BaseSingleton<AudioManager>
{
    [Header("--------------------Audio Source-------------------")]
    [SerializeField] AudioSource _musicSource;
    [SerializeField] AudioSource _sfxSource;

    [Header("--------------------Audio Clip-------------------")]
    public AudioClip mainBGM;
    public AudioClip btnClick1;
    public AudioClip btnClick2;

    private void Start()
    {
        PlayBackGroundMusic();
    }

    private void PlayBackGroundMusic()
    {
        _musicSource.clip = mainBGM;
        _musicSource.loop = true;
        _musicSource.Play();
    }
}
