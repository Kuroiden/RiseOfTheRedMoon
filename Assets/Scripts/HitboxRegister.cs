using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxRegister : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private float damageAmount;
    // List to ensure one enemy is only hit once per swing
    public List<GameObject> objectsHitThisSwing;

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
        if (other.CompareTag("Player") && other.gameObject != player /*|| other.gameObject.layer == gameObject.layer*/)
        {
            if (!objectsHitThisSwing.Contains(other.gameObject)) objectsHitThisSwing.Add(other.gameObject);

            foreach (GameObject x in objectsHitThisSwing)
            {
                x.GetComponent<PlayerController>().TakeDamage(damageAmount);
            }

            Debug.Log("This Thang is Hit!");
        }

        //if (objectsHitThisSwing.Contains(other.gameObject))
        //{
        //    return;
        //}

        //Damageable damageable = other.GetComponent<Damageable>();

        //if (damageable != null)
        //{
        //    damageable.TakeDamage(damageAmount);
            
        //}
    }
}
