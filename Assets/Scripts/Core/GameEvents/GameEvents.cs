using System;
using UnityEngine;


public static class GameEvents
{
    public static event Action<string> OnAbilityUnlocked;
    public static void AbilityUnlocked(string name) => OnAbilityUnlocked?.Invoke(name);
}
