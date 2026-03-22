using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// System zarządzania korkami — monitoruje ilość włączonych urządzeń
/// Jeśli więcej niż 3 urządzenia są włączone, korki się przewalają
/// Uwaga: Światło (LightController) zajmuje 1 slot na energię
/// </summary>
public class Korki : MonoBehaviour, I_Interactable
{
    [SerializeField] private int maxDevicesLimit = 3; // Maksymalna liczba urządzeń które mogą pracować (wliczając światło)
    
    private List<object> activeDevices = new List<object>(); // Włączone urządzenia (mogą być ElectronicDevice lub LightDevice)
    private bool korkiTriggered = false; // Czy korki są przewalone

    /// <summary>
    /// Rejestruje włączone urządzenie (ElectronicDevice lub LightDevice)
    /// </summary>
    public void RegisterDevice(object device)
    {
        if (!activeDevices.Contains(device))
        {
            activeDevices.Add(device);
            Debug.Log($"Korki: Urządzenie włączone. Aktywnych urządzeń: {activeDevices.Count}/{maxDevicesLimit}");

            // Sprawdzamy czy przewalimy korki
            CheckIfKorkiTriggered();
        }
    }

    /// <summary>
    /// Usuwa urządzenie z listy włączonych
    /// </summary>
    public void UnregisterDevice(object device)
    {
        if (activeDevices.Contains(device))
        {
            activeDevices.Remove(device);
            Debug.Log($"Korki: Urządzenie wyłączone. Aktywnych urządzeń: {activeDevices.Count}/{maxDevicesLimit}");
        }
    }

    /// <summary>
    /// Sprawdza czy przekroczono limit urządzeń — jeśli tak, przewala korki
    /// </summary>
    private void CheckIfKorkiTriggered()
    {
        if (activeDevices.Count > maxDevicesLimit)
        {
            TrigerKorki();
        }
    }

    /// <summary>
    /// Przewala wszystkie korki — wyłącza wszystkie urządzenia
    /// </summary>
    private void TrigerKorki()
    {
        if (korkiTriggered)
            return; // Już przewalone

        korkiTriggered = true;
        Debug.Log($"⚡ KORKI PRZEWALONE! Włączonych urządzeń: {activeDevices.Count} (limit: {maxDevicesLimit})");

        // Wyłączamy wszystkie urządzenia
        foreach (object device in activeDevices)
        {
            if (device is ElectronicDevice electronicDevice)
            {
                electronicDevice.SetEnabled(false);
            }
            else if (device is LightDevice lightDevice)
            {
                lightDevice.SetEnabled(false);
            }
        }
        activeDevices.Clear();
    }

    /// <summary>
    /// Zwraca czy korki są przewalone
    /// </summary>
    public bool AreKorkiTriggered()
    {
        return korkiTriggered;
    }

    /// <summary>
    /// Zwraca ilość aktywnych urządzeń
    /// </summary>
    public int GetActiveDeviceCount()
    {
        return activeDevices.Count;
    }

    /// <summary>
    /// Włącza korki ponownie — pozwala urządzeniom pracować (kliknięcie na Korki)
    /// </summary>
    public void Interact()
    {
        if (!korkiTriggered)
        {
            return;
        }

        korkiTriggered = false;
        Debug.Log("✅ Korki włączone ponownie! Urządzenia mogą pracować.");
    }
}
