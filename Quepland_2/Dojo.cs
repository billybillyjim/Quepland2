using System;
using System.Collections.Generic;

public class Dojo
{
	public string Name { get; set; }
	public string Description { get; set; }
	public List<string> OpponentNames { get; set; }
	public string URL { get; set; }
	public List<string> NPCs { get; set; } = new List<string>();
	private List<Monster> opponents;
	public List<Monster> Opponents { 
		get 
		{ 
			if(opponents == null)
            {
				opponents = new List<Monster>();
				foreach(string o in OpponentNames)
                {
					Monster m = BattleManager.Instance.GetMonsterByName(o);
					if(m != null)
                    {
						opponents.Add(m);
                    }
                    else
                    {
						Console.WriteLine("Failed to find opponent name:" + o + " for dojo:" + Name);
                    }
                }
            }
			return opponents;
		}
	}
	public int CurrentOpponent { get; set; }
	public bool HasBegunChallenge { get; set; }
	public DateTime? LastWinTime { get; set; }
	public DateTime? LastLossTime { get; set; }

	public void Reset()
    {
		HasBegunChallenge = false;
		CurrentOpponent = 0;
    }

	public DojoSaveData GetSaveData()
    {
		return new DojoSaveData { Name = Name, LastWin = LastWinTime };
    }
	public void LoadFromSave(DojoSaveData data)
    {
		LastWinTime = data.LastWin;
    }
}
