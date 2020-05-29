using System;

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

	public DropTable DropTable { get; set; } = new DropTable();
	public double GetRemainingHPPercent()
    {
		return ((double)CurrentHP / HP) * 100d;
    }
}
