using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private GameObject ObjectToInteractWith;
    [SerializeField] private bool isHold = false;

    [SerializeField] private bool hoverActivation = false;
    // Wywoływane gdy kliknie się myszą na ten obiekt
    private void OnMouseDown()
    {
        if(ObjectToInteractWith != null && !isHold && !hoverActivation)
        {
            ObjectToInteractWith.GetComponent<I_Interactable>()?.Interact();
        }
        else 
        {
            GetComponent<I_Interactable>()?.Interact();
        } 
    }
    bool isMouseOver = false;
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
        if(hoverActivation && !isMouseOver)
        {
            isMouseOver = true;
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
    
     private void OnMouseExit()
     {
         if(hoverActivation)
         {
             isMouseOver = false;
         }
     }

    
}
