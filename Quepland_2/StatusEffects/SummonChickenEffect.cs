using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SummonChickenEffect : IStatusEffect
{
    public string Name { get; set; } = "Chicken";
    public int Duration { get; set; }
    public int Speed { get; set; }
    public int Power { get; set; }
    public int RemainingTime { get; set; }

    public double ProcOdds { get; set; }
    public bool SelfInflicted { get; set; }
    private StatusEffectData d;
    public string Message { get; set; }
    public SummonChickenEffect(StatusEffectData data)
    {
        Name = data.Name;
        Duration = data.Duration;
        Speed = data.Speed;
        ProcOdds = data.ProcOdds;
        Power = data.Power;
        Message = data.Message;
        RemainingTime = data.Duration;
        SelfInflicted = data.SelfInflicted; 
        d = data;
    }
    public void DoEffect(Monster m)
    {
        MessageManager.AddMessage("An enemy Chill Chicken has come to protect its egg!");
    }
    public void DoEffect(Player p)
    {

        //MessageManager.AddMessage("An enemy Chill Chicken has come to protect its egg!");
        BattleManager.Instance.ResetOpponent(BattleManager.Instance.GetMonsterByName("Chill Chicken"));
        BattleManager.Instance.CurrentOpponents.Add(BattleManager.Instance.GetMonsterByName("Chill Chicken"));
        RemainingTime = 0;

    }
    public IStatusEffect Copy()
    {
        return new SummonChickenEffect(d);
    }
}

