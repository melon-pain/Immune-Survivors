using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Dendritic_Mobility", menuName = "Ability System/Abilities/Dendritic Mobility")]
public class Dendritic_Mobility : Ability
{
    [field: SerializeField] public float DashDistance { get; private set; }

    public override AbilitySpec CreateSpec(AbilitySystem owner)
    {
        AbilitySpec spec = new Dendritic_MobilitySpec(this, owner);
        return spec;
    }
}

public class Dendritic_MobilitySpec : AbilitySpec
{
    public Dendritic_MobilitySpec(Dendritic_Mobility ability, AbilitySystem owner) : base(ability, owner)
    {
        Init();
    }

    private Dendritic_Mobility mobility;

    private AttributeSet attributes;
    private Attribute attackDamage;
    private Attribute critRate;
    private Attribute critDMG;
    public Attribute Type_1_DMG_Bonus;
    public Attribute Type_2_DMG_Bonus;
    public Attribute Type_3_DMG_Bonus;

    private readonly LayerMask layerMask = LayerMask.GetMask("Enemy");

    public bool IsDashing { get; private set; } = false;

    public override bool CanActivateAbility()
    {
        return base.CanActivateAbility() && !IsDashing;
    }

    private Collider collider;
    private PlayerMovement movement;
    private SpriteRenderer sprite;
    private Animator animator;

    public override IEnumerator ActivateAbility()
    {
        animator.SetTrigger("Mobility");

        Vector3 direction = movement.lastInputDir;
        Vector3 startPos = owner.transform.position;
        Vector3 endPos = startPos + (direction * mobility.DashDistance);

        Vector3 rayDir = direction;
        float rayLength = mobility.DashDistance;

        collider.enabled = false;
        movement.enabled = false;

        Physics.IgnoreLayerCollision(6, 11, true);
        IsDashing = true;
        yield return new WaitForSeconds(0.2f);

        Physics.IgnoreLayerCollision(6, 11, false);

        AudioManager.instance.Play("DentriticMovement", owner.transform.position);
        movement.transform.position = endPos;

        var hits = Physics.SphereCastAll(startPos, 1f, rayDir, rayLength, layerMask);

        float AD = attackDamage.Value;
        float CRIT_RATE = critRate.Value;
        float CRIT_DMG = critDMG.Value;

        float Type_1 = Type_1_DMG_Bonus.Value;
        float Type_2 = Type_2_DMG_Bonus.Value;
        float Type_3 = Type_3_DMG_Bonus.Value;

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent(out Enemy enemy))
            {
                float MaxHP = enemy.MaxHP.Value;
                float HP = enemy.HP.Value;

                float DMGBonus = Type_1_DMG_Bonus.Value;

                switch (enemy.Type)
                {
                    case AntigenType.Type_1:
                        DMGBonus = Type_1;
                        break;
                    case AntigenType.Type_2:
                        DMGBonus = Type_2;
                        break;
                    case AntigenType.Type_3:
                        DMGBonus = Type_3;
                        break;
                }

                float ratio = Mathf.SmoothStep(0.25f, 0.75f, (HP / MaxHP));
                float missingHPBonusDMG = Mathf.Lerp(1f, 0.1f, ratio);

                //enemy.TakeDamage(damage);
                float damage = AD * DMGBonus * missingHPBonusDMG;
                float armor = enemy.Armor.Value;
                DamageCalculator.ApplyDamage(damage, CRIT_RATE, CRIT_DMG, armor, enemy);
                enemy.GetComponent<ImpactReceiver>().AddImpact(rayDir, rayLength);
            }
        }

        bool resetCD = false;

        // Check for dead enemies
        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                // If at least 1 enemy was killed, reset cooldown
                if (!resetCD && enemy.IsDead)
                {
                    // Get bonus Antigen
                    AntigenManager.instance.AddAntigen(enemy.Type);
                    resetCD = true;
                }
            }
        }

        // Teleport behind the last enemy hit
        if (hits.Length > 0)
        {
            if (Vector3.Distance(startPos, hits.Last().point) > Vector3.Distance(startPos, endPos))
                owner.transform.position = hits.Last().point + (rayDir * 1.5f);
        }

        // If no enemy was killed, do not reset CD
        if (!resetCD)
        {
            AudioManager.instance.Play("PlayerPickUp", owner.transform.position);
            CurrentCD = MaxCD;
            owner.StartCoroutine(UpdateCD());
        }

        owner.transform.localPosition = Vector3.zero;
        yield break;
    }

    public override void EndAbility()
    {
        base.EndAbility();
        IsDashing = false;

        PlayerMovement movement = owner.GetComponentInParent<PlayerMovement>();
        Collider collider = owner.GetComponent<Collider>();
        collider.enabled = true;
        movement.enabled = true;
    }

    private void Init()
    {
        attributes = owner.GetComponent<AttributeSet>();
        attackDamage = attributes.GetAttribute("Attack Damage");
        critRate = attributes.GetAttribute("Critical Rate");
        critDMG = attributes.GetAttribute("Critical Damage");
        CDReduction = attributes.GetAttribute("CD Reduction");

        Type_1_DMG_Bonus = attributes.GetAttribute("Type_1 DMG Bonus");
        Type_2_DMG_Bonus = attributes.GetAttribute("Type_2 DMG Bonus");
        Type_3_DMG_Bonus = attributes.GetAttribute("Type_3 DMG Bonus");

        mobility = ability as Dendritic_Mobility;

        collider = owner.GetComponent<Collider>(); 
        movement = owner.GetComponentInParent<PlayerMovement>();
        sprite = owner.GetComponentInChildren<SpriteRenderer>();
        animator = sprite.GetComponent<Animator>();
    }
}