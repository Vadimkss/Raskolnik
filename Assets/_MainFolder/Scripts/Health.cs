using System.Collections;
using UnityEngine;
using DamageNumbersPro;

public class Health : MonoBehaviour, IDamageable
{
    public float health = 100f;
    public DamageNumber damageTextPrefab;
    public Transform PopUpPlace;

    public void TakeDamage(float amount, DamageHandler.AttackType attackType)
    {
        health -= amount;

        ShowDamageText(amount);

        if (health <= 0f)
        {
            Die();
        }
    }

    private void ShowDamageText(float amount)
    {
        if (damageTextPrefab != null && PopUpPlace != null)
        {
            DamageNumber damageTextInstance = damageTextPrefab.Spawn(PopUpPlace.position, amount);
            damageTextInstance.transform.position += Vector3.up;
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}