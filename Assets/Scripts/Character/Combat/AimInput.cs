using UnityEngine;

public class AimInput : MonoBehaviour
{
    // viimeisin liikesy�te, ei normalisoida jotta analogin vahvuus toimii kynnyksen�
    public Vector2 Aim { get; private set; }
    public void SetAim(Vector2 v) => Aim = v;
}

