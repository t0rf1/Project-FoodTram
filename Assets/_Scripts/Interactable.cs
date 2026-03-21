using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private GameObject ObjectToInteractWith;
    // Wywoływane gdy kliknie się myszą na ten obiekt
    private void OnMouseDown()
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
