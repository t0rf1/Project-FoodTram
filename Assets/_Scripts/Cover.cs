using UnityEngine;
using System.Collections;

public class Cover : MonoBehaviour,I_Interactable
{
    bool isCovered = false;
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
        Vector3 targetPosition;
        
        if (isCovered)
        {
            // Jeśli już przykryty, przejdź w górę (odkryj)
            targetPosition = new Vector3(
                transform.localPosition.x, 
                transform.localPosition.y + coverMoveDistance, 
                transform.localPosition.z
            );
        }
        else
        {
            // Jeśli odkryty, przejdź w dół (przykryj)
            targetPosition = new Vector3(
                transform.localPosition.x, 
                transform.localPosition.y - coverMoveDistance, 
                transform.localPosition.z
            );
        }
        
        StartCoroutine(InterpolateMovement(targetPosition, movementDuration));
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
