using UnityEngine;
using UnityEngine.EventSystems;

public abstract class CustomNpcInteractions : MonoBehaviour
{
    public NpcCore npcCore;
    void Start()
    {
        npcCore = GetComponent<NpcCore>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public abstract void RunInteraction();

}
