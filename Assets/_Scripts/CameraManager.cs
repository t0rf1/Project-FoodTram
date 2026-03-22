using Unity.Cinemachine;
using UnityEngine;
using System.Linq;
using System.Collections;


public class CameraManager : MonoBehaviour
{
    public CinemachineCamera[] virtualCameras; // Przypisz kamery Cinemachine w inspektorze (dzieci: lewo, środek, prawo)
    private int currentIndex = 0;

    [Header("Transition")]
    public float fovOvershoot = 6f; // ile stopni FOV przy overshocie
    public float fovAnimDuration = 0.18f; // czas animacji overshoota (tam i z powrotem)

    [Header("Priority Settings")]
    public int activeCameraPriority = 10; // priorytet dla aktywnych kamer (lewo, środek, prawo)
    public int inactiveCameraPriority = 0; // priorytet dla nieaktywnych kamer
    public int lowerCameraPriority = 20; // priorytet dla dolnej kamery

    [Header("Lower camera")]
    public CinemachineCamera lowerCamera; // opcjonalna dolna kamera (wejście z środkowej przy S)
    private bool inLowerCamera = false;

    [Header("Cursor Follow")]
    public bool enableCursorFollow = true; // Czy śledzić kursor
    public float cursorFollowSpeed = 0.5f; // Szybkość follow (0-1, im mniejszy tym delikatniej)
    public float cursorFollowMaxOffset = 0.5f; // Maksymalne przesunięcie kamery (jednostki)

    // Przechowywanie bazowych pozycji kamer
    private Vector3[] cameraBasePositions;
    private Vector3[] cameraStartPositions; // Początkowe (true) pozycje kamer w edytorze

    // Input debounce
    private bool canSwitchCamera = true;
    private float inputCooldown = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (virtualCameras == null || virtualCameras.Length == 0)
        {
            // Próba znalezienia wszystkich aktywnych CinemachineCamera jako dzieci tego obiektu
            virtualCameras = GetComponentsInChildren<CinemachineCamera>(true); // Szuka w dzieciach, uwzględniając nieaktywne

            // Uporządkuj według pozycji w hierarchii (sibling index), żeby zapewnić kolejność: przód, lewo, tył, prawo
            virtualCameras = virtualCameras.OrderBy(c => c.transform.GetSiblingIndex()).ToArray();
        }

        if (virtualCameras == null || virtualCameras.Length != 4) // Oczekujemy dokładnie 4 kamer
        {
            Debug.LogWarning("CameraManager: Oczekiwano dokładnie 4 kamer Cinemachine jako dzieci tego obiektu (FrontView, LeftView, BackView, RightView). Przypisz je w inspektorze lub upewnij się, że są dziećmi.");
            enabled = false; // Wyłącz skrypt, jeśli nie ma 4 kamer
            return;
        }

        // Ustaw lewą kamerę (indeks 1) jako startową — bez efektu przejścia
        currentIndex = 1;
        
