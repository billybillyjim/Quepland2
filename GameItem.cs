using System;

public class GameItem
{
	public string Name { get; set; } = "Unset Name";
	public string Description { get; set; } = "Unset Description";
	public string GatherString { get; set; } = "You get an item";
	public string ExperienceGained { get; set; } = "None";
	public string ActionsEnabled { get; set; } = "None";
	public string RequiredSkill { get; set; } = "None";
	public string Icon { get; set; } = "Unset";

	public bool IsStackable { get; set; }

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
