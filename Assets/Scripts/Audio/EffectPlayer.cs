using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPlayer : MonoBehaviour
{
    
    private AudioSource[] _audioSources;
    
    public static EffectPlayer instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        _audioSources = transform.GetComponentsInChildren<AudioSource>();

        for (int i = 0; i < _audioSources.Length; i++)
            _audioSources[i].volume = PlayerPrefs.GetFloat("sound_configuration", 1);
    }

    public void PlayOneShot(AudioClip song)
    {
        if (song == null)       
            return;

        for (int i = 0; i < _audioSources.Length; i++)
        {
            if (!_audioSources[i].isPlaying)
            {
                _audioSources[i].pitch = 1;
                _audioSources[i].volume = PlayerPrefs.GetFloat("sound_configuration", 1);
                _audioSources[i].PlayOneShot(song);
                return;
            }
        }
        _audioSources[0].PlayOneShot(song);
    }
    
}
