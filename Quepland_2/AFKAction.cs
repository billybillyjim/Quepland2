using System;
using System.Collections.Generic;


public class AFKAction
{
	public int ExpPerHour { get; set; }
	public int BonusExp { get; set; }
	public string SkillTrained { get; set; } = "None";
	public string ButtonText { get; set; } = "None";
	public List<Requirement> Requirements { get; set; } = new List<Requirement>();
	public bool IsActive { get; set; }
	public DateTime ReturnTime { get; set; }
	public DateTime StartTime { get; set; }

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
    public string GetRequirementTooltip()
    {
        if (HasRequirements())
        {
            return "";
        }
        string req = "";

        foreach (Requirement r in Requirements)
        {
            if (r.IsMet() == false)
            {
                req += r.ToString() + "\n";
            }
        }
        
        req = req.Substring(0, req.Length - 1);
        return req;
    }
    public bool IsReady()
    {
		return DateTime.UtcNow.CompareTo(ReturnTime) > 0;
    }
    public override bool Equals(object obj)
    {
        if(obj is AFKAction == false)
        {
			return false;
        }
		return ((AFKAction)obj).ExpPerHour == ExpPerHour && ((AFKAction)obj).SkillTrained == SkillTrained && ((AFKAction)obj).ButtonText == ButtonText;

	}
}
