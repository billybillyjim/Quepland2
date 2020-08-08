using System;
using System.Collections.Generic;

public class WeaponInfo
{
    public int AttackSpeed { get; set; }
    public int Damage { get; set; }
    public int ArmorBonus { get; set; }

    public bool IsArrow { get; set; }
    public List<Requirement> WearRequirements { get; set; } = new List<Requirement>(); 
    public List<IStatusEffect> StatusEffects { get; set; } = new List<IStatusEffect>();
    public List<StatusEffectData> StatusEffectData { get; set; } = new List<StatusEffectData>();
    public WeaponInfo()
    {
        foreach (StatusEffectData data in StatusEffectData)
        {
            StatusEffects.Add(BattleManager.Instance.GenerateStatusEffect(data));
        }
    }
}
