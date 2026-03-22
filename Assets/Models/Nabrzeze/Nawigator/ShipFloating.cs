using UnityEngine;

public class ShipFloating : MonoBehaviour
{
    [Header("Vertical Movement")]
    public float bobHeight = 0.5f;
    public float bobSpeed = 1f;

    [Header("Rotation")]
    public float rollAmountX = 5f;
    public float rollAmountY = 5f;
    public float rollAmountZ = 5f;
    public float rollSpeed = 1f;

    [Header("Randomness")]
    public float noiseScale = 1f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float noiseOffsetBob;
    private float noiseOffsetRollX;
    private float noiseOffsetRollY;
    private float noiseOffsetRollZ;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.localRotation;
        noiseOffsetBob = Random.Range(0f, 100f);
        noiseOffsetRollX = Random.Range(0f, 100f);
        noiseOffsetRollY = Random.Range(0f, 100f);
        noiseOffsetRollZ = Random.Range(0f, 100f);
    }

    void Update()
    {
        float bobNoise = Mathf.PerlinNoise(noiseOffsetBob, Time.time * bobSpeed * noiseScale);
        float bobOffset = Mathf.Lerp(-bobHeight, bobHeight, bobNoise);
        transform.position = startPosition + Vector3.up * bobOffset;

        float rollX = Mathf.Lerp(-rollAmountX, rollAmountX, Mathf.PerlinNoise(noiseOffsetRollX, Time.time * rollSpeed * noiseScale));
        float rollY = Mathf.Lerp(-rollAmountY, rollAmountY, Mathf.PerlinNoise(noiseOffsetRollY, Time.time * rollSpeed * noiseScale));
        float rollZ = Mathf.Lerp(-rollAmountZ, rollAmountZ, Mathf.PerlinNoise(noiseOffsetRollZ, Time.time * rollSpeed * noiseScale));

        transform.localRotation = startRotation * Quaternion.Euler(rollX, rollY, rollZ);
    }
}