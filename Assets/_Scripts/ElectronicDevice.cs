using UnityEngine;
using System.Collections;

/// <summary>
/// Reprezentuje urządzenie elektroniczne które można włączać/wyłączać
/// Może przetwarzać potrawy — przetwarzana potrawa znika z ręki, a po talerzu czasu wraca gotowa
/// </summary>
public class ElectronicDevice : MonoBehaviour, I_Interactable
{
    [Header("Device Settings")]
    [SerializeField] private bool isEnabled = false; // Czy urządzenie pracuje

    [Header("Food Processing")]
    [SerializeField] private FoodPartType[] acceptedFoodTypes; // Typy potraw które urządzenie przyjmuje
    [SerializeField] private float processingTime = 5f; // Czas obróbki w sekundach
    [SerializeField] private FoodPartType processedFoodType; // Typ jedzenia po przetworzeniu
    [SerializeField] private GameObject processedFoodPrefab; // Prefab gotowego produktu do wydania

    private Korki korki;
    private CursorSettings cursorSettings;
    private bool isProcessing = false; // Czy urządzenie przetwarza potrawę
    private float processingTimer = 0f; // Timer obróbki
    private HoldedItem processedItem = null; // Item do wydania po obróbce

    private void Start()
    {
        korki = FindAnyObjectByType<Korki>();
        if (korki == null)
        {
            Debug.LogWarning("ElectronicDevice: Nie znaleziono skryptu Korki!");
        }

        cursorSettings = CursorSettings.Instance;
        if (cursorSettings == null)
        {
            Debug.LogWarning("ElectronicDevice: Nie znaleziono CursorSettings!");
        }
    }

    private void Update()
    {
        // Jeśli urządzenie przetwarza potrawę, odliczaj timer
        if (isProcessing)
        {
            processingTimer -= Time.deltaTime;

            if (processingTimer <= 0f)
            {
                isProcessing = false;
                Debug.Log($"ElectronicDevice ({gameObject.name}): Obróbka ukończona! Możesz odebrać produkt.");
            }
        }
    }

    /// <summary>
    /// Zwraca czy urządzenie jest włączone
    /// </summary>
    public bool IsEnabled()
    {
        return isEnabled;
    }

    /// <summary>
    /// Ustawia stan urządzenia
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
        Debug.Log($"ElectronicDevice ({gameObject.name}): {(enabled ? "Włączone" : "Wyłączone")}");
    }

    /// <summary>
    /// Kliknięcie na urządzenie — włącza/wyłącza lub przetwarza potrawę
    /// </summary>
    public void Interact()
    {
        // Jeśli urządzenie jest wyłączone lub korki przewalone
        if (korki != null && korki.AreKorkiTriggered())
        {
            Debug.LogWarning("ElectronicDevice: Korki są przewalone! Nie możesz użyć urządzenia.");
            return;
        }

        // Jeśli urządzenie przetwarza potrawę i obróbka ukończona — wydaj produkt
        if (isProcessing && processingTimer <= 0f)
        {
            GiveProcessedFood();
            return;
        }

        // Jeśli już przetwarza — nie rób nic
        if (isProcessing)
        {
            Debug.Log($"ElectronicDevice ({gameObject.name}): Urządzenie jeszcze przetwarza. Czekaj {processingTimer:F1}s");
            return;
        }

        // Jeśli gracz trzyma potrawę którą urządzenie akceptuje
        if (cursorSettings != null && cursorSettings.heldItem != null)
        {
            if (IsAcceptedFoodType(cursorSettings.heldItem.itemType))
            {
                ProcessFood();
                return;
            }
        }

        // W przeciwnym razie — toggle urządzenia
        ToggleDevice();
    }

    /// <summary>
    /// Sprawdza czy urządzenie akceptuje dany typ jedzenia
    /// </summary>
    private bool IsAcceptedFoodType(FoodPartType foodType)
    {
        if (acceptedFoodTypes == null || acceptedFoodTypes.Length == 0)
            return false;

        foreach (FoodPartType accepted in acceptedFoodTypes)
        {
            if (accepted == foodType)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Rozpoczyna przetwarzanie potrawy
    /// </summary>
    private void ProcessFood()
    {
        if (!isEnabled)
        {
            Debug.Log($"ElectronicDevice ({gameObject.name}): Włączam urządzenie automatycznie...");
            SetEnabled(true); // Włącz urządzenie
            
            // Zarejestruj w Korki
            if (korki != null)
            {
                korki.RegisterDevice(this);
            }
        }

        // Pobierz item z ręki
        HoldedItem heldItem = cursorSettings.heldItem;
        Debug.Log($"ElectronicDevice ({gameObject.name}): Rozpoczęto obróbkę {heldItem.itemType}");

        // Usuń item z ręki
        cursorSettings.Clear();

        // Rozpocznij timer obróbki
        isProcessing = true;
        processingTimer = processingTime;

        // Przygotuj produkt do wydania
        if (processedFoodPrefab != null)
        {
            GameObject processedObject = Instantiate(processedFoodPrefab);
            processedItem = processedObject.GetComponent<HoldedItem>();
            if (processedItem == null)
            {
                Debug.LogWarning("ElectronicDevice: Prefab nie ma komponentu HoldedItem!");
            }
        }
        else
        {
            Debug.LogWarning("ElectronicDevice: Brak prefabu przetwarzanego produktu!");
        }
    }

    /// <summary>
    /// Wydaje przetworzony produkt do ręki
    /// </summary>
    private void GiveProcessedFood()
    {
        if (processedItem != null && cursorSettings != null)
        {
            cursorSettings.heldItem = processedItem;
            cursorSettings.UpdateHeldItemTexture(); // Aktualizuj sprite w kursorze
            Debug.Log($"ElectronicDevice ({gameObject.name}): Wydano przetworzony produkt!");
            processedItem = null;
        }
        else
        {
            Debug.LogWarning("ElectronicDevice: Nie ma produktu do wydania!");
        }
    }

    /// <summary>
    /// Przełącza stan urządzenia
    /// </summary>
    private void ToggleDevice()
    {
        // Przełączamy stan
        bool newState = !isEnabled;
        SetEnabled(newState);

        // Informujemy system Korki o zmianie
        if (korki != null)
        {
            if (newState)
                korki.RegisterDevice(this);
            else
                korki.UnregisterDevice(this);
        }
    }
}
