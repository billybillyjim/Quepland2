using System;

public interface IStatusEffect
{
	public string Name { get; set; }
	/// <summary>
    /// The duration of the effect in game ticks.
    /// </summary>
	public int Duration { get; set; }
	/// <summary>
	/// The number of game ticks between effects.
	/// </summary>
	public int Speed { get; set; }
	/// <summary>
	/// Remaining time in game ticks.
	/// </summary>
	public int RemainingTime { get; set; }
	/// <summary>
	/// The odds of the effect being attached to the player or monster. Ranges from 0-1.
	/// </summary>
	public double ProcOdds { get; set; }
	/// <summary>
	/// Describes the amount of damage/stun/reduction/etc. the effect causes.
	/// </summary>
	public int Power { get; set; }
	/// <summary>
	/// The message sent when the effect is first applied.
	/// </summary>
	public string Message { get;set; }
	/// <summary>
	/// For special effects that affect the user, not the opponent.
	/// </summary>
	public bool SelfInflicted { get; set; }
	public void DoEffect(Monster m);
	public void DoEffect(Player p);
	public IStatusEffect Copy();
	
}
