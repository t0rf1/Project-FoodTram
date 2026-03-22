using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public enum FoodPartType
{
    none,
    Kebab,
    Frytobula,
    Kanapka,

    Skladnik,
    Napoj,

    ReadyFood
}
public enum SkladnikType
{
   frytki,
   kurczak,
   wolowina,
   sosCzosnkowy,
    sosOstry,
    ketchup,
    paprykarz,

    sorowka,
    frytkimroz,
    pasztecik,
    
}

/// <summary>
/// Definiuje przepis dla potrawy — główny składnik, wymagana ilość składników i referencje do prefabów
/// </summary>
[System.Serializable]
public class FoodRecipe
{
    public FoodPartType foodType;
    public int requiredIngredientCount = 2; // Ile składników musi mieć takie danie

    public GameObject baseIngredientPrefab; // Prefab głównego składnika (np. Kebab, Frytobula, Kanapka)
    public GameObject[] ingredientPrefabs; // Prefaby składników dla tego dania
}

public class FoodComposer : MonoBehaviour, I_Interactable
{
    [Header("Food Recipes")]
    public FoodRecipe[] foodRecipes; // Przypisz przepisy dla każdego dania w inspektorze

    [Header("Spawn Settings")]
    public Transform SpawnPoint; // Punkt, w którym będą pojawiać się składniki
    public GameObject finishedFoodPrefab; // Prefab gotowego produktu do spawnowania na desce
    public Transform finishedFoodSpawnPoint; // Punkt spawnowania gotowego produktu (na desce)

    // Bieżące dane o komponowaniu
    private FoodPartType currentFoodType = FoodPartType.none;
    private int ingredientsAdded = 0; // Ile składników już dodano
    private FoodRecipe currentRecipe = null; // Aktualny przepis
    private List<SkladnikType> usedIngredients = new List<SkladnikType>(); // Lista użytych składników
    public FinishedFood currentFinishedFood = null; // Referencja do gotowego produktu na desce

    CursorSettings cursorSettings;

    void Start()
    {
        cursorSettings = CursorSettings.Instance;
    }

    /// <summary>
    /// Pobiera przepis dla danego typu jedzenia
    /// </summary>
    private FoodRecipe GetRecipeForFoodType(FoodPartType foodType)
    {
        return System.Array.Find(foodRecipes, recipe => recipe.foodType == foodType);
    }

    /// <summary>
    /// Inicjuje komponowanie dania — ustawia typ i resetuje licznik
    /// </summary>
    public void InitiateFoodComposition(FoodPartType foodType)
    {
        currentRecipe = GetRecipeForFoodType(foodType);

        if (currentRecipe == null)
        {
            Debug.LogWarning($"FoodComposer: Brak przepisu dla typu {foodType}!");
            return;
        }

        // Resetuj, jeśli zmieniam typ dania
        if (currentFoodType != foodType)
        {
            ClearFoodChildren();
            ingredientsAdded = 0;
            usedIngredients.Clear();
            currentFoodType = foodType;
            Debug.Log($"FoodComposer: Zaczynam komponować {foodType}. Wymagane składników: {currentRecipe.requiredIngredientCount}");
            var mainIngidient = Instantiate(currentRecipe.baseIngredientPrefab, SpawnPoint.position, Quaternion.identity, transform); // Spawnuje główny składnik jako dziecko tego obiektu
            mainIngidient.tag = "food";
        }
    }

    /// <summary>
    /// Dodaje następny składnik do potrawy
    /// </summary>
    public void AddIngredient()
    {
        if (currentRecipe == null || currentFoodType == FoodPartType.none)
        {
            Debug.LogWarning("FoodComposer: Nie wybrano typu potrawy!");
            return;
        }

        // Sprawdź, czy już mamy wystarczająco składników
        if (ingredientsAdded >= currentRecipe.requiredIngredientCount)
        {
            Debug.Log($"FoodComposer: {currentFoodType} jest już gotowa! Wymagane: {currentRecipe.requiredIngredientCount}, dodano: {ingredientsAdded}");
            return;
        }

        // Pobierz prefab składnika
        if (currentRecipe.ingredientPrefabs == null || currentRecipe.ingredientPrefabs.Length == 0)
        {
            Debug.LogWarning($"FoodComposer: Brak prefabów składników dla {currentFoodType}!");
            return;
        }

        GameObject ingredientPrefab = currentRecipe.ingredientPrefabs[ingredientsAdded];
        
        if (ingredientPrefab == null)
        {
            Debug.LogWarning($"FoodComposer: Prefab składnika {ingredientsAdded} jest null!");
            return;
        }

        // Instantiate składnik
        var ingredient = Instantiate(ingredientPrefab, SpawnPoint.position, Quaternion.identity);
        ingredient.tag = "food";
        ingredient.transform.SetParent(transform);

        ingredientsAdded++;
        
        // Dodaj typ składnika do listy użytych (jeśli trzymasz Skladnik z typem)
        if (cursorSettings != null && cursorSettings.heldItem != null && 
            cursorSettings.heldItem.itemType == FoodPartType.Skladnik)
        {
            usedIngredients.Add(cursorSettings.heldItem.skladnikType);
        }

        Debug.Log($"FoodComposer: Dodano składnik ({ingredientsAdded}/{currentRecipe.requiredIngredientCount})");

        // Jeśli dodaliśmy ostatni składnik — automatycznie finalizuj!
        if (ingredientsAdded >= currentRecipe.requiredIngredientCount)
        {
            FinalizeFood();
        }
    }

