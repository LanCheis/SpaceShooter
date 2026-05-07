using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public PlayerController.WeaponType weaponType;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.UpgradeWeapon(weaponType);
            }
            Destroy(gameObject); // Biến mất sau khi ăn
        }
    }
}
