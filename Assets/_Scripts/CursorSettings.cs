using UnityEngine;
using UnityEngine.UI;

public class CursorSettings : MonoBehaviour
{
    public enum CursorState
    {
        Normal,      // Zwykły kursor
        Hover,       // Najechanie na obiekt
        Holding      // Podniesienie obiektu
    }

    // Singleton instance
    public static CursorSettings Instance { get; private set; }

    [Header("Cursor Appearance")]
    [SerializeField] private Texture2D cursorNormalTexture; // Kursor zwykły
    [SerializeField] private Texture2D cursorHoverTexture; // Kursor przy najechaniu
    [SerializeField] private Texture2D cursorHoldingTexture; // Kursor przy podnoszeniu
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero; // Punkt aktywny kursora

    [Header("Cursor Sprite Display")]
    [SerializeField] private Image cursorMainSpriteDisplay; // Sprite UI bezpośrednio na kursorze (nakładka)
    [SerializeField] private Image cursorHeldItemDisplay; // Sprite UI obok kursora (trzymany item)
    [SerializeField] private Vector2 cursorSpriteOffset = new Vector2(20, -20); // Offset sprite'u od kursora
    [SerializeField] private Vector2 cursorScale = Vector2.one; // Skala kursora
    [SerializeField] private Vector2 heldItemScale = Vector2.one; // Skala trzymanego przedmiotu
    
    public HoldedItem heldItem; // Przypisz skrypt HoldedItem w inspektorze

    private RectTransform mainSpriteRectTransform; // Cache RectTransform dla sprite'u na kursorze
    private RectTransform heldItemRectTransform; // Cache RectTransform dla sprite'u trzymanego itemu
    private CursorState currentCursorState = CursorState.Normal; // Obecny stan kursora
    private Texture2D cursorTexture; // Bieżąca tekstura kursora
    private Camera mainCamera; // Cache głównej kamery

    private void Awake()
    {
        // Jeśli już istnieje instancja, usuń duplikat
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Cursor.visible = false;

        // Ustaw tę instancję jako singleton
        Instance = this;
        
        // Cache RectTransform sprite'u
        if (cursorMainSpriteDisplay != null)
        {
            mainSpriteRectTransform = cursorMainSpriteDisplay.GetComponent<RectTransform>();
        }
        if (cursorHeldItemDisplay != null)
        {
            heldItemRectTransform = cursorHeldItemDisplay.GetComponent<RectTransform>();
        }

        // Ustaw domyślny kursor
        if (cursorNormalTexture != null)
        {
            SetCursorState(CursorState.Normal);
        }
        
        // Cache główną kamerę
        mainCamera = Camera.main;
        
        // Opcjonalnie: DontDestroyOnLoad, żeby singleton przetrwał zmianę sceny
        // DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Aktualizuj pozycję obu sprite'ów obok kursora
        Vector2 mousePos = Input.mousePosition;
        
        // Sprite główny (nakładka na kursor)
        if (cursorMainSpriteDisplay != null && mainSpriteRectTransform != null)
        {
            mainSpriteRectTransform.position = mousePos;
            // Zastosuj skalę kursora
            mainSpriteRectTransform.localScale = cursorScale;
        }
        
        // Sprite trzymanego itemu
        if (cursorHeldItemDisplay != null && heldItemRectTransform != null)
        {
            heldItemRectTransform.position = mousePos + cursorSpriteOffset;
            // Zastosuj skalę trzymanego przedmiotu
            heldItemRectTransform.localScale = heldItemScale;
        }

        // Sprawdź czy kursor jest nad interactable obiektem
        CheckInteractableHover();
    }

    /// <summary>
    /// Sprawdza czy kursor jest nad obiektem interactable
    /// </summary>
    private void CheckInteractableHover()
    {
        // Nie zmieniaj tekstury jeśli coś trzymasz (Holding)
        if (currentCursorState == CursorState.Holding)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Sprawdź czy trafiony obiekt ma komponent Interactable
            I_Interactable interactable = hit.collider.GetComponent<I_Interactable>();
            if (interactable != null)
            {
                SetCursorState(CursorState.Hover);
                return;
            }
        }

        // Jeśli nie mamy nic trzymanego i nie ma obiektu do interakcji, ustaw Normal
        if (currentCursorState != CursorState.Holding)
        {
            SetCursorState(CursorState.Normal);
        }
    }

    public void UpdateHeldItemTexture()
    {
        // Zaktualizuj sprite UI trzymanego itemu
        if (cursorHeldItemDisplay != null && heldItem != null && heldItem.itemTexture != null)
        {
            // Utwórz sprite z tekstury
            cursorHeldItemDisplay.sprite = Sprite.Create(
                heldItem.itemTexture,
                new Rect(0, 0, heldItem.itemTexture.width, heldItem.itemTexture.height),
                Vector2.one * 0.5f
            );
            cursorHeldItemDisplay.enabled = true;
            SetCursorState(CursorState.Holding); // Zmień stan na podnoszenie
        }
    }

    public void Clear()
    {
        heldItem = null;
        
        // Wyłącz sprite UI trzymanego itemu jeśli jest pusty
        if (cursorHeldItemDisplay != null)
        {
            cursorHeldItemDisplay.enabled = false;
        }

        // Resetuj kursor do normalnego
        SetCursorState(CursorState.Normal);
    }

    /// <summary>
    /// Zmienia stan kursora (Normal, Hover, Holding)
    /// </summary>
    public void SetCursorState(CursorState state)
    {
        currentCursorState = state;
        
        Texture2D textureToUse = state switch
        {
            CursorState.Normal => cursorNormalTexture,
            CursorState.Hover => cursorHoverTexture ?? cursorNormalTexture,
            CursorState.Holding => cursorHoldingTexture ?? cursorNormalTexture,
            _ => cursorNormalTexture
        };

        SetCursor(textureToUse);
        UpdateMainSpriteDisplay(textureToUse);
    }

    /// <summary>
    /// Aktualizuje sprite UI na kursorze na podstawie tekstury
    /// </summary>
    private void UpdateMainSpriteDisplay(Texture2D texture)
    {
        if (cursorMainSpriteDisplay != null && texture != null)
        {
            // Utwórz sprite z tekstury
            cursorMainSpriteDisplay.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.one * 0.5f
            );
            cursorMainSpriteDisplay.enabled = true;
        }
        else if (cursorMainSpriteDisplay != null)
        {
            // Wyłącz sprite jeśli nie ma tekstury
            cursorMainSpriteDisplay.enabled = false;
        }
    }

    /// <summary>
    /// Zmienia wygląd hardware kursora
    /// Zawsze ukrywa normalny kursor systemowy
    /// </summary>
    public void SetCursor(Texture2D newCursorTexture)
    {
        // Upewnij się że kursor systemowy jest zawsze ukryty
        Cursor.visible = false;
        
        if (newCursorTexture != null)
        {
            // Ustaw custom kursor
            Cursor.SetCursor(newCursorTexture, cursorHotspot, CursorMode.Auto);
            cursorTexture = newCursorTexture;
        }
        else
        {
            // Resetuj do brak kursora (również ukryty)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            cursorTexture = null;
        }
    }

    
}