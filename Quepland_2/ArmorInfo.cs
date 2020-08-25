using System;
using System.Collections.Generic;

public class ArmorInfo
{
    public int Damage { get; set; }
    public int ArmorBonus { get; set; }
    public List<Requirement> WearRequirements { get; set; } = new List<Requirement>();
    public List<IStatusEffect> StatusEffects { get; set; } = new List<IStatusEffect>();
    public List<StatusEffectData> StatusEffectData { get; set; } = new List<StatusEffectData>();
    public ArmorInfo()
    {
        foreach (StatusEffectData data in StatusEffectData)
        {
            StatusEffects.Add(BattleManager.Instance.GenerateStatusEffect(data));
        }
    }
}
