using System;

public class AreaSaveData
{
	public int ID { get; set; }
	public bool IsUnlocked { get; set; }
	public DateTime TrapHarvestTime { get; set; }
	public string TrapState { get; set; }
	public bool TripIsActive { get; set; }
	public DateTime TripStartTime { get; set; }
	public DateTime TripReturnTime { get; set; }
}
