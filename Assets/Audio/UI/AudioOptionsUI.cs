using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class AudioOptionsUI : MonoBehaviour
{
    public AudioMixer mixer;

    [Header("Sliders 0..1")]
    public Slider master;
    public Slider music;
    public Slider sfx;
    public Toggle muteToggle;

    public TMP_Text masterValueLabel; // optional

    const string K_Master = "opt_master", K_Music = "opt_music", K_Sfx = "opt_sfx", K_Mute = "opt_mute";
    const string P_Master = "MasterVolume", P_Music = "MusicVolume", P_Sfx = "SFXVolume";

    void OnEnable()
    {
        float m = PlayerPrefs.GetFloat(K_Master, 0.8f);
        float mu = PlayerPrefs.GetFloat(K_Music, 0.8f);
        float fx = PlayerPrefs.GetFloat(K_Sfx, 0.8f);
        bool mt = PlayerPrefs.GetInt(K_Mute, 0) == 1;

        master.SetValueWithoutNotify(m);
        music.SetValueWithoutNotify(mu);
        sfx.SetValueWithoutNotify(fx);
        muteToggle.SetIsOnWithoutNotify(mt);

        ApplyAll(m, mu, fx, mt, false);
        Hook(true);
    }
    void OnDisable() { Hook(false); }

    void Hook(bool on)
    {
        if (on)
        {
            master.onValueChanged.AddListener(v => ApplyAll(v, music.value, sfx.value, muteToggle.isOn, true));
            music.onValueChanged.AddListener(v => ApplyAll(master.value, v, sfx.value, muteToggle.isOn, true));
            sfx.onValueChanged.AddListener(v => ApplyAll(master.value, music.value, v, muteToggle.isOn, true));
            muteToggle.onValueChanged.AddListener(m => ApplyAll(master.value, music.value, sfx.value, m, true));
        }
        else
        {
            master.onValueChanged.RemoveAllListeners();
            music.onValueChanged.RemoveAllListeners();
            sfx.onValueChanged.RemoveAllListeners();
            muteToggle.onValueChanged.RemoveAllListeners();
        }
    }

    void ApplyAll(float m, float mu, float fx, bool muted, bool save)
    {
        if (muted) mixer.SetFloat(P_Master, -80f);
        else mixer.SetFloat(P_Master, LinearToDb(m));

        mixer.SetFloat(P_Music, LinearToDb(mu));
        mixer.SetFloat(P_Sfx, LinearToDb(fx));

        if (masterValueLabel) masterValueLabel.text = Mathf.RoundToInt(m * 100f) + "%";

        if (save)
        {
            PlayerPrefs.SetFloat(K_Master, m);
            PlayerPrefs.SetFloat(K_Music, mu);
            PlayerPrefs.SetFloat(K_Sfx, fx);
            PlayerPrefs.SetInt(K_Mute, muted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    float LinearToDb(float v) => Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f;

    public void ResetToDefaults()
    {
        ApplyAll(0.8f, 0.8f, 0.8f, false, true);
        master.SetValueWithoutNotify(0.8f);
        music.SetValueWithoutNotify(0.8f);
        sfx.SetValueWithoutNotify(0.8f);
        muteToggle.SetIsOnWithoutNotify(false);
    }
}
