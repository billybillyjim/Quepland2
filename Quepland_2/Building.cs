using Quepland_2.Components;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

public class Building
{
	public string Name { get; set; }
	private string _buttonText;
	public string ButtonText { get { if (_buttonText == null) { return Name; } return _buttonText; } set { _buttonText = value; } }
	public string Description { get; set; }
	public string URL { get; set; }
    public bool HasOven { get; set; }
    public bool HasSmithy { get; set; }
    public bool IsGuild { get; set; }
    public string AlchemicalHall { get; set; } = "None";
    public double QueplarMultiplier { get; set; } = 1;
	public List<string> NPCs { get; set; } = new List<string>();
	public List<Shop> Shops { get; set; } = new List<Shop>();
	public List<TanningSlot> TanningSlots { get; set; } = new List<TanningSlot>();
    public List<Requirement> Requirements { get; set; } = new List<Requirement>();
    public List<AFKAction> AFKActions { get; set; } = new List<AFKAction>();
    public int LoadedTanningSlotsIterator;

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
    public void LoadTanningData(TanningSaveData data)
    {
        try
        {
            TanningSlots[LoadedTanningSlotsIterator].LoadData(data);
            LoadedTanningSlotsIterator++;
        }
        catch
        {
            Console.WriteLine("Failed to load tanning save data.");
        }
        
    }
}
