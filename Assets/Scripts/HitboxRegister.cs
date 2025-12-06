using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxRegister : MonoBehaviour
{
    private float damageAmount;
    // List to ensure one enemy is only hit once per swing
    private List<GameObject> objectsHitThisSwing;

    // Called by PlayerController at the start of the attack
    public void Initialize(float damage)
    {
        damageAmount = damage;
        // Reset the list for a new attack
        objectsHitThisSwing = new List<GameObject>();
    }

    // This runs when the hitbox (which must be a Trigger) overlaps another collider
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == gameObject.layer)
        {
            return;
        }

        if (objectsHitThisSwing.Contains(other.gameObject))
        {
            return;
        }

        Damageable damageable = other.GetComponent<Damageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damageAmount);
            objectsHitThisSwing.Add(other.gameObject);
            Debug.Log("This Thang is Hit!");
        }
    }
}
