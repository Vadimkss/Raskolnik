using UnityEngine;

public class DamageHandler : MonoBehaviour
{
    public enum AttackType { Grave, Arms, Katanas, Curse, Other }

    private IDamageable damageableComponent;
 

    private void Awake()
    {
        damageableComponent = GetComponent<IDamageable>();
      
    }

    public void ApplyDamage(float amount, AttackType attackType)
    {
        if (damageableComponent == null)
            return;

        damageableComponent.TakeDamage(amount, attackType);
    }

   
}