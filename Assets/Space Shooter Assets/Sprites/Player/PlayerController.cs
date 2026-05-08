using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 12f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePointCenter;
    [Tooltip("Shots per second")]
    public float attackSpeed = 5f;
    private float nextFireTime = 0f;

    [Header("Engines")]
    public ParticleSystem[] engines;

    [Header("World Bounds")]
    public Vector2 minBounds = new Vector2(-8f, -4.5f);
    public Vector2 maxBounds = new Vector2(8f, 4.5f);

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearDamping = 0;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        float right = Mathf.Max(0f,  moveInput.x);
        float left  = Mathf.Max(0f, -moveInput.x);
        float up    = Mathf.Max(0f,  moveInput.y);
        float down  = Mathf.Max(0f, -moveInput.y);

        SetEngine(0, Mathf.Max(right, down));
        SetEngine(1, right);
        SetEngine(2, Mathf.Max(right, up));
        SetEngine(3, Mathf.Max(left,  down));
        SetEngine(4, left);
        SetEngine(5, Mathf.Max(left,  up));

        if (Input.GetKey(KeyCode.K) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + (1f / attackSpeed);
        }

        transform.rotation = Quaternion.identity;
    }

    void FixedUpdate()
    {
        Vector2 desiredVelocity = moveInput.normalized * moveSpeed;
        Vector2 nextPos = rb.position + desiredVelocity * Time.fixedDeltaTime;
        nextPos.x = Mathf.Clamp(nextPos.x, minBounds.x, maxBounds.x);
        nextPos.y = Mathf.Clamp(nextPos.y, minBounds.y, maxBounds.y);
        rb.linearVelocity = (nextPos - rb.position) / Time.fixedDeltaTime;
    }

    Vector3 GetFirePosition()
    {
        if (firePointCenter != null) return firePointCenter.position;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float topY = sr != null ? sr.bounds.extents.y : 0.5f;
        return transform.position + new Vector3(0f, topY, 0f);
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;
        Instantiate(bulletPrefab, GetFirePosition(), Quaternion.identity);
    }

    void SetEngine(int index, float activation)
    {
        if (index >= engines.Length || engines[index] == null) return;
        var main = engines[index].main;
        main.startSize = Mathf.Lerp(0.3f, 1.8f, activation);
    }
}
