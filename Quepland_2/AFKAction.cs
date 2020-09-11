using System;
using System.Collections.Generic;


public class AFKAction
{
    public int ExpPerHour { get; set; } = 0;
    public int BonusExp { get; set; } = 0;
	public string SkillTrained { get; set; } = "None";
	public string ButtonText { get; set; } = "None";
	public List<Requirement> Requirements { get; set; } = new List<Requirement>();
	public bool IsActive { get; set; }
	public DateTime ReturnTime { get; set; }
	public DateTime StartTime { get; set; }
    public string UniqueID { get
        {
            return SkillTrained + ButtonText + ExpPerHour + BonusExp;
        } 
    }

    public TimeSpan GetRemainingTime()
    {
        return ReturnTime.Subtract(DateTime.UtcNow);
    }
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
}
