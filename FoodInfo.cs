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
	/// <summary>
	/// The speed the item heals atin game ticks. Default is every 5 ticks.
	/// </summary>
	public int HealSpeed { get; set; } = 5;
}
