using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BurnEffect : IStatusEffect
{
    public string Name { get; set; } = "Burn";
    public int Duration { get; set; }
    public int Speed { get; set; }
    public int Power { get; set; }
    public int RemainingTime { get; set; }

    public double ProcOdds { get; set; }
    public bool SelfInflicted { get; set; }

    public string Message { get; set; }
    private StatusEffectData d;
    public BurnEffect(StatusEffectData data)
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
            MessageManager.AddMessage(m.Name + " took " + Power + " damage from being on fire.");
            m.CurrentHP -= Power;
        }
    }
    public void DoEffect(Player p)
    {
        if (RemainingTime % Speed == 0 && RemainingTime > 0)
        {
            MessageManager.AddMessage("You took " + Power + " damage from being on fire.");
            p.CurrentHP -= Power;
        }
    }
    public IStatusEffect Copy()
    {
        return new BurnEffect(d);
    }
}

