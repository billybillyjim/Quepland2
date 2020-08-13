using Quepland_2.Components;
using System;
using System.Collections.Generic;
using System.Linq;

public class Dungeon
{
	private double _progress;
	public double Progress 
	{ get { return _progress; } 
		set{ 
			_progress = Math.Min(value, Size); 
			if(_progress == Size)
            {
				IsComplete = true;
            }
		} 
	}
	public bool IsComplete { get; set; }
	public double Size { get; set; }
	public string Name { get; set; }
	public string URL { get; set; }
	public string ButtonText { get; set; } = "Unset"; 
	public List<string> AreaURLs { get; set; }
	private List<Area> _areas;
	public List<Area> Areas { 
		get { 
			if(_areas == null)
            {
				_areas = new List<Area>();
				foreach (string a in AreaURLs)
				{
					_areas.Add(AreaManager.Instance.GetAreaByURL(a));
					if(AreaManager.Instance.GetAreaByURL(a) == null)
                    {
						Console.WriteLine("Dungeon " + Name + " has json typo:" + a);
                    }
				}
				return _areas;
			}
			return _areas;
		} 
	}
	public List<string> MonsterNames { get; set; }
	private List<Monster> _monsters;
	public List<Monster> Monsters
	{
		get
		{
			if (_monsters == null)
			{
				_monsters = new List<Monster>();
				if(MonsterNames == null)
                {
					Console.WriteLine(Name + " has no monster names.");
                }
				foreach (string m in MonsterNames)
				{
					_monsters.Add(BattleManager.Instance.GetMonsterByName(m));
				}
				return _monsters;
			}
			return _monsters;
		}
	}
	private static readonly Random random = new Random();

	public Area GetRandomArea()
    {
		if(Areas == null || Areas.Count == 0)
        {
			return null;
        }
		return Areas[random.Next(0, Areas.Count)];
    }
	public Monster GetRandomMonster()
    {
		if(Monsters == null || Monsters.Count == 0)
        {
			return null;
        }
		return Monsters[random.Next(0, Monsters.Count)];
    }
	public List<Area> GetLockedAreas()
    {
		if(Areas == null || Areas.Count == 0)
        {
			return new List<Area>();
        }
		return Areas.Where(x => x.IsUnlocked == false).ToList();
    }
	public List<Area> GetUnlockedAreas()
	{
		if (Areas == null || Areas.Count == 0)
		{
			return new List<Area>();
		}
		return Areas.Where(x => x.IsUnlocked == true).ToList();
    }
	public Area GetRandomLockedArea()
    {
		List<Area> locked = GetLockedAreas();
		if(locked.Count == 0) { return null; }
		return locked[random.Next(0, locked.Count)];
    }
	public double GetPercentProgress()
    {
		return (Progress / Size) * 100d;
    }
	public string GetButtonText()
    {
		if(ButtonText == "Unset")
        {
			return "Enter " + Name;
        }
		return ButtonText;
    }
	public DungeonSaveData GetSaveData()
    {
		return new DungeonSaveData() { Progress = Progress, Name = Name, IsComplete = IsComplete };
    }
	public void LoadSaveData(DungeonSaveData data)
    {
		Progress = data.Progress;
		IsComplete = data.IsComplete;
    }
}
