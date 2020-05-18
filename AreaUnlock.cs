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
    public override string ToString()
    {
        string req = "";
        foreach(Requirement r in Requirements)
        {
            if(r.IsMet() == false)
            {
                req += r.ToString() + "\n";
            }        
        }
        req = req.Substring(0, req.Length - 1);
        return req;
    }
}
