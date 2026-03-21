using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class NpcSpawnQueue
{
    public GameObject npcPrefab;
    public float spawnIn;
}
public class NpcSpawner : MonoBehaviour
{
    [SerializeField] private NpcMannager npcManager;
    [SerializeField] public List<NpcSpawnQueue> NpcQueue;

    private bool ReadyToSpawn = false;
    private bool queuing = false;
    public float spawntimer;
    public void SpawnNext()
    {

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!ReadyToSpawn)
        {
           
           if(NpcQueue.Count > 0)
            {
            
            if (!queuing)
            {
                queuing = true;
                spawntimer = NpcQueue[0].spawnIn;
            }
            else
            {
                if (spawntimer > 0)
                {
                    spawntimer -= Time.deltaTime;
                }
                else
                {
                    ReadyToSpawn = true;
                    queuing = false;
                }
            }
            }
            
        }
        else
        {

            if (NpcQueue.Count == 0)
            {

                Debug.Log("no more npcs to spawn");
            }
            else
            {
                AttemptSpawn();

            }
        }
    }


    public void AttemptSpawn()
    {
        if (npcManager == null)
        {
            Debug.LogError("NpcManager not assigned!");
            return;
        }

        if (npcManager.queueOpen)
        {
            if (npcManager.TrySpawnNpc(NpcQueue[0].npcPrefab))
            {
                Debug.Log("NPC spawned successfully");
                ReadyToSpawn = false;
                NpcQueue.RemoveAt(0);
            }
            else
            {
                Debug.Log("Queue is full, waiting...");
                //ReadyToSpawn = false;
            }
        }
        else
        {
            Debug.Log("Queue closed, waiting for availability...");
            //ReadyToSpawn = false;
        }
    }
}
