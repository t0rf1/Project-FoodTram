using UnityEngine;

/// <summary>
/// Reprezentuje urządzenie elektroniczne które można włączać/wyłączać
/// </summary>
public class ElectronicDevice : MonoBehaviour, I_Interactable
{
    [SerializeField] private bool isEnabled = true; // Czy urządzenie pracuje

    private Korki korki;

    private void Start()
    {
        korki = FindAnyObjectByType<Korki>();
        if (korki == null)
        {
            Debug.LogWarning("ElectronicDevice: Nie znaleziono skryptu Korki!");
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
    /// Kliknięcie na urządzenie — włącza/wyłącza je
    /// </summary>
    public void Interact()
    {
        if (korki == null || korki.AreKorkiTriggered())
        {
            Debug.LogWarning("ElectronicDevice: Korki są przewalone! Nie możesz włączyć urządzenia.");
            return;
        }

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
