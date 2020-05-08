using System;

public interface IEquippable
{
	int AttackSpeed { get; set; }
	int Damage { get; set; }
	int ArmorBonus { get; set; }
	string StatusEffect { get; set; }
	int EffectDuration { get; set; }
	string EquipSlot { get; set; }

	void Equip();

}
