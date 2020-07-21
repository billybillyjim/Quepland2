using System;
using System.Collections.Generic;

public class Building
{
	public string Name { get; set; }
	private string _buttonText;
	public string ButtonText { get { if (_buttonText == null) { return Name; } return _buttonText; } set { _buttonText = value; } }
	public string Description { get; set; }
	public string URL { get; set; }
	public List<string> NPCs { get; set; } = new List<string>();
	public List<Shop> Shops { get; set; } = new List<Shop>();
	public List<TanningSlot> TanningSlots { get; set; } = new List<TanningSlot>();
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
    public string GetRequirementTooltip()
    {
        if (HasRequirements())
        {
            return "";
        }
        string req = "";

        bool hasEquipInfo = false;

        if (hasEquipInfo == false)
        {
            foreach (Requirement r in Requirements)
            {
                if (r.IsMet() == false)
                {
                    req += r.ToString().Replace("tools", "means") + "\n";
                }
            }
        }
        req = req.Substring(0, req.Length - 1);
        return req;
    }
}
