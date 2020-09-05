using System;
using System.Collections.Generic;

public class PlayerSaveData
{
	public int CurrentHP { get; set; }
	public int MaxHP { get; set;}
	public int DeathCount { get; set; }
	public int ArtisanPoints { get; set; }
	public int InventorySize { get; set; }
	public string ActiveFollowerName { get; set; }
	public List<string> EquippedItems { get; set; }
}
