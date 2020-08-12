using System;

public class HunterTrapSlot
{
	public DateTime HarvestTime { get; set; }
	public GameItem Trap { get; set; }
    public DropTable DropTable { get; set; }
    public string State { get; set; } = "Unset";
    private static Random rand = new Random();

	public void SetTrap(GameItem trap)
    {
        if (trap.TrapInfo != null)
        {
            HarvestTime = DateTime.UtcNow + new TimeSpan(0, trap.TrapInfo.TimeToReady, 0);
            Trap = trap;
        }
    }
    public void Collect()
    {
        for(int i = 0; i < Trap.TrapInfo.Size;i++)
        {
            Drop drop = DropTable.GetDrop();
            double chance = 1;
            GameItem dropItem = ItemManager.Instance.GetItemByName(drop.ItemName);
            foreach (Requirement req in dropItem.Requirements)
            {
                if(req.Skill == "Hunting")
                {
                    if(Player.Instance.GetLevel("Hunting") >= req.SkillLevel)
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
            if(chance >= rand.NextDouble())
            {
                MessageManager.AddMessage("You find a " + drop.ItemName + " in the trap.");
                Player.Instance.GainExperience(dropItem.ExperienceGained);
                Player.Instance.Inventory.AddDrop(drop);
            }
            else
            {
                MessageManager.AddMessage("You find evidence a " + drop.ItemName + " escaped the trap.");

            }
        }
    }
}
