using UnityEngine;

public class BabciaJumScare : CustomNpcInteractions
{
    public GameObject JumpScare;

    public override void RunInteraction()
    {
        Instantiate(JumpScare, transform.position, Quaternion.identity);
        npcCore.GoToNextBodyVisual();
      
    }
}
