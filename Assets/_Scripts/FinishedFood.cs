using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Reprezentuje gotową potrawę z listą użytych składników
/// </summary>
public class FinishedFood : MonoBehaviour
{
    public FoodPartType foodType;
    public List<SkladnikType> usedIngredients = new List<SkladnikType>();

    public void SetFoodData(FoodPartType type, List<SkladnikType> ingredients)
    {
        foodType = type;
        usedIngredients = new List<SkladnikType>(ingredients);
        Debug.Log($"FinishedFood: Stworzono {type} z składnikami: {string.Join(", ", usedIngredients)}");
    }

    public void DisplayFoodInfo()
    {
        Debug.Log($"🍴 {foodType}: {string.Join(", ", usedIngredients)}");
    }
}
