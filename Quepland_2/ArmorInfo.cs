using System;
using System.Collections.Generic;

public class ArmorInfo
{
    public int Damage { get; set; }
    public int ArmorBonus { get; set; }
    public List<Requirement> WearRequirements { get; set; } = new List<Requirement>();
    private List<IStatusEffect> statusEffects;
    public List<IStatusEffect> StatusEffects
    {
        get
        {
            if (statusEffects == null)
            {
                statusEffects = new List<IStatusEffect>();
                foreach (StatusEffectData data in StatusEffectData)
                {
                    statusEffects.Add(BattleManager.Instance.GenerateStatusEffect(data));
                }
            }
            return statusEffects;
        }
    }
    public List<StatusEffectData> StatusEffectData { get; set; } = new List<StatusEffectData>();
    public ArmorInfo()
    {

    }
}
