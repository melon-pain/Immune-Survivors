using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveEffects : MonoBehaviour
{
    [SerializeField] private EffectType type;

    //This field is only if type == EffectType.Buff
    [SerializeField] private PlayerUnitType unit;

    [SerializeField] private List<Image> effectSlots = new List<Image>();

    public void Start()
    {
        if (type == EffectType.Buff)
        {
            UpgradeManager.instance.OnEffectAcquired += UpdateBuffs;
        }
        else
        {
            UpgradeManager.instance.OnEffectAcquired += UpdateWeapons;
        }
    }

    public void OnDestroy()
    {
        //if (type == EffectType.Buff)
        //{
        //    UpgradeManager.instance.OnEffectAcquired -= UpdateBuffs;
        //}
        //else
        //{
        //    UpgradeManager.instance.OnEffectAcquired -= UpdateWeapons;
        //}
    }

    public void UpdateBuffs()
    {
        Dictionary<Effect, int> buffs = UpgradeManager.instance.GetEffects(unit);

        int i = 0;

        foreach (var key in buffs.Keys)
        {
            effectSlots[i].sprite = key.Sprite;
            effectSlots[i].gameObject.SetActive(true);
            i++;
        }
    }

    public void UpdateWeapons()
    {
        Dictionary<Effect, int> weapons = UpgradeManager.instance.GetWeapons();

        int i = 0;

        foreach(var key in weapons.Keys)
        {
            effectSlots[i].sprite = key.Sprite;
            effectSlots[i].gameObject.SetActive(true);
            i++;
        }
    }
}
