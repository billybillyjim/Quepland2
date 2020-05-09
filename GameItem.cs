using System;

public class GameItem
{
	public string Name { get; set; } = "Unset Name";
	public string Description { get; set; } = "Unset Description";
	public string GatherString { get; set; } = "You get an item";
	public string ExperienceGained { get; set; } = "None";
	public string EnabledActions { get; set; } = "None";
	public string RequiredAction { get; set; } = "None";

	public string Icon { get; set; } = "Unset";
	public string EquipSlot { get; set; }

	public bool IsStackable { get; set; }
	public bool IsEquipped { get; set; }

	public int Value { get; set; } = 1;
	/// <summary>
	/// The number of game ticks it takes on average to acquire one resource.
	/// </summary>
	public int GatherSpeed { get; set; } = 10;
	public int RequiredLevel { get; set; }
	public override string ToString()
	{
		return Name;
	}

}
