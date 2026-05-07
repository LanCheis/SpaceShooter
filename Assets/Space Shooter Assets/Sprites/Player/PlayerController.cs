using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum WeaponType { Neutron, Laser }

    [Header("Di chuyển")]
    public float moveSpeed = 12f;
    
    [Header("Cấu hình vũ khí")]
    public WeaponType currentWeapon = WeaponType.Neutron;
    [Range(1, 3)] public int weaponLevel = 1; 
    public GameObject bulletPrefab;
    public Transform firePointCenter; // Điểm duy nhất ở đầu máy bay
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    [Header("Màu sắc đạn")]
    public Color neutronColor = Color.green;
    public Color laserColor = Color.red;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Camera mainCam;
    [Header("Hiệu ứng 6 động cơ")]
// Bạn sẽ kéo 6 cái Effect vào danh sách này trong Unity
public ParticleSystem[] engines;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        nextFireTime = 0f; // Reset thời gian bắn về 0
        rb.gravityScale = 0;
        rb.linearDamping = 0;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
    }

    void Update()
{
    moveInput.x = Input.GetAxisRaw("Horizontal");
    moveInput.y = Input.GetAxisRaw("Vertical");
    // Kiểm tra xem máy bay có đang di chuyển không
    bool isMoving = moveInput.magnitude > 0.1f;

    foreach (ParticleSystem ps in engines)
    {
        if (ps != null)
        {
            var main = ps.main;
            // Nếu di chuyển (isMoving = true) thì size là 1.5, nếu đứng im thì size 0.7
            main.startSize = isMoving ? 1.5f : 0.7f;
        }
    }
    // SỬA LẠI ĐOẠN NÀY: 
    // Dùng Input.GetKey để nhấn giữ là bắn liên tục
    if (Input.GetKey(KeyCode.J)) 
    {
        if (Time.time >= nextFireTime) 
        {
            Shoot();
            // Đảm bảo fireRate không bằng 0. Ví dụ 0.2f
            nextFireTime = Time.time + fireRate; 
        }
    }

    transform.rotation = Quaternion.identity;
}

    void Shoot()
    {
        if (bulletPrefab == null || firePointCenter == null) return;

        Color bulletColor = (currentWeapon == WeaponType.Neutron) ? neutronColor : laserColor;

        // Cơ chế bắn theo Level giống Chicken Invaders
        if (weaponLevel == 1)
        {
            // Level 1: 1 viên ở giữa
            CreateBullet(Vector3.zero, 0, bulletColor);
        }
        else if (weaponLevel == 2)
        {
            // Level 2: 2 viên song song
            CreateBullet(new Vector3(-0.2f, 0, 0), 0, bulletColor);
            CreateBullet(new Vector3(0.2f, 0, 0), 0, bulletColor);
        }
        else if (weaponLevel == 3)
        {
            // Level 3: 3 viên tỏa ra (1 thẳng, 2 nghiêng)
            CreateBullet(Vector3.zero, 0, bulletColor);
            CreateBullet(Vector3.zero, 15f, bulletColor);  // Nghiêng trái
            CreateBullet(Vector3.zero, -15f, bulletColor); // Nghiêng phải
        }
    }

    void CreateBullet(Vector3 offset, float zRotation, Color color)
{
    GameObject bullet = Instantiate(bulletPrefab, firePointCenter.position + offset, Quaternion.Euler(0, 0, zRotation));
    SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();
    
    if (sr != null) 
    {
        sr.color = color;

        // TỰ ĐỘNG BIẾN ĐỔI HÌNH DÁNG TỪ 1 ASSET GỐC
        if (currentWeapon == WeaponType.Neutron)
        {
            // Neutron: Làm cho đạn lùn lại (Y nhỏ) và rộng ra (X to) để giống hình tròn/hạt
            bullet.transform.localScale = new Vector3(1.5f, 0.5f, 1f);
        }
        else if (currentWeapon == WeaponType.Laser)
        {
            // Laser: Làm cho đạn rất dài (Y to) và mỏng lại (X nhỏ)
            bullet.transform.localScale = new Vector3(0.4f, 2.5f, 1f);
        }
    }
}

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed;
        Vector3 viewPos = mainCam.WorldToViewportPoint(transform.position);
        viewPos.x = Mathf.Clamp(viewPos.x, 0.05f, 0.95f);
        viewPos.y = Mathf.Clamp(viewPos.y, 0.05f, 0.95f);
        transform.position = mainCam.ViewportToWorldPoint(viewPos);
    }

    // Hàm hỗ trợ ăn vật phẩm để nâng cấp
    public void UpgradeWeapon(WeaponType newType)
    {
        if (currentWeapon == newType)
        {
            if (weaponLevel < 3) weaponLevel++;
        }
        else
        {
            currentWeapon = newType;
            weaponLevel = 1;
        }
    }
}
