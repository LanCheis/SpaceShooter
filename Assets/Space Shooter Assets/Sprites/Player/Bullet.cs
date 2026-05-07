using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
   public float speed = 20f;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Thay linearVelocity bằng velocity nếu cái kia báo lỗi đỏ
            rb.linearVelocity = transform.up * speed;
        }
        
        Destroy(gameObject, 3f);
    }
}
