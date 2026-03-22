using UnityEngine;
using System.Collections.Generic;
public class AngryGuyWalkForward : MonoBehaviour, I_Interactable
{
     public HoldedItem heldItem;
    public List<GameObject> BodyVisuals;
    public Transform GoToTarget;
    public float WalkSpeed = 2f;
    public Transform GoOutwaypointOne;
    public Transform GoOutwaypointTwo;
    public float WaitDuration = 2f;
    public float StoppingDistance = 0.1f;
    
    private enum MovementState
    {
        ToTarget,
        Waiting,
        ToWaypointOne,
        ToWaypointTwo
    }
    
    public bool WalkBack = false;
    int bodyvisualIndex = -1;
    private MovementState currentState = MovementState.ToWaypointOne;
    private float waitTimer = 0f;
    private bool isReturning = false;
    private bool hasTriggeredReturn = false;
    public void GoToNextBodyVisual()
    {
        foreach (var visual in BodyVisuals)
        {
            visual.SetActive(false);
        }

        if (bodyvisualIndex < BodyVisuals.Count - 1)
        {
            bodyvisualIndex++;
        }
        BodyVisuals[bodyvisualIndex].SetActive(true);


    }

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GoToNextBodyVisual();
    }

    // Update is called once per frame
    void Update()
    {
         if (heldItem != null && heldItem.itemType == FoodPartType.ReadyFood)
            {
                WalkBack = true;

            }

        // WalkBack takes priority - wait and then go to waypoints
        if (WalkBack && !hasTriggeredReturn)
        {
            currentState = MovementState.Waiting;
            waitTimer = 0f;
            isReturning = true;
            hasTriggeredReturn = true;
            GoToNextBodyVisual();
        }
        
        switch(currentState)
        {
            case MovementState.ToWaypointOne:
                MoveTowards(GoOutwaypointOne.position);
                if (Vector3.Distance(transform.position, GoOutwaypointOne.position) < StoppingDistance)
                {
                    if (isReturning)
                    {
                        currentState = MovementState.ToWaypointTwo;
                    }
                    else
                    {
                        currentState = MovementState.ToTarget;
                    }
                }
                break;
            
            case MovementState.ToTarget:
                MoveTowards(GoToTarget.position);
                if (Vector3.Distance(transform.position, GoToTarget.position) < StoppingDistance)
                {
                    currentState = MovementState.Waiting;
                    waitTimer = 0f;
                    isReturning = true;
                }
                break;
                
            case MovementState.Waiting:
                waitTimer += Time.deltaTime;
                if (waitTimer >= WaitDuration)
                {
                    currentState = MovementState.ToWaypointOne;
                }
                break;
                
            case MovementState.ToWaypointTwo:
                MoveTowards(GoOutwaypointTwo.position);
                if (Vector3.Distance(transform.position, GoOutwaypointTwo.position) < StoppingDistance)
                {
                    // Reached final waypoint
                    Destroy(gameObject);hasTriggeredReturn = false;
                }
                break;
        }
    }
    
    void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * WalkSpeed * Time.deltaTime;
    }

public void Interact()
    {
        if (CursorSettings.Instance.heldItem != null)
        {

            heldItem = CursorSettings.Instance.heldItem;
            Debug.Log("Grabbed Item: " + heldItem.skladnikType);
            Debug.Log("Dano: item " + CursorSettings.Instance.heldItem.gameObject.name);
            CursorSettings.Instance.UpdateHeldItemTexture(); // Aktualizacja tekstury kursora
            CursorSettings.Instance.heldItem = null; // Czyszczenie trzymanego przedmiotu



        }
        else
        {
            Debug.Log("Kosz: Nie trzymasz żadnego przedmiotu do wyrzucenia!");
        }

    }

}