    /// <summary>
    /// Finalizuje potrawę — spawna gotowy produkt na desce z listą składników
    /// </summary>
    public void FinalizeFood()
    {
        if (currentRecipe == null || currentFoodType == FoodPartType.none)
        {
            Debug.LogWarning("FoodComposer: Nie masz potrawy do sfinalizowania!");
            return;
        }

        if (ingredientsAdded < currentRecipe.requiredIngredientCount)
        {
            Debug.LogWarning($"FoodComposer: Niewystarczająco składników! Masz: {ingredientsAdded}, wymagane: {currentRecipe.requiredIngredientCount}");
            return;
        }

        Debug.Log($"🍴 FoodComposer: {currentFoodType} jest gotowa! ({ingredientsAdded} składników)");
        
        // Spawnuj gotowy produkt na desce
        SpawnFinishedFood();

        ResetComposer();
    }

    /// <summary>
    /// Spawna gotowy produkt na desce (nie jako child) z informacją o użytych składnikach
    /// </summary>
    private void SpawnFinishedFood()
    {
        if (finishedFoodPrefab == null || finishedFoodSpawnPoint == null)
        {
            Debug.LogWarning("FoodComposer: Brak prefabu gotowego produktu lub punktu spawnowania!");
            return;
        }

        // Spawnuj jako dziecko tego obiektu
        GameObject finishedProduct = Instantiate(
            finishedFoodPrefab, 
            finishedFoodSpawnPoint.position, 
            Quaternion.identity,
            transform  // Jako child tego obiektu
        );

        // Upewnij się że produkt jest aktywny
        finishedProduct.SetActive(true);

        // Ustaw tag żeby nie był usuwany przez ClearFoodChildren()
        finishedProduct.tag = "finishedFood";

        // Przypisz dane o potrawę
        FinishedFood foodComponent = finishedProduct.GetComponent<FinishedFood>();
        if (foodComponent != null)
        {
            foodComponent.SetFoodData(currentFoodType, usedIngredients);
            currentFinishedFood = foodComponent; // Zachowaj referencję
        }
        else
        {
            Debug.LogWarning("FoodComposer: Prefab nie ma komponentu FinishedFood!");
        }
    }

    /// <summary>
    /// Resetuje stan kompozer'a (ale nie czyści gotowego produktu na desce)
    /// </summary>
    public void ResetComposer()
    {
        ClearFoodChildren();
        ingredientsAdded = 0;
        usedIngredients.Clear();
        currentFoodType = FoodPartType.none;
        currentRecipe = null;
        // NIE zerujemy currentFinishedFood — będzie czyszczone przy podnoszeniu produktu
        Debug.Log("FoodComposer: Zresetowano");
    }

    /// <summary>
    /// Usuwa wszystkie obiekty-dzieci z tagiem "food"
    /// </summary>
    private void ClearFoodChildren()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("food"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Obsługa kliknięcia — zbieranie gotowego produktu lub dodanie składnika
    /// </summary>
    public void Interact()
    {
        if (cursorSettings == null)
        {
            Debug.LogError("FoodComposer: CursorSettings nie znaleziony!");
            return;
        }

        Debug.Log($"FoodComposer.Interact() → currentFinishedFood={currentFinishedFood}, heldItem={cursorSettings.heldItem}");

        // Najpierw sprawdzamy czy jest gotowy produkt i gracz nic nie trzyma
        if (currentFinishedFood != null && cursorSettings.heldItem == null)
        {
            Debug.Log("FoodComposer: Zbieranie gotowego produktu...");
            
            // Pobieramy komponent HoldedItem z gotowego produktu
            HoldedItem finishedFoodItem = currentFinishedFood.GetComponent<HoldedItem>();
            
            if (finishedFoodItem != null)
            {
                // Przydzielamy do kursora
                cursorSettings.heldItem = finishedFoodItem;
                // Aktualizuj sprite w kursorze
                cursorSettings.UpdateHeldItemTexture();
                Debug.Log("FoodComposer: Podniesiono gotowy produkt " + currentFinishedFood.foodType);

                // Niszczymy produkt z deski
                Destroy(currentFinishedFood.gameObject);
                currentFinishedFood = null;

                // Resetujemy inne dane (ale nie czyszczmy wszystkiego)
                ingredientsAdded = 0;
                usedIngredients.Clear();
                Debug.Log("FoodComposer: Przygotowano na nowe danie");
                return;
            }
            else
            {
                Debug.LogWarning("FoodComposer: Gotowy produkt nie ma komponentu HoldedItem!");
                return;
            }
        }

        // Jeśli gracz nic nie trzyma — nie możemy nic zrobić
        if (cursorSettings.heldItem == null)
        {
            Debug.LogWarning("FoodComposer: Nie trzymasz żadnego przedmiotu!");
            return;
        }

        FoodPartType itemType = cursorSettings.heldItem.itemType;

        // Jeśli trzymasz główny składnik — zacznij nowe danie
        if (itemType == FoodPartType.Kebab || itemType == FoodPartType.Frytobula || itemType == FoodPartType.Kanapka)
        {
            InitiateFoodComposition(itemType);
            cursorSettings.Clear();
            Debug.Log($"FoodComposer: Założono bazę: {itemType}");
        }
        // Jeśli trzymasz zwykły składnik — dodaj do bieżącego dania
        else if (itemType == FoodPartType.Skladnik)
        {
            AddIngredient();
            cursorSettings.Clear();
        }
        else
        {
            Debug.Log($"FoodComposer: {itemType} nie może być użyty tutaj!");
        }
    }
}
