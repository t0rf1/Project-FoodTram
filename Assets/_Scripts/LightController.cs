using UnityEngine;

/// <summary>
/// Kontroler oświetlenia — włącza/wyłącza światła na podstawie stanu korków
/// Zajmuje 1 slot energii w systemie Korki
/// </summary>
public class LightController : MonoBehaviour
{
    [SerializeField] private Light[] lights; // Wszystkie światła które mają być kontrolowane
    [SerializeField] private bool turnOffWhenKorkiTriggered = true; // Czy zgasić światła gdy przewalą się korki

    private Korki korki;
    private bool lastKorkiState = false; // Stan korków z ostatniego sprawdzenia
    private bool lightsAreOn = true; // Czy światła są włączone
    private LightDevice lightDevice; // Proxy dla światła jako urządzenia

    private void Start()
    {
        korki = FindAnyObjectByType<Korki>();
        if (korki == null)
        {
            Debug.LogWarning("LightController: Nie znaleziono skryptu Korki!");
            return;
        }

        // Jeśli nie ma przypisanych świateł, znajdź je automatycznie
        if (lights == null || lights.Length == 0)
        {
            lights = GetComponentsInChildren<Light>();
            if (lights.Length == 0)
            {
                Debug.LogWarning("LightController: Nie znaleziono żadnych świateł!");
            }
        }

        // Utwórz i zarejestruj światło jako urządzenie zajmujące 1 slot energii
        lightDevice = new LightDevice(this);
        korki.RegisterDevice(lightDevice);
        Debug.Log("LightController: Zarejestrowali światło jako urządzenie (1 slot)");
    }

    private void Update()
    {
        if (korki == null) return;

        // Sprawdzamy czy stan korków się zmienił
        bool currentKorkiState = korki.AreKorkiTriggered();
        
        if (currentKorkiState != lastKorkiState)
        {
            lastKorkiState = currentKorkiState;

            if (currentKorkiState && turnOffWhenKorkiTriggered)
            {
                // Korki się przewaliły — gaś światła
                SetLightsEnabled(false);
                lightsAreOn = false;
                // Wyrejestruj światło z systemu
                korki.UnregisterDevice(lightDevice);
                Debug.Log("LightController: Światła zgaszone! Korki przewalone.");
            }
            else if (!currentKorkiState && !lightsAreOn)
            {
                // Korki zostały włączone — włącz światła
                SetLightsEnabled(true);
                lightsAreOn = true;
                // Zarejestruj światło z powrotem
                korki.RegisterDevice(lightDevice);
                Debug.Log("LightController: Światła włączone! Korki naprawione.");
            }
        }
    }

    /// <summary>
    /// Włącza/wyłącza wszystkie kontrolowane światła
    /// </summary>
    private void SetLightsEnabled(bool enabled)
    {
        if (lights == null || lights.Length == 0) return;

        foreach (Light light in lights)
        {
            if (light != null)
            {
                light.enabled = enabled;
            }
        }
    }

    /// <summary>
    /// Ręcznie włącz/wyłącz światła (opcjonalnie)
    /// </summary>
    public void SetLights(bool enabled)
    {
        SetLightsEnabled(enabled);
    }
}

/// <summary>
/// Proxy klasa reprezentująca światło jako urządzenie elektroniczne
/// </summary>
public class LightDevice
{
    private LightController lightController;
    private bool isEnabled = true;

    public LightDevice(LightController controller)
    {
        lightController = controller;
    }

    public bool IsEnabled() => isEnabled;
    public void SetEnabled(bool enabled) => isEnabled = enabled;
}
