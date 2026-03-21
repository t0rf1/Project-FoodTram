using UnityEngine;

public class HoldedItem : MonoBehaviour
{
    public Texture2D itemTexture; // Przypisz teksturę trzymanego przedmiotu w inspektorze
    public FoodPartType itemType; // Określ typ trzymanego przedmiotu (np. Kebab, Frytobula, Kanapka)

    [ShowIf(nameof(itemType), (int)FoodPartType.Skladnik)]
    public SkladnikType skladnikType; // Określ typ składnika, jeśli itemType to Skladnik
    
}