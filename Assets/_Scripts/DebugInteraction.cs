using UnityEngine;

public class DebugInteraction : MonoBehaviour, I_Interactable
{
    public void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }
}
