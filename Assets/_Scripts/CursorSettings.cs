using UnityEngine;

public class CursorSettings : MonoBehaviour
{
    // Singleton instance
    public static CursorSettings Instance { get; private set; }

    Texture2D cursorTexture; // Przypisz teksturę kursora w inspektorze
    public HoldedItem heldItem; // Przypisz skrypt HoldedItem w inspektorze

    private void Awake()
    {
        // Jeśli już istnieje instancja, usuń duplikat
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Ustaw tę instancję jako singleton
        Instance = this;
        
        // Opcjonalnie: DontDestroyOnLoad, żeby singleton przetrwał zmianę sceny
        // DontDestroyOnLoad(gameObject);
    }

    public void UpdateHeldItemTexture()
    {
        cursorTexture = heldItem.itemTexture;
    }
    public void Clear()
    {
        heldItem = null;
        cursorTexture = null;
    }

    
}