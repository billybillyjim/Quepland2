using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class Player
{
    private static readonly Player instance = new Player();
    private Player() { }
    static Player() { }
    public static Player Instance
    {
        get
        {
            return instance;
        }
    }
    public string Name { get; set; } = "";
    public Inventory Inventory = new Inventory(30);
    private List<GameItem> equippedItems = new List<GameItem>();
    public List<Skill> Skills = new List<Skill>();
    public Follower CurrentFollower { get; set; }
    public int MaxHP = 50;
    public int CurrentHP;
    public int TicksToNextAttack;
    public int Deaths { get; set; }
    public bool JustDied { get; set; }

    public Skill LastGainedExp { get; set; }
    public Skill ExpTrackerSkill { get; set; }
    
    

    public async Task LoadSkills(HttpClient Http)
    {
        Skills.AddRange(await Http.GetJsonAsync<Skill[]>("data/Skills.json"));
    }
    private void IncreaseMaxHPBy(int amount)
    {
        MaxHP += amount;
    }
    public void GainExperience(string skill, long amount)
    {
        Skill s = Skills.FirstOrDefault(x => x.Name == skill);
        if (s != null)
        {
            GainExperience(s, amount);
        }
    }
    public void GainExperience(string skillAndExp)
    {
        if (string.IsNullOrEmpty(skillAndExp) || skillAndExp == "None")
        {
            return;
        }
        if (int.TryParse(skillAndExp.Split(':')[1], out int amount))
        {
            GainExperience(Skills.FirstOrDefault(x => x.Name == skillAndExp.Split(':')[0]), amount);
        }
    }
    public void GainExperienceMultipleTimes(string skillAndExp, int times)
    {
        for(int i = 0; i < times; i++)
        {
            GainExperience(skillAndExp);
        }
    }
    public double GetGearMultiplier(GameItem item)
    {
        if(item.Requirements == null || item.Requirements.Count == 0)
        {
            return 1;
        }
        string skill = item.Requirements.FirstOrDefault(x => x.Skill != "None").Skill;
        double multi = 1;
        foreach(GameItem i in equippedItems)
        {
            multi -= i.GatherSpeedBonus;
        }
        return Math.Max(multi, 0.01);
    }
    public double GetLevelMultiplier(GameItem item)
    {
        if (item.Requirements == null || item.Requirements.Count == 0)
        {
            return 1;
        }
        string skillName = item.Requirements.FirstOrDefault(x => x.Skill != "None").Skill;
        Skill s = Skills.FirstOrDefault(x => x.Name == skillName);
        if(s == null)
        {
            return 1;
        }
        double multi = 1;
        if(s.Level < 100)
        {
            multi = 1 - (s.Level * 0.005);
        }
        else if(s.Level < 200)
        {
            multi = 1 - (0.5 + ((s.Level - 100) * 0.002));
        }
        else if(s.Level < 300)
        {
            multi = 1 - (0.7 + ((s.Level - 200) * 0.001));
        }
        else
        {
            multi = 1 - (0.8 + ((s.Level - 300) * 0.0005));
        }
        return Math.Max(multi, 0.01);
    }
    public void Equip(GameItem item)
    {
        Unequip(equippedItems.Find(x => x.EquipSlot == item.EquipSlot));
        equippedItems.Add(item);
        if(item.WeaponInfo != null)
        {
            TicksToNextAttack = GetWeaponAttackSpeed();
        }
        item.Rerender = true;
        item.IsEquipped = true;
    }
    public void Unequip(GameItem item)
    {
        if (item != null)
        {
            if (item.WeaponInfo != null)
            {
                TicksToNextAttack = GetWeaponAttackSpeed();
            }
            item.IsEquipped = false;
            item.Rerender = true;
            equippedItems.Remove(item);
        }
    }
    public int GetTotalDamage()
    {
        int total = 0;
        total += Skills.Find(x => x.Name == "Strength").GetSkillLevel() * 3;
        foreach(GameItem item in equippedItems)
        {
            if(item.WeaponInfo != null)
            {
                total += item.WeaponInfo.Damage;
                if (GetWeapon().EnabledActions == "Archery" && Inventory.HasArrows() == false)
                {
                    total += GetLevel("Strength");
                }
                else
                {
                    total += Skills.Find(x => x.Name == item.GetSkillForWeaponExp()).GetSkillLevel() * 3;
                }

            }
            if(item.ArmorInfo != null)
            {
                total += item.ArmorInfo.Damage;
            }
        }
        if(GetWeapon() != null && GetWeapon().EnabledActions == "Archery")
        {
            if (Inventory.HasArrows())
            {
                total += Inventory.GetStrongestArrow().WeaponInfo.Damage;
            }
        }
        return total;
    }
    public GameItem GetWeapon()
    {
        return equippedItems.Find(x => x.EquipSlot == "R Hand");
    }
    public int GetWeaponAttackSpeed()
    {
        GameItem weapon = GetWeapon();
        if (weapon != null && weapon.WeaponInfo != null)
        {
            return Math.Max(4, GetWeapon().WeaponInfo.AttackSpeed - (GetLevel("Deftness") / 25));
        }
        else
        {
            return Math.Max(8, 12 - (GetLevel("Deftness") / 25));
        }
    }
    public int GetLevel(string skillName)
    {
        foreach (Skill skill in Skills)
        {
            if (skill.Name == skillName)
            {
                return skill.GetSkillLevel();
            }
        }
        return 0;
    }
    public void GainExperience(Skill skill, long amount)
    {
        if (skill == null)
        {
            Console.WriteLine("Player gained " + amount + " experience in unfound skill.");
            return;
        }
        if (amount <= 0)
        {
            return;
        }
        LastGainedExp = skill;
        double multi = 1;
        foreach(GameItem i in equippedItems)
        {
            if(i.ExperienceBonusSkill == skill.Name)
            {
                multi += i.ExperienceGainBonus;
            }
        }
        skill.Experience += (long)(amount * multi);
        

        if (skill.Experience >= (long)Skill.GetExperienceRequired(skill.GetSkillLevelUnboosted()))
        {
            LevelUp(skill);
        }
    }
    public void GainExperienceFromWeapon(GameItem weapon, int damageDealt)
    {
        if (weapon.EnabledActions == null)
        {
            return;
        }
        if (weapon.EnabledActions.Contains("Knife"))
        {
            GainExperience("Deftness", (int)(damageDealt * 1.5));
            GainExperience("Knifesmanship", (int)(damageDealt));
        }
        else if (weapon.EnabledActions.Contains("Sword"))
        {
            GainExperience("Deftness", (int)(damageDealt * 0.5));
            GainExperience("Strength", damageDealt);
            GainExperience("Swordsmanship", (int)(damageDealt));
        }
        else if (weapon.EnabledActions.Contains("Axe"))
        {
            GainExperience("Deftness", (int)(damageDealt * 0.5));
            GainExperience("Strength", damageDealt);
            GainExperience("Axemanship", (int)(damageDealt));
        }
        else if (weapon.EnabledActions.Contains("Hammer"))
        {
            GainExperience("Strength", (int)(damageDealt * 1.5));
            GainExperience("Hammermanship", (int)(damageDealt));
        }
        else if (weapon.EnabledActions.Contains("Archery"))
        {

                GainExperience("Archery", (int)(damageDealt * 1.5));

        }
        else if (weapon.EnabledActions.Contains("Fishing"))
        {
            GainExperience("Fishing", (int)(damageDealt * 0.1));
        }
    }
    public void LevelUp(Skill skill)
    {
        skill.SetSkillLevel(skill.GetSkillLevelUnboosted() + 1);
        MessageManager.AddMessage("You leveled up! Your " + skill.Name + " level is now " + skill.GetSkillLevelUnboosted() + ".");
        if (skill.Name == "Strength")
        {
            Inventory.IncreaseMaxSizeBy(1);

            if (skill.GetSkillLevelUnboosted() % 10 == 0)
            {
                Inventory.IncreaseMaxSizeBy(4);
                MessageManager.AddMessage("You feel stronger. You can now carry 5 more items in your inventory.");
            }
            else
            {
                MessageManager.AddMessage("You feel stronger. You can now carry 1 more item in your inventory.");
            }
        }
        else if (skill.Name == "HP")
        {
            IncreaseMaxHPBy(5);
            if (skill.GetSkillLevelUnboosted() % 5 == 0)
            {
                IncreaseMaxHPBy(10);
                MessageManager.AddMessage("You feel much healthier. Your maximum HP has increased by 15!");
            }
            else
            {
                MessageManager.AddMessage("You feel healthier. Your maximum HP has increased by 5.");
            }
        }

        if (skill.Experience >= Skill.GetExperienceRequired(skill.GetSkillLevelUnboosted()))
        {
            LevelUp(skill);

        }
    }
    public void Die()
    {
        CurrentHP = MaxHP;
        JustDied = true;
        Deaths++;
        if(Deaths == 1)
        {
            MessageManager.AddMessage("Whoops! Looks like you died. Don't worry, you don't lose anything but pride when you die in Quepland.");
        }
        else
        {
            MessageManager.AddMessage("Whoops! Looks like you died.");
        }
        if (BattleManager.Instance.CurrentDojo != null)
        {
            BattleManager.Instance.CurrentDojo.CurrentOpponent = 0;
            BattleManager.Instance.CurrentDojo.HasBegunChallenge = false;
            BattleManager.Instance.CurrentDojo = null;
        }
        BattleManager.Instance.EndBattle();
    }
    public bool FollowerGatherItem(GameItem item)
    {
        if (CurrentFollower != null && CurrentFollower.IsBanking == false)
        {
            if (CurrentFollower.Inventory.GetAvailableSpaces() <= 0)
            {
                CurrentFollower.IsBanking = true;
                CurrentFollower.TicksToNextAction = CurrentFollower.AutoCollectSpeed;
                MessageManager.AddMessage(CurrentFollower.AutoCollectMessage.Replace("$", item.Name));
                return false;
            }
            else if(CurrentFollower.MeetsRequirements(item))
            {
                CurrentFollower.Inventory.AddItem(item.Copy());
                GainExperience(item.ExperienceGained);
                MessageManager.AddMessage(item.GatherString);
                return true;
            }
            else
            {
                if(MessageManager.GetMessages().Any(x => x.Text.Contains(CurrentFollower.Name + " is unable to carry ")) == false)
                {
                    MessageManager.AddMessage(CurrentFollower.Name + " is unable to carry " + item.Name + ".");
                }
                
                return false;
            }

        }
        return false;
    }
    public bool PlayerGatherItem(GameItem item)
    {
        if (Inventory.AddItem(item.Copy()) == false)
        {
            if (CurrentFollower != null && CurrentFollower.IsBanking)
            {
                MessageManager.AddMessage("Your inventory is full. You wait for your follower to return from banking.");
            }
            else
            {
                MessageManager.AddMessage("Your inventory is full.");
            }
            return false;
        }
        else
        {
            GainExperience(item.ExperienceGained);
            MessageManager.AddMessage(item.GatherString);
        }
        return true;
    }
    public List<GameItem> GetEquippedItems()
    {
        return equippedItems;
    }

    public bool HasSkillRequirement(string skill, int lvl)
    {
        Skill s = Skills.FirstOrDefault(x => x.Name == skill);
        if (s == null)
        {
            Console.WriteLine("Failed to find game item:" + skill);
            return false;
        }
        return s.Level >= lvl;
    }
    public bool HasToolRequirement(GameItem item)
    {
        return Inventory.HasToolRequirement(item);
    }
    public bool HasToolRequirement(string action)
    {
        return Inventory.HasToolRequirement(action);
    }
}

