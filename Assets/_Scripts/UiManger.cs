using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemHeld; // Przypisz Text w inspektorze

    public void UpdateItemHeldText()
    {
        if (CursorSettings.Instance.heldItem == null)
        {
            itemHeld.text = "Nie trzymasz żadnego składnika!";
        }
        else
        {
            itemHeld.text = $"Trzymasz: {CursorSettings.Instance.heldItem.gameObject.name}";
        }
        
    }
    void Update()
    {
        UpdateItemHeldText();
    }
}