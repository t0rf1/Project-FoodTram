using UnityEngine;

public class WalkOutBad : CustomNpcInteractions
{
    public GameObject JumpScare;

    public override void RunInteraction()
    {
        Instantiate(JumpScare, transform.position, Quaternion.identity);
        npcCore.FreeQueueSlotAndDestroy();
    }
}
