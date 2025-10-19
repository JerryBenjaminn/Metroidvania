using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    public float lifeTime = 0.7f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
