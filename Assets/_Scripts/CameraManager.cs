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

            // Uporządkuj według pozycji w hierarchii (sibling index), żeby zapewnić kolejność: lewo, środek, prawo
            virtualCameras = virtualCameras.OrderBy(c => c.transform.GetSiblingIndex()).ToArray();
        }

        if (virtualCameras == null || virtualCameras.Length != 3) // Oczekujemy dokładnie 3 kamer
        {
            Debug.LogWarning("CameraManager: Oczekiwano dokładnie 3 kamer Cinemachine jako dzieci tego obiektu. Przypisz je w inspektorze lub upewnij się, że są dziećmi.");
            enabled = false; // Wyłącz skrypt, jeśli nie ma 3 kamer
            return;
        }

        // Ustaw środkową kamerę (indeks 1) jako startową — bez efektu przejścia
        currentIndex = 1;
        
        // Na starcie ustaw wszystkie kamery z niskim priorytetem, a potem włącz środkową
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            virtualCameras[i].Priority = inactiveCameraPriority;
            virtualCameras[i].gameObject.SetActive(false);
        }
        
        // Aktywuj środkową bez blendu
        virtualCameras[1].Priority = activeCameraPriority;
        virtualCameras[1].gameObject.SetActive(true);
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
                    // Z dolnej kamery: D przenosi nas na prawą kamerę (index 2)
                    SetLowerCameraActive(false);
                    currentIndex = 2;
                    ActivateCamera(currentIndex);
                    StartCoroutine(PlayTransitionEffect());
                    StartCoroutine(InputCooldown());
                }
                else if (currentIndex < virtualCameras.Length - 1)
                {
                    // Ze zwykłych kamer: przesuwamy w prawo
                    currentIndex++;
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
                    // Z dolnej kamery: A przenosi nas na lewą kamerę (index 0)
                    SetLowerCameraActive(false);
                    currentIndex = 0;
                    ActivateCamera(currentIndex);
                    StartCoroutine(PlayTransitionEffect());
                    StartCoroutine(InputCooldown());
                }
                else if (currentIndex > 0)
                {
                    // Ze zwykłych kamer: przesuwamy w lewo
                    currentIndex--;
                    ActivateCamera(currentIndex);
                    StartCoroutine(PlayTransitionEffect());
                    StartCoroutine(InputCooldown());
                }
            }
        }

        // Wejście/wyjście do dolnej kamery przy S, tylko gdy jesteśmy na środkowej kamerze (index 1)
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
    }

    private void SetLowerCameraActive(bool isActive)
    {
        if (lowerCamera == null) return;

        inLowerCamera = isActive;

        if (isActive)
        {
            // Wejście do dolnej kamery — wyłącz środkową, włącz dolną z wyższym priorytetem
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
            // Wyjście z dolnej kamery — wyłącz dolną, przywróć środkową
            lowerCamera.Priority = inactiveCameraPriority;
            lowerCamera.gameObject.SetActive(false);

            // Przywróć środkową kamerę
            ActivateCamera(currentIndex);
            Debug.Log("CameraManager: Exited lower camera, returned to middle camera.");
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
