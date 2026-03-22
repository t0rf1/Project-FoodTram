using UnityEngine;

/// <summary>
/// Obraca obiekt o określony kąt wzdłuż wybranej osi
/// Obrócenie następuje przy najechaniu myszką, powrót do pozycji początkowej przy zabraniu
/// </summary>
public class RotateOnHover : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    [Header("Rotation Settings")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y; // Oś obrotu
    [SerializeField] private float rotationDegrees = 90f; // Ile stopni się obrócić
    [SerializeField] private float rotationSpeed = 5f; // Szybkość obrotu (lerp speed)
    [SerializeField] private bool flipAxis = false; // Odwróć kierunek (np. -Y zamiast +Y)
    [SerializeField] private Transform targetObject = null; // Opcjonalnie: obiekt do obrócenia zamiast siebie

    private Quaternion originalRotation; // Oryginalna rotacja obiektu
    private Quaternion targetRotation; // Docelowa rotacja
    private bool isHovering = false; // Czy kursor jest nad obiektem
    private Collider objectCollider; // Cache collidera obiektu
    private Transform rotationTransform; // Transform który będzie obracany (this lub targetObject)

    private void Start()
    {
        // Ustal który transform będzie obracany
        rotationTransform = (targetObject != null) ? targetObject : transform;

        // Zapamiętaj oryginalna rotację
        originalRotation = rotationTransform.rotation;
        targetRotation = originalRotation;

        // Cache collider'a (zawsze z tego obiektu, bo on ma collider do detektowania hover'u)
        objectCollider = GetComponent<Collider>();
        if (objectCollider == null)
        {
            Debug.LogWarning("RotateOnHover: Obiekt nie ma Collidera! Skrypt nie będzie działać.");
            enabled = false;
        }
    }

    private void Update()
    {
        // Lerp między obecną a docelową rotacją
        rotationTransform.rotation = Quaternion.Lerp(rotationTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void OnMouseEnter()
    {
        isHovering = true;
        // Oblicz docelową rotację (obrócenie o kąt)
        targetRotation = originalRotation * GetRotationFromAxis();
    }

    private void OnMouseExit()
    {
        isHovering = false;
        // Powróć do oryginalnej rotacji
        targetRotation = originalRotation;
    }

    /// <summary>
    /// Zwraca quaternion obrotu na podstawie wybranej osi
    /// </summary>
    private Quaternion GetRotationFromAxis()
    {
        // Jeśli flipAxis, odwróć kąt
        float angle = flipAxis ? -rotationDegrees : rotationDegrees;

        return rotationAxis switch
        {
            RotationAxis.X => Quaternion.AngleAxis(angle, Vector3.right),
            RotationAxis.Y => Quaternion.AngleAxis(angle, Vector3.up),
            RotationAxis.Z => Quaternion.AngleAxis(angle, Vector3.forward),
            _ => Quaternion.identity
        };
    }
}
