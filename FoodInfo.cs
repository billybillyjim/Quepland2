using System;

public class FoodInfo
{
	public int HealAmount { get; set; }
	/// <summary>
	/// The number of heals per item eaten. Decrements once every HealSpeed ticks.
	/// </summary>
	public int HealDuration { get; set; }
	public string BuffedSkill { get; set; }
	public int BuffAmount { get; set; }
	public int HealSpeed { get; set; } = 5;
}
