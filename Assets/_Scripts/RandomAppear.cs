using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Pokazuje losowe dziecko gdy zmieni się na wybraną kamerę
/// Każde dziecko ma RandomAppearChild komponent do konfiguracji
/// </summary>
public class RandomAppear : MonoBehaviour
{
    [SerializeField] private float appearDuration = 1f; // Ile sekund obiekt będzie widoczny
    [SerializeField] private float disappearAnimationDuration = 0.3f; // Jak szybko poruszać się przy znikaniu
    [SerializeField] private float disappearDistance = 5f; // Jak daleko się poruszać

    private CameraManager cameraManager;
    private int lastCameraIndex = -1; // Indeks ostatniej kamery
    private bool isCurrentlyAppearing = false; // Czy coś się teraz pojawia
    private Transform[] children; // Dzieci tego obiektu
    private RandomAppearChild[] childConfigs; // Konfiguracje każdego dziecka

    private void Start()
    {
        cameraManager = FindAnyObjectByType<CameraManager>();
        if (cameraManager == null)
        {
            Debug.LogWarning("RandomAppear: Nie znaleziono CameraManager!");
            enabled = false;
            return;
        }

        // Pobierz wszystkie dzieci
        children = GetComponentsInChildren<Transform>();
        
        // Ustalenie że mamy co najmniej jedną transformę (sam obiekt)
        if (children.Length <= 1)
        {
            Debug.LogWarning("RandomAppear: Nie znaleziono żadnych dzieci tego obiektu!");
            enabled = false;
            return;
        }

        // Pobierz konfiguracje każdego dziecka
        childConfigs = new RandomAppearChild[children.Length];
        for (int i = 1; i < children.Length; i++)
        {
            childConfigs[i] = children[i].GetComponent<RandomAppearChild>();
            // Jeśli dziecko nie ma RandomAppearChild, wyłącz je
            if (childConfigs[i] == null)
            {
                children[i].gameObject.SetActive(false);
            }
            else
            {
                children[i].gameObject.SetActive(false);
            }
        }

        lastCameraIndex = -1; // Zresetuj indeks kamery
    }

    private void Update()
    {
        if (cameraManager == null || children == null || children.Length <= 1) return;

        // Pobierz bieżący indeks kamery
        int currentCameraIndex = GetCurrentCameraIndex();

        // Sprawdź czy zmieniliśmy kamerę
        if (currentCameraIndex != lastCameraIndex && currentCameraIndex >= 0)
        {
            OnCameraChanged(currentCameraIndex);
        }

        lastCameraIndex = currentCameraIndex;
    }

    /// <summary>
    /// Callback gdy zmieni się kamera
    /// </summary>
    private void OnCameraChanged(int cameraIndex)
    {
        // Zbierz wszystkie dzieci które mogą się pojawić na tej kamerze
        List<int> validChildren = new List<int>();
        
        for (int i = 1; i < children.Length; i++)
        {
            if (childConfigs[i] != null && childConfigs[i].CanAppearOnCamera(cameraIndex))
            {
                validChildren.Add(i);
            }
        }

        // Jeśli nie ma żadnych ważnych dzieci, wyjdź
        if (validChildren.Count == 0) return;

        // Losuj jedno z ważnych dzieci
        int randomIndex = validChildren[Random.Range(0, validChildren.Count)];
        GameObject selectedChild = children[randomIndex].gameObject;
        RandomAppearChild config = childConfigs[randomIndex];

        // Sprawdź szansę pojawienia się tego konkretnego obiektu
        if (Random.value <= config.GetAppearChance() && !isCurrentlyAppearing)
        {
            StartCoroutine(ShowObjectTemporarily(selectedChild));
        }
    }

    /// <summary>
    /// Pokazuje obiekt na określony czas, potem animuje jego zniknięcie
    /// </summary>
    private IEnumerator ShowObjectTemporarily(GameObject targetObject)
    {
        isCurrentlyAppearing = true;
        
        // Resetuj pozycję obiektu do początkowej
        RandomAppearChild childConfig = targetObject.GetComponent<RandomAppearChild>();
        if (childConfig != null)
        {
            childConfig.ResetToInitialPosition();
        }
        
        targetObject.SetActive(true);
        Debug.Log($"RandomAppear: Pojawił się {targetObject.name}!");

        yield return new WaitForSeconds(appearDuration);

        // Animacja zniknięcia - szybki ruch w losowym kierunku
        yield return StartCoroutine(AnimateDisappear(targetObject));

        targetObject.SetActive(false);
        isCurrentlyAppearing = false;
        Debug.Log($"RandomAppear: Zniknął {targetObject.name}!");
    }

    /// <summary>
    /// Animuje szybki ruch obiektu w losowym kierunku (lewo-prawo lub góra-dół)
    /// </summary>
    private IEnumerator AnimateDisappear(GameObject targetObject)
    {
        Transform targetTransform = targetObject.transform;
        Vector3 startPosition = targetTransform.localPosition;
        
        // Losuj kierunek: 0 = lewo, 1 = prawo, 2 = góra, 3 = dół
        int direction = Random.Range(0, 4);
        Vector3 moveDirection = Vector3.zero;

        switch (direction)
        {
            case 0: // Lewo
                moveDirection = Vector3.left;
                break;
            case 1: // Prawo
                moveDirection = Vector3.right;
                break;
            case 2: // Góra
                moveDirection = Vector3.up;
                break;
            case 3: // Dół
                moveDirection = Vector3.down;
                break;
        }

        Vector3 endPosition = startPosition + moveDirection * disappearDistance;
        float elapsedTime = 0f;

        while (elapsedTime < disappearAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / disappearAnimationDuration);
            
            // Interpoluj pozycję
            targetTransform.localPosition = Vector3.Lerp(startPosition, endPosition, progress);
            
            yield return null;
        }

        // Upewnij się że osiągnęliśmy końcową pozycję
        targetTransform.localPosition = endPosition;
    }

    /// <summary>
    /// Pobiera bieżący indeks kamery - używa refleksji bo pole je private
    /// </summary>
    private int GetCurrentCameraIndex()
    {
        try
        {
            var field = cameraManager.GetType().GetField("currentIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (int)field.GetValue(cameraManager);
            }
        }
        catch { }
        return -1; // Fallback
    }
}
