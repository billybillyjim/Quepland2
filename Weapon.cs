using System;

public class Weapon : GameItem, IEquippable
{
    public int AttackSpeed { get; set; }
    public int Damage { get; set; }
    public int ArmorBonus { get; set; }
    public string StatusEffect { get; set; }
    public int EffectDuration { get; set; }
    public string EquipSlot { get; set; }
    public void Equip()
    {
        
    }
}
