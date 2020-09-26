using System;
using System.Collections.Generic;
using System.Linq;

public class Monster
{
	public string Name { get; set; }
	public int HP { get; set; }
	public int CurrentHP { get; set; }
	public int Armor { get; set; }
	public int Damage { get; set; }
	public int AttackSpeed { get; set; }
	public int TicksToNextAttack { get; set; }
	public bool IsDefeated { get; set; }
    public string Strengths { get; set; } = "None";
    public string Weaknesses { get; set; } = "None";
	public List<IStatusEffect> CurrentStatusEffects { get; set; } = new List<IStatusEffect>();
    public List<IStatusEffect> StatusEffects { get; set; } = new List<IStatusEffect>();
    public List<StatusEffectData> StatusEffectData { get; set; } = new List<StatusEffectData>();

	public DropTable DropTable { get; set; } = new DropTable();
	public double GetRemainingHPPercent()
    {
		return ((double)CurrentHP / HP) * 100d;
    }
    public void LoadStatusEffects()
    {
        foreach(StatusEffectData data in StatusEffectData)
        {
            StatusEffects.Add(BattleManager.Instance.GenerateStatusEffect(data));
        }
    }
    public bool HasStatusEffect(string name)
    {
        return CurrentStatusEffects.Any(x => x.Name == name);
    }
    public void AddStatusEffect(IStatusEffect effect)
    {
        if (HasStatusEffect(effect.Name))
        {
            CurrentStatusEffects.First(x => x.Name == effect.Name).RemainingTime = effect.Duration;
        }
        else
        {
            CurrentStatusEffects.Add(effect.Copy());
        }
        
    }
    public void TickStatusEffects()
    {
        List<IStatusEffect> endedEffects = new List<IStatusEffect>();
        foreach (IStatusEffect effect in CurrentStatusEffects)
        {
            effect.RemainingTime--;
            if (effect.RemainingTime <= 0)
            {
                endedEffects.Add(effect);
            }
            else
            {
                effect.DoEffect(this);
            }
        }
        CurrentStatusEffects.RemoveAll(x => endedEffects.Contains(x));
    }
}
