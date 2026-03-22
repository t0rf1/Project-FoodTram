using UnityEngine;

/// <summary>
/// Komponent do przypisania każdemu dziecku RandomAppear
/// Definiuje na których kamerach to dziecko może się pojawić
/// </summary>
public class RandomAppearChild : MonoBehaviour
{
    [System.Flags]
    public enum CameraPages
    {
        Front = 1 << 0,   // Kamera przednia (index 0)
        Left = 1 << 1,    // Kamera lewa (index 1)
        Back = 1 << 2,    // Kamera tylna (index 2)
        Right = 1 << 3,   // Kamera prawa (index 3)
        Lower = 1 << 4    // Kamera dolna (index 4)
    }

    [SerializeField] private CameraPages enabledPages = CameraPages.Front;
    [SerializeField] private float appearChance = 0.1f; // Szansa pojawienia się na tej kamerze

    private Vector3 initialWorldPosition; // Zapamiętana pozycja startowa (światowa)

    private void Awake()
    {
        // Zapamiętaj pozycję światową przed wyłączeniem obiektu
        // To gwarantuje że pozycja będzie taka sama niezależnie od pozycji rodzica
        initialWorldPosition = transform.position;
    }

    /// <summary>
    /// Sprawdza czy to dziecko może się pojawić na danej kamerze
    /// </summary>
    public bool CanAppearOnCamera(int cameraIndex)
    {
        return cameraIndex switch
        {
            0 => (enabledPages & CameraPages.Front) != 0,
            1 => (enabledPages & CameraPages.Left) != 0,
            2 => (enabledPages & CameraPages.Back) != 0,
            3 => (enabledPages & CameraPages.Right) != 0,
            4 => (enabledPages & CameraPages.Lower) != 0,
            _ => false
        };
    }

    /// <summary>
    /// Zwraca szansę pojawienia się tego konkretnego obiektu
    /// </summary>
    public float GetAppearChance()
    {
        return appearChance;
    }

    /// <summary>
    /// Resetuje pozycję obiektu do początkowej (światowej)
    /// </summary>
    public void ResetToInitialPosition()
    {
        transform.position = initialWorldPosition;
    }
}
