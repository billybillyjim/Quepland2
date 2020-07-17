using System;
using System.Collections.Generic;

public class WeaponInfo
{
    public int AttackSpeed { get; set; }
    public int Damage { get; set; }
    public int ArmorBonus { get; set; }
    public string StatusEffect { get; set; }
    public int EffectDuration { get; set; }

    public bool IsArrow { get; set; }
    public List<Requirement> WearRequirements { get; set; }
}
