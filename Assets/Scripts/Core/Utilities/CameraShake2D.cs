using UnityEngine;
#if CINEMACHINE
using Cinemachine;
#endif

public class CameraShake2D : MonoBehaviour
{
#if CINEMACHINE
    [SerializeField] CinemachineImpulseSource impulse;
    void Reset() { impulse = GetComponent<CinemachineImpulseSource>(); }

    public void Shake(float amp = 0.6f, float freq = 1.5f)
    {
        if (!impulse) return;
        impulse.m_DefaultVelocity = Random.insideUnitCircle.normalized;
        impulse.GenerateImpulseWithVelocity(impulse.m_DefaultVelocity * amp);
    }
#else
    Transform cam;
    Vector3 basePos;
    void Awake() { cam = Camera.main ? Camera.main.transform : null; if (cam) basePos = cam.position; }
    public void Shake(float amp = 0.15f, float _ = 0f)
    {
        if (!cam) return;
        cam.position = basePos + (Vector3)(Random.insideUnitCircle * amp);
        // palautuu seuraavalla kamerapäivityksellä tai tee oma palautus jos haluat
    }
#endif
}
