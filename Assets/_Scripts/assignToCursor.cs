using UnityEngine;

public class assignToCursor : MonoBehaviour, I_Interactable
{
    
    private CursorSettings cursorSettings; // Przypisz skrypt CursorSettings w inspektorze
    [SerializeField] HoldedItem itemToAssign; // Przypisz skrypt HoldedItem w inspektorze
    void Start()
    {
        cursorSettings = CursorSettings.Instance;
    }
    public void Interact()
    {
        cursorSettings.heldItem = itemToAssign;
        cursorSettings.UpdateHeldItemTexture();
    }
}
