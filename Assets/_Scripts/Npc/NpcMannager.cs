using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QueuePlace
{
    public Transform SpawnPoint;
    public Transform place;
    public Transform WalkOutTarget;
    public bool Ocupied = false;
}

public class NpcMannager : MonoBehaviour
{
    public bool queueOpen = true;
    public NpcSpawner npcSpawner;
    [SerializeField] public List<QueuePlace> queuePlaces;


    public GameObject NpcToSpawdn;

    public bool TrySpawnNpc(GameObject npcPrefab)
    {
        int queueIndex = GetUnnocupiedQueuePlace();
        if (queueIndex != -1)
        {
            queuePlaces[queueIndex].Ocupied = true;
            GameObject npc = Instantiate(npcPrefab, queuePlaces[queueIndex].SpawnPoint);
            npc.GetComponent<NpcCore>().SetQueueIndex(queueIndex);
            npc.GetComponent<NpcCore>().SetTargetPlace(queuePlaces[queueIndex].place.position);
            npc.GetComponent<NpcCore>().SetGoOutPlace(queuePlaces[queueIndex].WalkOutTarget.position);
            npc.GetComponent<NpcCore>().SetNpcManager(this);
            return true;
        }
        return false;
    }

    public void FreeQueueSlot(int index)
    {
        if (index >= 0 && index < queuePlaces.Count)
        {
            queuePlaces[index].Ocupied = false;
            queueOpen = true;
            Debug.Log($"Queue slot {index} freed");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetUnnocupiedQueuePlace();
    }

    public int GetUnnocupiedQueuePlace()
    {
        int index = -1;
        foreach (var queuePlace in queuePlaces)
        {
            if (!queuePlace.Ocupied)
            {
                index = queuePlaces.IndexOf(queuePlace);
            }
        }
        if (index == -1)
        {
            queueOpen = false;
        }
        else
        {
            queueOpen = true;
        }
        Debug.Log("index: " + index);
        return index;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
