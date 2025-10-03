using UnityEngine;
using UnityEngine.Audio;

public class SettingsBootstrap : MonoBehaviour
{
    public AudioMixer mixer;
    const string P_Master = "MasterVolume", P_Music = "MusicVolume", P_Sfx = "SFXVolume";

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        float m = PlayerPrefs.GetFloat("opt_master", 0.8f);
        float mu = PlayerPrefs.GetFloat("opt_music", 0.8f);
        float fx = PlayerPrefs.GetFloat("opt_sfx", 0.8f);
        bool mt = PlayerPrefs.GetInt("opt_mute", 0) == 1;

        mixer.SetFloat(P_Master, mt ? -80f : Mathf.Log10(Mathf.Clamp(m, 0.0001f, 1)) * 20f);
        mixer.SetFloat(P_Music, Mathf.Log10(Mathf.Clamp(mu, 0.0001f, 1)) * 20f);
        mixer.SetFloat(P_Sfx, Mathf.Log10(Mathf.Clamp(fx, 0.0001f, 1)) * 20f);
    }
}
