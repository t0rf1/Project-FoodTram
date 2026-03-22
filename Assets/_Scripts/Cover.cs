using UnityEngine;
using System.Collections;

public class Cover : MonoBehaviour, I_Interactable
{
    public enum MovementAxis
    {
        Y_Axis, // Góra/Dół
        X_Axis, // Lewo/Prawo
        Z_Axis  // Przód/Tył
    }
    [SerializeField] private MovementAxis movementAxis = MovementAxis.Y_Axis; // Wybieralna oś z inspektora
    [SerializeField] private bool flipAxis = false; // Odwróć kierunek (np. -Y zamiast +Y)

    public bool isCovered = false;
    public float coverMoveDistance = 0.5f;
    public float movementDuration = 0.5f; // czas trwania interpolacji (w sekundach)
    private bool isMoving = false; // flaga, żeby uniknąć wielokrotnych ruchów jednocześnie

    public void Interact()
    {
        if (!isMoving)
        {
            MoveCover();
            isCovered = !isCovered;
        }
    }

    private void MoveCover()
    {
        Vector3 targetPosition = transform.localPosition;
        
        if (isCovered)
        {
            // Jeśli już przykryty, przejdź w kierunku pozytywnym (odkryj)
            targetPosition += GetAxisDirection() * coverMoveDistance;
        }
        else
        {
            // Jeśli odkryty, przejdź w kierunku negatywnym (przykryj)
            targetPosition -= GetAxisDirection() * coverMoveDistance;
        }
        
        StartCoroutine(InterpolateMovement(targetPosition, movementDuration));
    }

    /// <summary>
    /// Zwraca wektor kierunku na podstawie wybranej osi
    /// </summary>
    private Vector3 GetAxisDirection()
    {
        Vector3 direction = movementAxis switch
        {
            MovementAxis.X_Axis => Vector3.right,  // (1, 0, 0)
            MovementAxis.Y_Axis => Vector3.up,     // (0, 1, 0)
            MovementAxis.Z_Axis => Vector3.forward,// (0, 0, 1)
            _ => Vector3.up
        };

        // Jeśli flipAxis jest włączony, odwróć kierunek
        if (flipAxis)
        {
            direction *= -1f;
        }

        return direction;
    }

    private IEnumerator InterpolateMovement(Vector3 targetPosition, float duration)
    {
        isMoving = true;
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration); // wartość od 0 do 1
            
            // Interpolacja liniowa (Lerp) między pozycją startową a docelową
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            
            yield return null;
        }

        // Upewnij się, że kończymy dokładnie w pozycji docelowej
        transform.localPosition = targetPosition;
        isMoving = false;
    }
}