        // Na starcie ustaw wszystkie kamery z niskim priorytetem, a potem włącz lewą
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            virtualCameras[i].Priority = inactiveCameraPriority;
            virtualCameras[i].gameObject.SetActive(false);
        }
        
        // Aktywuj lewą bez blendu
        virtualCameras[1].Priority = activeCameraPriority;
        virtualCameras[1].gameObject.SetActive(true);

        // Przechowaj bazowe pozycje wszystkich kamer do resetowania offsetu cursora
        cameraBasePositions = new Vector3[virtualCameras.Length];
        cameraStartPositions = new Vector3[virtualCameras.Length]; // Początkowe pozycje
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            cameraStartPositions[i] = virtualCameras[i].transform.position; // Zapamiętaj początkową pozycję
            cameraBasePositions[i] = cameraStartPositions[i]; // Bazowa = początkowa
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (virtualCameras == null || virtualCameras.Length == 0) return;

        // Obsługuj przełączanie kamer, ale z ochroną przed spamem
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (canSwitchCamera)
            {
                if (inLowerCamera)
                {
                    // Z dolnej kamery: D przenosi nas na prawą kamerę (index 3)
                    SetLowerCameraActive(false);
                    currentIndex = 3;
                    ActivateCamera(currentIndex);
                    StartCoroutine(PlayTransitionEffect());
                    StartCoroutine(InputCooldown());
                }
                else
                {
                    // Ze zwykłych kamer: przesuwamy w prawo z zapętleniem
                    currentIndex = (currentIndex + 1) % virtualCameras.Length;
                    ActivateCamera(currentIndex);
                    StartCoroutine(PlayTransitionEffect());
                    StartCoroutine(InputCooldown());
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (canSwitchCamera)
            {
                if (inLowerCamera)
                {
                    // Z dolnej kamery: A przenosi nas na lewą kamerę (index 1)
                    SetLowerCameraActive(false);
                    currentIndex = 1;
                    ActivateCamera(currentIndex);
                    StartCoroutine(PlayTransitionEffect());
                    StartCoroutine(InputCooldown());
                }
                else
                {
                    // Ze zwykłych kamer: przesuwamy w lewo z zapętleniem
                    currentIndex = (currentIndex - 1 + virtualCameras.Length) % virtualCameras.Length;
                    ActivateCamera(currentIndex);
                    StartCoroutine(PlayTransitionEffect());
                    StartCoroutine(InputCooldown());
                }
            }
        }

        // Wejście/wyjście do dolnej kamery przy S, tylko gdy jesteśmy na lewej kamerze (index 1)
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (canSwitchCamera && currentIndex == 1)
            {
                if (!inLowerCamera && lowerCamera != null)
                {
                    SetLowerCameraActive(true);
                    StartCoroutine(InputCooldown());
                }
                else if (inLowerCamera)
                {
                    SetLowerCameraActive(false);
                    StartCoroutine(InputCooldown());
                }
            }
        }

        // Z dolnej kamery: W wraca do kamery centralnej (tylko z dolnej)
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (canSwitchCamera && inLowerCamera)
            {
                SetLowerCameraActive(false);
                StartCoroutine(InputCooldown());
            }
        }

        // Obsługa follow cursora
        if (enableCursorFollow && !inLowerCamera)
        {
            UpdateCursorFollow();
        }
    }

    /// <summary>
    /// Oblicza pozycję kursora i lekko przesuwają kamerę w stronę kursora (z limitem)
    /// </summary>
    private void UpdateCursorFollow()
    {
        if (virtualCameras == null || virtualCameras.Length == 0) return;

        var activeCamera = virtualCameras[currentIndex];
        if (activeCamera == null) return;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        // Pozycja kursora w viewport (0-1, gdzie 0.5, 0.5 to center)
        Vector3 mousePos = Input.mousePosition;
        Vector3 viewportPos = mainCam.ScreenToViewportPoint(mousePos);

        // Wyliczamy odchylenie od center (0.5, 0.5)
        float offsetX = (viewportPos.x - 0.5f) * 2f; // -1 do 1
        float offsetY = (viewportPos.y - 0.5f) * 2f; // -1 do 1

        // Ograniczamy do maksymalnej odległości
        offsetX = Mathf.Clamp(offsetX, -1f, 1f);
        offsetY = Mathf.Clamp(offsetY, -1f, 1f);

        // Używamy lokalnych osi kamery zamiast global
        // Right (X) i Up (Y) камery to osie na ekranie
        Vector3 cameraRight = activeCamera.transform.right;
        Vector3 cameraUp = activeCamera.transform.up;

        // Wyliczamy offset bazując na lokalnych osiach kamery
        Vector3 targetOffset = (cameraRight * offsetX + cameraUp * offsetY) * cursorFollowMaxOffset;

        // Target pozycja = bazowa pozycja + offset
        Vector3 basePos = cameraBasePositions[currentIndex];
        Vector3 targetPos = basePos + targetOffset;

        // Lerp z bazy + offsetu (nie akumulujemy!)
        Vector3 currentPos = activeCamera.transform.position;
        Vector3 newPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * cursorFollowSpeed);
        activeCamera.transform.position = newPos;
    }

    private IEnumerator InputCooldown()
    {
        canSwitchCamera = false;
        yield return new WaitForSeconds(inputCooldown);
        canSwitchCamera = true;
    }

    private void ActivateCamera(int index)
    {
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            if (virtualCameras[i] != null)
            {
                // Ustaw priorytet aktywnej kamery, reszty mają niski priorytet
                virtualCameras[i].Priority = (i == index) ? activeCameraPriority : inactiveCameraPriority;

                // Włącz/wyłącz GameObject kamery
                virtualCameras[i].gameObject.SetActive(i == index);
            }
        }

        // Resetuj kamerę do jej początkowej pozycji (bez akumulowanego offsetu)
        if (cameraStartPositions != null && index < cameraStartPositions.Length)
        {
            virtualCameras[index].transform.position = cameraStartPositions[index];
            // Resetuj bazową pozycję na początkową
            cameraBasePositions[index] = cameraStartPositions[index];
        }
    }

    private void SetLowerCameraActive(bool isActive)
    {
        if (lowerCamera == null) return;

        inLowerCamera = isActive;

        if (isActive)
        {
            // Wejście do dolnej kamery — wyłącz lewą, włącz dolną z wyższym priorytetem
            if (virtualCameras != null && virtualCameras.Length > 1 && virtualCameras[1] != null)
            {
                virtualCameras[1].Priority = inactiveCameraPriority;
                virtualCameras[1].gameObject.SetActive(false);
            }

            lowerCamera.Priority = lowerCameraPriority;
            lowerCamera.gameObject.SetActive(true);
            Debug.Log("CameraManager: Entered lower camera.");
        }
        else
        {
            // Wyjście z dolnej kamery — wyłącz dolną, przywróć lewą
            lowerCamera.Priority = inactiveCameraPriority;
            lowerCamera.gameObject.SetActive(false);

            // Przywróć lewą kamerę
            ActivateCamera(currentIndex);
            //Debug.Log("CameraManager: Exited lower camera, returned to left camera.");
        }
    }

    IEnumerator PlayTransitionEffect()
    {
        // Prosty efekt overshoot: delikatne przybliżenie (zmiana FOV) kamery głównej i powrót
        var cam = Camera.main;
        if (cam != null && fovOvershoot > 0f && fovAnimDuration > 0f)
        {
            float originalFOV = cam.fieldOfView;
            float targetFOV = Mathf.Max(1f, originalFOV - fovOvershoot);

            // Tam
            float t = 0f;
            while (t < fovAnimDuration * 0.5f)
            {
                t += Time.deltaTime;
                float p = t / (fovAnimDuration * 0.5f);
                cam.fieldOfView = Mathf.Lerp(originalFOV, targetFOV, Mathf.Sin(p * Mathf.PI * 0.5f));
                yield return null;
            }

            // Powrót (z lekkim „overshoot” odwrotnym - szybciej)
            t = 0f;
            while (t < fovAnimDuration * 0.5f)
            {
                t += Time.deltaTime;
                float p = t / (fovAnimDuration * 0.5f);
                cam.fieldOfView = Mathf.Lerp(targetFOV, originalFOV, 1f - Mathf.Cos(p * Mathf.PI * 0.5f));
                yield return null;
            }

            cam.fieldOfView = originalFOV;
        }

        yield break;
    }
}
