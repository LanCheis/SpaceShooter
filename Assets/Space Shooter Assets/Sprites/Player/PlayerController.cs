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

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
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
        rb.linearVelocity = moveInput.normalized * moveSpeed;
        Vector3 viewPos = mainCam.WorldToViewportPoint(transform.position);
        viewPos.x = Mathf.Clamp(viewPos.x, 0.05f, 0.95f);
        viewPos.y = Mathf.Clamp(viewPos.y, 0.05f, 0.95f);
        transform.position = mainCam.ViewportToWorldPoint(viewPos);
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
