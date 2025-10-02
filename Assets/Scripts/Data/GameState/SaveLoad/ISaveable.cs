using UnityEngine;

public interface ISaveable
{
    // Palauta serialisoitava POCO (luokka/struct, jossa [Serializable])
    object CaptureState();
    // Ota vastaan sama POCO ja palauta tila
    void RestoreState(object state);
}
