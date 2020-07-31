using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Transactions;

public class Quest
{
	public string Name { get; set; }
	private int _progress;
	public int Progress { get { return _progress; } 
		set {
			_progress = value;
			if (_progress == ProgressToComplete) { Complete(); } 
		} 
	}
	public int ProgressToComplete { get; set; }
	public List<Requirement> Requirements { get; set; } = new List<Requirement>();
	public List<Reward> Rewards { get; set; } = new List<Reward>();
	public string CompletionText { get; set; } = "Unset";
	public bool IsComplete { get; set; }
	public int ID { get; set; }

	public void Complete()
    {
		if(IsComplete == false)
        {
			IsComplete = true;
			foreach (Reward reward in Rewards)
			{
				reward.Award();
				MessageManager.AddMessage("You earned " + reward.ToString());
			}
			MessageManager.AddMessage(CompletionText);
		}
        else
        {
			Console.WriteLine("Game attempted to complete Quest:" + Name + " more than once. Progress:" + Progress);
        }
    }
	public QuestSaveData GetSaveData()
    {
		return new QuestSaveData { ID = ID, IsCompleted = IsComplete, Progress = Progress };
    }
	public void LoadFromSave(QuestSaveData data)
    {
		IsComplete = data.IsCompleted;
		_progress = data.Progress;
    }
}
