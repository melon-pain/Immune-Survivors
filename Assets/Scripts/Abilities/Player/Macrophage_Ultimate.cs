using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[CreateAssetMenu(fileName = "Macrophage_Ultimate", menuName = "Ability System/Abilities/Macrophage Ultimate")]
public class Macrophage_Ultimate : Ability
{
    [field: SerializeField] public float Duration { get; private set; }

    public override AbilitySpec CreateSpec(AbilitySystem owner)
    {
        return new Macrophage_UltimateSpec(this, owner);
    }
}

public class Macrophage_UltimateSpec : AbilitySpec
{
    public Macrophage_UltimateSpec(Macrophage_Ultimate ability, AbilitySystem owner) : base(ability, owner)
    {
        Init();
    }

    public AttributeSet attributes;
    public Attribute level;
    public Attribute hpRegen;
    public Attribute attackDamage;
    public Attribute attackSpeed;
    public Attribute attackRange;
    public Attribute attackSize;
    public Attribute armor;
    public Attribute knockbackPower;
    public Attribute dotAmount;
    public Attribute dotDuration;
    public Attribute dotTickRate;

    private SpriteRenderer sprite;
    private Animator animator;
    private GameObject outline;

    // TODO: Make required level visible on ScriptableObject
    public override bool CanActivateAbility()
    {
        if (GameManager.instance)
        {
            if (GameManager.instance.GamePaused || GameManager.instance.GameTimePaused)
                return false;
        }

        return level.Value >= 5f && base.CanActivateAbility();
    }

    public override IEnumerator ActivateAbility()
    {
        Macrophage_Ultimate ultimate = ability as Macrophage_Ultimate;
        
        var playable = owner.GetComponent<PlayableDirector>();
        playable.Play();

        animator.SetTrigger("Ultimate"); outline.SetActive(false);

        foreach (Transform child in sprite.transform)
        {
            child.gameObject.SetActive(false);
        }

        AttributeModifier regenMod = new(1f, AttributeModifierType.Multiply);
        hpRegen.AddModifier(regenMod);

        // 100% increase in attack values
        AttributeModifier attackMod = new(1f, AttributeModifierType.Multiply);
        attackDamage.AddModifier(attackMod);
        attackSpeed.AddModifier(attackMod);
        attackRange.AddModifier(attackMod);

        AttributeModifier knockbackMod = new(1f, AttributeModifierType.Add);
        knockbackPower.AddModifier(knockbackMod);

        AttributeModifier armorMod = new((level.Value + 1) * 2f, AttributeModifierType.Add);
        armor.AddModifier(armorMod);

        AttributeModifier sizeMod = new(level.Value * 0.1f, AttributeModifierType.Add);
        attackSize.AddModifier(sizeMod);

        // 500% increased DoT
        AttributeModifier dotMod = new(5f, AttributeModifierType.Multiply);
        dotAmount.AddModifier(dotMod);
        dotDuration.AddModifier(dotMod);
        
        AudioManager.instance.Play("MacrophageUltimate", owner.transform.position);
        yield return new WaitForSeconds(ultimate.Duration);

        foreach (Transform child in sprite.transform)
        {
            child.gameObject.SetActive(true);
        }

        // Remove all modifiers
        hpRegen.RemoveModifier(regenMod);

        attackDamage.RemoveModifier(attackMod);
        attackSpeed.RemoveModifier(attackMod);
        attackRange.RemoveModifier(attackMod);

        knockbackPower.RemoveModifier(knockbackMod);
        attackSize.RemoveModifier(sizeMod);

        dotAmount.RemoveModifier(dotMod);
        dotDuration.RemoveModifier(dotMod);

        armor.RemoveModifier(armorMod);
        outline.SetActive(true);
        CurrentCD = MaxCD;
        owner.StartCoroutine(UpdateCD());

        yield break;
    }

    public void Init()
    {
        attributes = owner.GetComponent<AttributeSet>();

        level = attributes.GetAttribute("Level");
        hpRegen = attributes.GetAttribute("HP Regen");
        attackDamage = attributes.GetAttribute("Attack Damage");
        attackSpeed = attributes.GetAttribute("Attack Speed");
        attackRange = attributes.GetAttribute("Attack Range");
        attackSize = attributes.GetAttribute("Attack Size");
        knockbackPower = attributes.GetAttribute("Knockback Power");
        armor = attributes.GetAttribute("Armor");
        dotAmount = attributes.GetAttribute("DoT Amount");
        dotDuration = attributes.GetAttribute("DoT Duration");
        dotTickRate = attributes.GetAttribute("DoT Tick Rate");
        CDReduction = attributes.GetAttribute("CD Reduction");

        sprite = owner.GetComponentInChildren<SpriteRenderer>();
        animator = sprite.GetComponent<Animator>();
        outline = owner.transform.Find("Sprite").Find("Outline").gameObject;
    }
}