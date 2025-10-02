using UnityEngine;

[DisallowMultipleComponent]
public class SaveableEntity : MonoBehaviour
{
    [SerializeField] string id;
    public string Id => id;

    void OnValidate()
    {
        // Luo uusi GUID jos puuttuu (ja vain editorissa)
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString();
    }

    public ISaveable[] GetSaveables() => GetComponents<ISaveable>();
}
