using System.Collections;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;
    public Sprite icon;
    public float cooldown = 0.2f;

    public virtual bool CanUse(IAbilityUser user) => true;
    public abstract IEnumerator Execute(IAbilityUser user);

    // helperi cooldownille
    protected IEnumerator Cooldown(float seconds) {
        yield return new WaitForSeconds(seconds);
    }
    // lisäys Abilityyn vain debugiin:
#if UNITY_EDITOR
    public void DrawDebugBox(Vector2 center, Vector2 size, float facing)
    {
        var a = center + new Vector2(-size.x / 2, -size.y / 2);
        var b = center + new Vector2(size.x / 2, -size.y / 2);
        var c = center + new Vector2(size.x / 2, size.y / 2);
        var d = center + new Vector2(-size.x / 2, size.y / 2);
        Debug.DrawLine(a, b, Color.red, 0.02f);
        Debug.DrawLine(b, c, Color.red, 0.02f);
        Debug.DrawLine(c, d, Color.red, 0.02f);
        Debug.DrawLine(d, a, Color.red, 0.02f);
    }
#endif

}

