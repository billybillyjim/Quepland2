using System;

public class AreaSaveData
{
	public int ID { get; set; }
	public bool IsUnlocked { get; set; }
	public DateTime TrapHarvestTime { get; set; }
	public string TrapState { get; set; }
	public int TrapSize { get; set; }
	public bool TripIsActive { get; set; }
	public DateTime TripStartTime { get; set; }
	public DateTime TripReturnTime { get; set; }
	public int HuntingBoost { get; set; }
	public string dtLocation { get; set; }
}
