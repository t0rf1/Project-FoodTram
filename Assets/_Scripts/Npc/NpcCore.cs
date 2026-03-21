using System.Collections.Generic;
using UnityEngine;

public enum Produkt
{
    Kebab, Bula, Paprykaz, napoj
}
public enum SubType
{
    Kebab_klasyk, Kebab_wege, kebab_kurczak
    , napoj_kola, napoj_fanta, napoj_sok
    , none

}

[System.Serializable]
public class Zamowienie
{

    public Produkt produkt;
    public SubType subType;
}

public class NpcCore : MonoBehaviour
{
    public CustomNpcInteractions walkoutGood;
    public CustomNpcInteractions walkOutBad;
    
    public NpcMannager npcMannager;
    public float waitTime;
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

    [SerializeField] public bool waiting;
    [SerializeField] private float WaitTimer;

    public Produkt GivenProduct;

    // Queue management
    private int queueIndex = -1;

    // Visual element tracking
    private Vector3 bodyVisualStartLocalPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Cache the body visual's initial local position
        if (bodyVisual != null)
        {
            bodyVisualStartLocalPos = bodyVisual.transform.localPosition;
        }

        StartMovementToTarget(transform.position, TargetPlace, WalkInTime);
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
                walkOutBad.RunInteraction();
                /*
                waiting = false;
                finished = true; // change when added not served behavour
                //walkoutGood.RunInteraction();
                StartWalkingOutBehaviour();*/
            }

            if (GivenProduct == zamowienie[0].produkt)
            {
                waiting = false;
                walkoutGood.RunInteraction();
                
            }

        }
        else
        {
            WaitTimer = waitTime;
            waiting = true;
        }

        // Additional waiting behavior can be implemented here (e.g., animations, timers)
    }
    public void StartWalkingOutBehaviour()
    {
        StartMovementToTarget(transform.position, GoOutTarget, WalkOutTime);
    }



    private void UpdateMovement()
    {
        movementElapsedTime += Time.deltaTime;
        float normalizedTime = Mathf.Clamp01(movementElapsedTime / WalkTime);

        // Evaluate curve from 0 to 1
        float curveValue = walkEanEase.Evaluate(normalizedTime);

        // Interpolate position
        transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);

        // Apply visual effects to body
        if (bodyVisual != null)
        {
            ApplyWobble(normalizedTime);
            ApplyTilt(normalizedTime);
        }

        // Check if reached target
        if (normalizedTime >= 1f)
        {
            isMoving = false;
            isIdle = true;
            transform.position = targetPosition;
            ResetBodyVisuals();
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
}
