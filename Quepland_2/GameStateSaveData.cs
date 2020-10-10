using System;

public class GameStateSaveData
{
	public bool IsHunting { get; set; }
	public string Location { get; set; }
	public string CurrentLand { get; set; }
	public ArtisanTask CurrentTask { get; set; }
	public bool CompactInventory { get; set; }
	public bool HideLockedItems { get; set; }
}
