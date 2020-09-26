using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class EmptyEffect : IStatusEffect
{
    public string Name { get; set; } = "Empty";
    public int Duration { get; set; }
    public int Speed { get; set; }
    public int Power { get; set; }
    public int RemainingTime { get; set; }

    public double ProcOdds { get; set; }
    public bool SelfInflicted { get; set; }
    private StatusEffectData d;

    public string Message { get; set; }
    public EmptyEffect(StatusEffectData data)
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
        if (RemainingTime % Speed == 0 && RemainingTime > 0)
        {
           
            MessageManager.AddMessage(m.Name + " has lost its regenerative power!");
        }
    }
    public void DoEffect(Player p)
    {
        if (RemainingTime % Speed == 0 && RemainingTime > 0)
        {
            GameState.CancelEating = true;
            MessageManager.AddMessage("You feel empty inside...");
        }
    }
    public IStatusEffect Copy()
    {
        return new EmptyEffect(d);
    }
}

