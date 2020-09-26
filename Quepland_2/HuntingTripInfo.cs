using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class HuntingTripInfo
{
	public DropTable DropTable { get; set; }
	public int SuggestedHuntingLevel { get; set; }
	public List<Requirement> Requirements { get; set; } = new List<Requirement>();
	public bool IsActive { get; set; }
	public DateTime ReturnTime { get; set; }
	public DateTime StartTime { get; set; }
	public int LevelBoost { get; set; }
	public string DropTableLocation { get; set; } = "";

	public void LoadSaveData(bool isActive, DateTime rTime, DateTime sTime, int boost, string dtLocation)
    {

			IsActive = isActive;
			ReturnTime = rTime;
			StartTime = sTime;
			LevelBoost = boost;
			DropTableLocation = dtLocation;

    }
}
