using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Neutrophil_Grenade", menuName = "Ability System/Abilities/Neutrophil Grenade")]
public class Neutrophil_Grenade : Ability
{
    public override AbilitySpec CreateSpec(AbilitySystem owner)
    {
        return new Neutrophil_GrenadeSpec(this, owner);
    }
}

public class Neutrophil_GrenadeSpec : AbilitySpec
{
    #region Attributes
    public AttributeSet attributes;

    public Attribute level;
    public Attribute attackDamage;
    public Attribute attackSpeed;
    public Attribute attackRange;
    public Attribute attackSize;
    public Attribute attackCount;
    public Attribute critRate;
    public Attribute critDMG;

    public Attribute Type_1_DMG_Bonus;
    public Attribute Type_2_DMG_Bonus;
    public Attribute Type_3_DMG_Bonus;

    #endregion Attributes

    private Neutrophil_Grenade basicAttack;

    private ObjectPool grenades;

    public Neutrophil_GrenadeSpec(Neutrophil_Grenade ability, AbilitySystem owner) : base(ability, owner)
    {
        Init();
        owner.StartCoroutine(TryActivateAbility());
    }

    public bool IsAttacking { get; private set; } = false;

    public override bool CanActivateAbility()
    {
        return base.CanActivateAbility() && !IsAttacking;
    }

    public override IEnumerator ActivateAbility()
    {
        while (true)
        {
            yield return new WaitUntil(() => owner.GetComponent<AbilitySet>().CanUseBasicAttack);
            IsAttacking = true;

            // Wait before shooting
            yield return new WaitForSeconds(2f / attackSpeed.Value);

            if (owner.GetComponent<AbilitySet>().CanUseBasicAttack)
                yield return Shoot();
        }
    }

    private IEnumerator Shoot()
    {
        WaitForSeconds wait = new(0.25f);

                                         // Level 2 and higher: Increase DMG by 10
        float AD = attackDamage.Value + (abilityLevel >= 2 ? 10f : 0f);
        // Increase size per level
        float AZ = attackSize.Value + ((abilityLevel - 1) * 1.05f);
        float CRIT_RATE = critRate.Value;
        float CRIT_DMG = critDMG.Value;
                                // Level 3 and higher: Increase slow amount by 40%
        float SLOW = -0.3f + (abilityLevel >= 3 ? -0.4f : 0f);
                                // Level 4 and higher: Increase slow field duration by 2s
        float LIFESPAN = 3f + (abilityLevel >= 4 ? 2f : 0f);

        Vector3 scale = Vector3.one * AZ;

        int AC = (int)attackCount.Value;

        float Type_1 = Type_1_DMG_Bonus.Value;
        float Type_2 = Type_2_DMG_Bonus.Value;
        float Type_3 = Type_3_DMG_Bonus.Value;

        for (int i = 0; i < AC; i++)
        {
            GameObject target = EnemyManager.instance.GetNearestEnemy(owner.transform.position, attackRange.Value / 2f);
            if (target == null)
                continue;

            GameObject grenadeObject = grenades.RequestPoolable(owner.transform.position);

            if (grenadeObject == null)
                continue;

            Vector3 targetPos = target.transform.position;
            Vector3 dir = (targetPos - owner.transform.position).normalized;

            grenadeObject.transform.forward = dir;

            NeutrophilGrenade grenade = grenadeObject.GetComponent<NeutrophilGrenade>();
            grenade.targetPos = targetPos;
            grenade.attackDamage = AD;
            grenade.attackSize = AZ;
            grenade.critRate = CRIT_RATE;
            grenade.critDMG = CRIT_DMG;
            grenade.slowAmount = SLOW;
            grenade.lifeSpan = LIFESPAN;

            grenade.transform.localScale = scale;

            grenade.Type_1_DMG_Bonus = Type_1;
            grenade.Type_2_DMG_Bonus = Type_2;
            grenade.Type_3_DMG_Bonus = Type_3;

            yield return wait;
            Debug.Log($"GRANADA: {grenadeObject.transform.position}");
        }
    }

    public override void EndAbility()
    {
        IsAttacking = false;
        base.EndAbility();
    }

    // Cache all attributes required by this ability
    public void Init()
    {
        attributes = owner.GetComponent<AttributeSet>();

        level = attributes.GetAttribute("Level");
        attackDamage = attributes.GetAttribute("Attack Damage");
        critRate = attributes.GetAttribute("Critical Rate");
        critDMG = attributes.GetAttribute("Critical Damage");
        attackSpeed = attributes.GetAttribute("Attack Speed");
        attackRange = attributes.GetAttribute("Attack Range");
        attackCount = attributes.GetAttribute("Attack Count");
        attackSize = attributes.GetAttribute("Attack Size");

        Type_1_DMG_Bonus = attributes.GetAttribute("Type_1 DMG Bonus");
        Type_2_DMG_Bonus = attributes.GetAttribute("Type_2 DMG Bonus");
        Type_3_DMG_Bonus = attributes.GetAttribute("Type_3 DMG Bonus");

        basicAttack = ability as Neutrophil_Grenade;

        grenades = GameObject.Find("Neutrophil Grenade Pool").GetComponentInChildren<ObjectPool>();
    }
}