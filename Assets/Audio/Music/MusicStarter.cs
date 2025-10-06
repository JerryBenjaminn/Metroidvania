using UnityEngine;
public class MusicStarter : MonoBehaviour
{
    public AudioClip levelMusic;
    public float fade = 0.6f;
    void Start() { if (levelMusic) AudioManager.Instance.PlayMusic(levelMusic, fade); }
}
