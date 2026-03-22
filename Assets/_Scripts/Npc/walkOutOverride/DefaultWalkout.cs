using UnityEngine;

public class DefaultWalkout : CustomNpcInteractions
{

    public override void RunInteraction()
    {
        npcCore.waiting = false;
        npcCore.finished = true;
        npcCore.StartMovementToTarget(transform.position, npcCore.GoOutTarget, npcCore.WalkOutTime);
    }
}
