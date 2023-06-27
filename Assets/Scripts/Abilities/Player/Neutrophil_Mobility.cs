using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Neutrophil_Mobility", menuName = "Ability System/Abilities/Neutrophil Mobility")]
public class Neutrophil_Mobility : Ability
{
    [field: SerializeField] public float DashSpeed { get; private set; }
    [field: SerializeField] public float MaxDashTime { get; private set; }

    public override AbilitySpec CreateSpec(AbilitySystem owner)
    {
        AbilitySpec spec = new Neutrophil_MobilitySpec(this, owner);
        return spec;
    }
}

public class Neutrophil_MobilitySpec : AbilitySpec
{
    public Neutrophil_MobilitySpec(Neutrophil_Mobility ability, AbilitySystem owner) : base(ability, owner)
    {

    }

    public bool IsDashing { get; private set; } = false;

    public override bool CanActivateAbility()
    {
        return base.CanActivateAbility() && !IsDashing;
    }

    public override IEnumerator ActivateAbility()
    {
        CurrentCD = ability.Cooldown;
        owner.StartCoroutine(UpdateCD());

        float tick = 0f;

        var mobility = ability as Neutrophil_Mobility;
        CharacterController controller = owner.GetComponentInParent<CharacterController>();
        Vector3 direction = controller.velocity.normalized;

        IsDashing = true;

        while (tick < mobility.MaxDashTime)
        {
            tick += Time.deltaTime;
            controller.Move(direction * (mobility.DashSpeed * Time.deltaTime));
            yield return null;
        }

        yield break;
    }

    public override void EndAbility()
    {
        IsDashing = false;
        base.EndAbility();
    }
}