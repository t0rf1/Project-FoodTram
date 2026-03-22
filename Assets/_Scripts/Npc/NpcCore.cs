using System.Collections.Generic;
using UnityEngine;




[System.Serializable]
public class Zamowienie
{

    public FoodPartType produkt;
    [ShowIf(nameof(produkt), (int)FoodPartType.Skladnik)]
    public SkladnikType skladnikType;
}

public class NpcCore : MonoBehaviour, I_Interactable
{
    public List<GameObject> BodyVisuals;

    public CustomNpcInteractions walkoutGood;
    public CustomNpcInteractions walkOutBad;

    public CustomNpcInteractions WaitAction;

    public NpcMannager npcMannager;
    public float AngryMetter;
    public float WalkTime;
    public float WalkInTime;
    public float WalkOutTime;
    public float incidentTimeMin;
    public float incidentTimeMax;
    public AnimationCurve walkEanEase;
    public AnimationCurve Wooble;
    public AnimationCurve motionFadeOut; // Controls fade-out of all motion (wobble + tilt) near end

    public Vector3 TargetPlace;
    public Vector3 GoOutTarget;
    public GameObject bodyVisual; // Secondary object for wobble and tilt

    [SerializeField]
    public List<Zamowienie> zamowienie;

    public HoldedItem heldItem;

    // Wobble parameters
    public float wobbleAmplitude = 0.3f;
    public float wobbleFrequency = 4f;

    // Tilt parameters
    public float maxTiltAngle = 15f;

    // Movement tracking
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float movementElapsedTime;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isIdle;
    [SerializeField] public bool finished;
    [SerializeField] private bool isWalkingOut;

    [SerializeField] public bool waiting;
    [SerializeField] private float WaitTimer;

    [SerializeField] private int bodyvisualIndex = -1;

    //public FoodPartType GivenProduct;

    // Queue management
    private int queueIndex = -1;

    // Visual element tracking
    private Vector3 bodyVisualStartLocalPos;
    bool updatedVisuals = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GoToNextBodyVisual();
        // Cache the body visual's initial local position
        if (bodyVisual != null)
        {
            bodyVisualStartLocalPos = bodyVisual.transform.localPosition;
        }

        StartMovementToTarget(transform.position, TargetPlace, WalkInTime);
    }

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
    public void SetNpcManager(NpcMannager manager)
    {
        npcMannager = manager;
    }
    public void SetTargetPlace(Vector3 target)
    {
        TargetPlace = target;
    }
    public void SetGoOutPlace(Vector3 target)
    {
        GoOutTarget = target;
    }
    public void SetQueueIndex(int index)
    {
        queueIndex = index;
    }

    private void OnDestroy()
    {
        // Ensure queue slot is freed if NPC is destroyed
        FreeQueueSlot();
    }

    private void FreeQueueSlot()
    {
        if (npcMannager != null && queueIndex >= 0)
        {
            npcMannager.FreeQueueSlot(queueIndex);
        }
    }

    public void FreeQueueSlotAndDestroy()
    {
        FreeQueueSlot();
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            UpdateMovement();
        }
        else if (isIdle)
        {
            if (finished)
            {
                Debug.Log("Leavee");
                FreeQueueSlotAndDestroy();
            }
            HandleWaitingBehaviour();
            // Here you can implement waiting behavior or other logic when not moving
        }
    }

    public void StartMovementToTarget(Vector3 startPos, Vector3 endPos, float wTime)
    {
        if (TargetPlace == null)
        {
            Debug.LogWarning("TargetPlace not assigned!");
            return;
        }
        WalkTime = wTime;
        startPosition = startPos;
        targetPosition = endPos;
        movementElapsedTime = 0f;
        isMoving = true;
    }

    public void HandleWaitingBehaviour()
    {
        if (waiting)
        {
            WaitTimer -= Time.deltaTime;


            if (WaitTimer <= 0f)
            {
                isWalkingOut = true;
                walkOutBad.RunInteraction();
                /*
                waiting = false;
                finished = true; // change when added not served behavour
                //walkoutGood.RunInteraction();
                StartWalkingOutBehaviour();*/
            }
            else if (WaitTimer < AngryMetter / 2f && !updatedVisuals)
            {
                Debug.Log("NPC is getting angrier! (Halfway there)");
                GoToNextBodyVisual();
                updatedVisuals = true; // Reset visuals update flag to allow for "angry" feedback again if timer is extended
            }

            if(WaitAction!= null)
            {
                WaitAction.RunInteraction();
            }
            
            // Here you can add visual feedback for the NPC getting angrier (e.g., change color, play animation)
            // This is just a placeholder for demonstration
            


            if (heldItem != null && heldItem.itemType == zamowienie[0].produkt)
            {
                waiting = false;
                isWalkingOut = true;
                walkoutGood.RunInteraction();

            }

        }
        else
        {
            WaitTimer = AngryMetter;
            waiting = true;
        }

        // Additional waiting behavior can be implemented here (e.g., animations, timers)
    }
    public void StartWalkingOutBehaviour()
    {
        isWalkingOut = true;
        StartMovementToTarget(transform.position, GoOutTarget, WalkOutTime);
    }



    private void UpdateMovement()
    {
        movementElapsedTime += Time.deltaTime;
        float normalizedTime = Mathf.Clamp01(movementElapsedTime / WalkTime);

        if (isWalkingOut)
        {
            // Use inverted animation curves for walk-out
            float invertedTime = 1f - normalizedTime;
            float curveValue = walkEanEase.Evaluate(invertedTime);
            transform.position = Vector3.Lerp(startPosition, targetPosition, normalizedTime);

            // Apply visual effects with inverted curves
            if (bodyVisual != null)
            {
                ApplyWobble(invertedTime);
                ApplyTilt(invertedTime);
            }
        }
        else
        {
            // Normal walk-in with easing and effects
            float curveValue = walkEanEase.Evaluate(normalizedTime);
            transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);

            // Apply visual effects to body
            if (bodyVisual != null)
            {
                ApplyWobble(normalizedTime);
                ApplyTilt(normalizedTime);
            }
        }

        // Check if reached target
        if (normalizedTime >= 1f)
        {
            isMoving = false;
            isIdle = true;
            transform.position = targetPosition;
            ResetBodyVisuals();
            isWalkingOut = false;
        }
    }

    private void ApplyWobble(float normalizedTime)
    {
        float wobbleAmount = Mathf.Sin(normalizedTime * wobbleFrequency * Mathf.PI * 2f) * wobbleAmplitude;
        float motionFade = motionFadeOut.Evaluate(normalizedTime);
        wobbleAmount *= motionFade;

        Vector3 newLocalPos = bodyVisualStartLocalPos;
        newLocalPos.y += wobbleAmount;
        bodyVisual.transform.localPosition = newLocalPos;
    }

    private void ApplyTilt(float normalizedTime)
    {
        float tiltCurveValue = Wooble.Evaluate(normalizedTime);
        float tiltAngle = Mathf.Lerp(-maxTiltAngle, maxTiltAngle, tiltCurveValue);
        float motionFade = motionFadeOut.Evaluate(normalizedTime);
        tiltAngle *= motionFade;

        bodyVisual.transform.localRotation = Quaternion.Euler(0f, 0f, tiltAngle);
    }

    private void ResetBodyVisuals()
    {
        // Reset wobble and tilt
        bodyVisual.transform.localPosition = bodyVisualStartLocalPos;
        bodyVisual.transform.localRotation = Quaternion.identity;
    }

    public bool ReachPlace()
    {
        return !isMoving && Vector3.Distance(transform.position, targetPosition) < 0.01f;
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
