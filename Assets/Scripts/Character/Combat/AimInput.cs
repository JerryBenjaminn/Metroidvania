using UnityEngine;

public class AimInput : MonoBehaviour
{
    // viimeisin liikesyöte, ei normalisoida jotta analogin vahvuus toimii kynnyksenä
    public Vector2 Aim { get; private set; }
    public void SetAim(Vector2 v) => Aim = v;
}

