using System;
using System.Collections.Generic;

public class AreaUnlock
{
	public string AreaURL { get; set; }
	public string ButtonText { get; set; }
	public List<Requirement> Requirements { get; set; } = new List<Requirement>();
	public bool HasRequirements()
    {
		foreach(Requirement r in Requirements)
        {
			if(r.IsMet() == false)
            {
				return false;
            }
        }
		return true;
    }
}
