using System;
using System.Collections.Generic;

public class Dialog
{ 
	public string ButtonText { get; set; } = "Unset";
	public string ResponseText { get; set; } = "Unset";
	public string Quest { get; set; } = "None";
	public int NewQuestProgressValue { get; set; }
	public List<Requirement> Requirements { get; set; } = new List<Requirement>();

	public bool HasRequirements()
	{
		foreach (Requirement r in Requirements)
		{
			if (r.IsMet() == false)
			{
				return false;
			}
		}

		return true;
	}
	
}
