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
            Instantiate(JumpScare, ThrowCat.position, Quaternion.identity);
            spawnOne = true;
        }
        npcCore.GoToNextBodyVisual();
      
    }
}
