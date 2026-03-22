using UnityEngine;

public class ZulAction : CustomNpcInteractions
{
    public float despawnTime = 2f;
    public float panSpeed = 2f;
    Cover coverDetect;
    public GameObject JumpScare;
    private bool isCoverClosed = false;
    private float despawnTimer = 0f;

    void Start()
    {
        coverDetect = GameObject.FindGameObjectWithTag("CoverDetect").GetComponent<Cover>();
        
    }

    void Update()
    {
        // Slowly pan forward
        
            gameObject.transform.Translate(Vector3.forward*-1 * panSpeed * Time.deltaTime);
        

        // Check if cover is closed
        if (coverDetect != null && coverDetect.isCovered && !isCoverClosed)
        {
            Debug.Log("Closed i got");
            isCoverClosed = true;
            despawnTimer = 0f;
        }

        // If cover is closed, start despawn timer
        if (isCoverClosed)
        {
            despawnTimer += Time.deltaTime;
            if (despawnTimer >= despawnTime)
            {
                Destroy(gameObject);
                
            }
        }
    }

    public override void RunInteraction()
    {
        Debug.Log("ITS ZUL TIME");
    }
}
