using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Audio/Sound Event")]
public class SoundEvent : ScriptableObject
{
    public AudioClip[] clips;

    [Header("Levels")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(-3f, 3f)] public float pitch = 1f;
    [Range(0f, 1f)] public float pitchVariance = 0.05f;

    [Header("Spatial")]
    [Range(0f, 1f)] public float spatialBlend = 1f;  // 0 = 2D, 1 = 3D
    public float minDistance = 1f;
    public float maxDistance = 20f;

    [Header("Routing")]
    public AudioMixerGroup outputOverride; // tyhjä = käytä AudioManagerin oletusta (SFX/UI)

    [Header("Limits")]
    public float cooldown = 0f;       // sekuntia
    public int maxSimultaneous = 8;   // sama eventti yhtä aikaa

    public AudioClip PickClip() => (clips != null && clips.Length > 0)
        ? clips[Random.Range(0, clips.Length)]
        : null;
}
