using UnityEngine;
using System.Reflection;

/// <summary>
/// Kontroler LED lampek — pokazuje ilość włączonych urządzeń za pomocą świecących lampek
/// Zielony = pracujące, Czerwony = wyłączone lub korki przewalone
/// </summary>
public class LedController : MonoBehaviour
{
    [SerializeField] private Material greenMaterial; // Materiał zielony (pracujący)
    [SerializeField] private Material redMaterial; // Materiał czerwony (wyłączony)
    
    private MeshRenderer[] ledRenderers; // Renderery 3 LED lampek (dzieci tego obiektu)
    private Korki korki;

    private void Start()
    {
        // Pobierz wszystkie MeshRenderer'y z dzieci (powinno być 3)
        ledRenderers = GetComponentsInChildren<MeshRenderer>();
        
        if (ledRenderers == null || ledRenderers.Length < 3)
        {
            Debug.LogWarning("LedController: Oczekiwano 3 LED lampek jako dzieci tego obiektu!");
            enabled = false;
            return;
        }

        korki = FindAnyObjectByType<Korki>();
        if (korki == null)
        {
            Debug.LogWarning("LedController: Nie znaleziono skryptu Korki!");
        }

        // Inicjalizacja materiałów
        if (greenMaterial == null || redMaterial == null)
        {
            Debug.LogWarning("LedController: Nie przypisano materiałów!");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (korki == null || ledRenderers == null) return;

        // Pobierz ilość aktywnych urządzeń używając refleksji
        int activeDeviceCount = GetActiveDeviceCountFromKorki();
        bool korkiTriggered = korki.AreKorkiTriggered();

        // Aktualizuj wygląd lampek
        for (int i = 0; i < ledRenderers.Length; i++)
        {
            if (ledRenderers[i] == null) continue;

            // Jeśli korki przewalone, wszystkie lampki są czerwone
            if (korkiTriggered)
            {
                SetLedMaterial(ledRenderers[i], redMaterial);
            }
            // Lampka świeci na zielono jeśli jest włączone urządzenie o tym indeksie
            else if (i < activeDeviceCount)
            {
                SetLedMaterial(ledRenderers[i], greenMaterial);
            }
            // Inaczej lampka jest czerwona (wyłączona)
            else
            {
                SetLedMaterial(ledRenderers[i], redMaterial);
            }
        }
    }

    /// <summary>
    /// Pobiera ilość urządzeń z Korki za pomocą refleksji
    /// </summary>
    private int GetActiveDeviceCountFromKorki()
    {
        try
        {
            var method = korki.GetType().GetMethod("GetActiveDeviceCount", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                return (int)method.Invoke(korki, null);
            }
        }
        catch
        {
            // Jeśli refleksja się nie uda, zwróć 0
        }
        return 0;
    }

    /// <summary>
    /// Ustawia materiał dla LED lampki
    /// </summary>
    private void SetLedMaterial(MeshRenderer renderer, Material material)
    {
        if (renderer.material != material)
        {
            renderer.material = material;
        }
    }
}
