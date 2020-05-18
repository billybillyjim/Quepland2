using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class SmithingInfo
{
    [JsonIgnore]
	private GameItem _smeltsInto { get; set; }
	public string SmeltsIntoString { get; set; }
    [JsonIgnore]
	public GameItem SmeltsInto { get
        {
			if(_smeltsInto == null)
            {
                _smeltsInto = ItemManager.Instance.GetItemByName(SmeltsIntoString);
            }
            return _smeltsInto;
        } 
    }
    [JsonIgnore]
    private List<GameItem> _smithsInto { get; set; }
    public List<string> SmithsIntoString { get; set; }
    [JsonIgnore]
    public List<GameItem> SmithsInto { get
        {
            if(SmithsIntoString == null || SmithsIntoString.Count == 0)
            {
                return new List<GameItem>();
            }
            if (_smithsInto == null)
            {
                _smithsInto = new List<GameItem>();
                foreach(string s in SmithsIntoString)
                {
                    _smithsInto.Add(ItemManager.Instance.GetItemByName(s));
                }
            }
            return _smithsInto;
        } 
    }
    public int SmeltingExperience { get; set; }
    public int SmithingExperience { get; set; }
    public int SmeltingSpeed { get; set; } = 12;
    public int SmithingSpeed { get; set; } = 18;
}
