using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private GameObject ObjectToInteractWith;
    [SerializeField] private bool isHold = false;
    // Wywoływane gdy kliknie się myszą na ten obiekt
    private void OnMouseDown()
    {
        if(ObjectToInteractWith != null && !isHold)
        {
            ObjectToInteractWith.GetComponent<I_Interactable>()?.Interact();
        }
        else 
        {
            GetComponent<I_Interactable>()?.Interact();
        } 
    }

    private void OnMouseOver()
    {
        if (isHold && Input.GetMouseButton(0)) // Sprawdza, czy lewy przycisk myszy jest trzymany
        {
            if(ObjectToInteractWith != null)
            {
                ObjectToInteractWith.GetComponent<I_Interactable>()?.Interact();
            }
            else
            {
                GetComponent<I_Interactable>()?.Interact();
            }
        }
    }

    
}
