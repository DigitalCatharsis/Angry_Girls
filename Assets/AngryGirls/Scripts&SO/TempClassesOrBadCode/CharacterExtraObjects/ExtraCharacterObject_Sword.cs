using Angry_Girls;
using UnityEngine;

public class ExtraCharacterObject_Sword : MonoBehaviour, IExtraCharacterObject
{
    public void OnRagdollEnabled()
    {
        gameObject.transform.SetParent(transform, false);
    }
}
