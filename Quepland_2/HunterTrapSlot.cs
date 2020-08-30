using System;

public class HunterTrapSlot
{
	public DateTime HarvestTime { get; set; }
	public GameItem Trap { get; set; }
    public DropTable DropTable { get; set; }
    public string State { get; set; } = "Unset";
    public int Size { get; set; }
    private static Random rand = new Random();

	public void SetTrap(GameItem trap)
    {
        if (trap.TrapInfo != null)
        {
            HarvestTime = DateTime.UtcNow + new TimeSpan(0, trap.TrapInfo.TimeToReady, 0);
            State = "Set";
            Trap = trap;
            Size = trap.TrapInfo.Size;
        }
    }
    public void Collect()
    {
        for(int i = 0; i < Size;i++)
        {
            try
            {
                Drop drop = DropTable.GetDrop();
                double chance = 1;
                foreach (Requirement req in drop.Item.Requirements)
                {
                    if (req.Skill == "Hunting")
                    {
                        if (Player.Instance.GetLevel("Hunting") >= req.SkillLevel)
                        {
                            chance = 1;
                        }
                        else
                        {
                            chance = 1 / (req.SkillLevel - Player.Instance.GetLevel("Hunting"));
                        }
                        continue;
                    }
                }
                if (chance >= rand.NextDouble())
                {
                    MessageManager.AddMessage("You find a " + drop.Item + " in the trap.");
                    Player.Instance.GainExperience(drop.Item.ExperienceGained);
                    Player.Instance.Inventory.AddDrop(drop);
                }
                else
                {
                    MessageManager.AddMessage("You find evidence a " + drop.Item.Name + " escaped the trap.");

                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Drop was null.");
                Console.WriteLine(e);
                MessageManager.AddMessage("You find nothing in the trap. It's strange... It shouldn't be this way...");
            }

        }
        State = "Unset";
    }
}
