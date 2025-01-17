using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MacrophageConsume : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;

    [HideInInspector] public float attackDamage;
    [HideInInspector] public float attackRange;
    [HideInInspector] public float attackSize;
    [HideInInspector] public float critRate;
    [HideInInspector] public float critDMG;
    [HideInInspector] public float knockbackPower;
    [HideInInspector] public float dot;
    [HideInInspector] public float duration;
    [HideInInspector] public int tickRate;

    // Start is called before the first frame update
    private void OnEnable()
    {
        StartCoroutine(Consume());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator Consume()
    {
        yield return new WaitForSeconds(1f);

        var hits = Physics.OverlapSphere(transform.position, attackSize, layerMask);

        //float damage = DamageCalculator.CalcDamage(attackDamage, critRate, critDMG);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Enemy>(out Enemy enemy))
            {
                float armor = enemy.attributes.GetAttribute("Armor").Value;
                //enemy.TakeDamage(damage);
                DamageCalculator.ApplyDamage(attackDamage, critRate, critDMG, armor, enemy);
                enemy.ApplyDoT(dot, duration, tickRate);

                // Pull effect
                if (enemy.TryGetComponent<ImpactReceiver>(out ImpactReceiver impactReceiver))
                {
                    Vector3 dir = Vector3.Normalize(enemy.transform.position - transform.position);
                    impactReceiver.AddImpact(dir, -knockbackPower); 
                }
            }
        }

        Destroy(this.gameObject);
    }
}
