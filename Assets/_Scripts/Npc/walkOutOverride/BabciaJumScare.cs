using UnityEngine;

public class BabciaJumScare : CustomNpcInteractions
{
    public GameObject JumpScare;
    public Transform ThrowCat;
    bool spawnOne = false;
    public override void RunInteraction()
    {
        if(!spawnOne)
        {
            Transform g = GameObject.FindGameObjectWithTag("CatForcedPosition").transform;
           
            Instantiate(JumpScare, g.position, Quaternion.identity);
            spawnOne = true;
        }
        npcCore.GoToNextBodyVisual();
      
    }
}
