using UnityEngine;

public class PatusJumpSCARE : CustomNpcInteractions
{
    public GameObject JumpScare;
    

    public override void RunInteraction()
    {
        GameObject patusPoint = GameObject.FindGameObjectWithTag("PatusPoint");

        Instantiate(JumpScare, patusPoint.transform.position, Quaternion.identity);
        Debug.Log("Patus Jump Scare Triggered");
        npcCore.waiting = false;
        npcCore.finished = true;
        npcCore.StartMovementToTarget(transform.position, npcCore.GoOutTarget, npcCore.WalkOutTime);
        
    }
}
