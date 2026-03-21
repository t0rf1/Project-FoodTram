using UnityEngine;

public class Kosz : MonoBehaviour, I_Interactable  
{
    public void Interact()
    {
        // Sprawdź, czy gracz trzyma jakiś przedmiot
        if (CursorSettings.Instance.heldItem != null)
        {
            // Jeśli tak, to "wyrzuć" go do kosza
            Debug.Log("Kosz: Wyrzucono " + CursorSettings.Instance.heldItem.gameObject.name);
            CursorSettings.Instance.UpdateHeldItemTexture(); // Aktualizacja tekstury kursora
            CursorSettings.Instance.heldItem = null; // Czyszczenie trzymanego przedmiotu
            
        }
        else
        {
            Debug.Log("Kosz: Nie trzymasz żadnego przedmiotu do wyrzucenia!");
        }
    }

}
