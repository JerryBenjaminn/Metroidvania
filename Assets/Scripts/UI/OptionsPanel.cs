using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    public AudioMixer mixer;     // vedä Master-mixeri
    public Slider masterSlider;  // 0..1

    void OnEnable()
    {
        // lue PlayerPrefs aseta slider (jos haluat)
        float v = PlayerPrefs.GetFloat("opt_master", 0.8f);
        masterSlider.SetValueWithoutNotify(v);
        Apply(v);
        masterSlider.onValueChanged.AddListener(Apply);
    }

    void OnDisable()
    {
        masterSlider.onValueChanged.RemoveListener(Apply);
    }

    void Apply(float v)
    {
        // muunna 0..1 lineaarinen dB [-80, 0]
        float dB = Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f;
        mixer.SetFloat("MasterVolume", dB);
        PlayerPrefs.SetFloat("opt_master", v);
    }
}
