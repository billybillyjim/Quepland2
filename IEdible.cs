using System;

public interface IEdible
{
	int HealAmount { get; set; }
	int BuffAmount { get; set; }
	int HealDuration { get; set; }
	string BuffedSkill { get; set; }
	void Consume();
}
