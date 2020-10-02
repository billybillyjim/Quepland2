using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
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
    private Follower currentFollower;
    public Follower CurrentFollower { get { return currentFollower; } }
    public int MaxHP = 50;
    public int CurrentHP;
    public int TicksToNextAttack { get; set; }
    public int Deaths { get; set; }
    public int ArtisanPoints { get; set; }
    public bool JustDied { get; set; }

    public Skill LastGainedExp { get; set; }
    public Skill ExpTrackerSkill { get; set; }
    public List<IStatusEffect> CurrentStatusEffects { get; set; } = new List<IStatusEffect>();
    
    public void SetFollower(Follower f)
    {
        BattleManager.Instance.AutoBattle = false;
        BattleManager.Instance.SelectedOpponent = null;
        currentFollower = f;

    }

    public async Task LoadSkills(HttpClient Http)
    {
        Skills.AddRange(await Http.GetFromJsonAsync<Skill[]>("data/Skills.json"));
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
        string skill = item.Requirements.FirstOrDefault(x => x.Skill != "None")?.Skill ?? "None";
        if(skill == "None")
        {
            return 1;
        }
        double multi = 1;
        foreach(GameItem i in equippedItems)
        {
            if (i.EnabledActions.Contains(skill))
            {
                multi -= i.GatherSpeedBonus;
            }         
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
        if(s.GetSkillLevel() < 100)
        {
            multi = 1 - (s.GetSkillLevel() * 0.005);
        }
        else if(s.GetSkillLevel() < 200)
        {
            multi = 1 - (0.5 + ((s.GetSkillLevel() - 100) * 0.002));
        }
        else if(s.GetSkillLevel() < 300)
        {
            multi = 1 - (0.7 + ((s.GetSkillLevel() - 200) * 0.001));
        }
        else
        {
            multi = 1 - (0.8 + ((s.GetSkillLevel() - 300) * 0.0005));
        }
        return Math.Max(multi, 0.01);
    }
    public void Equip(GameItem item)
    {
        GameItem e = equippedItems.FirstOrDefault(x => x.EquipSlot == item.EquipSlot);
        if(e != null)
        {
            Unequip(e);
        }
        equippedItems.Add(item);
        if(item.WeaponInfo != null)
        {
            TicksToNextAttack = GetWeaponAttackSpeed();
        }
        item.Rerender = true;
        item.IsEquipped = true;
    }
    public void Equip(string itemName)
    {
        GameItem match = Inventory.GetItems().FirstOrDefault(x => x.Key.Name == itemName).Key;
        if(match != null)
        {
            Equip(match);
        }
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
                    string skill = item.GetSkillForWeaponExp();
                    if (skill == "")
                    {
                        total += GetLevel("Strength");
                    }
                    else 
                    {
                        total += Skills.Find(x => x.Name == skill).GetSkillLevel() * 3;
                    }
                    
                }

            }
            if(item.ArmorInfo != null)
            {
                total += item.ArmorInfo.Damage;
            }
        }
        if(GetWeapon() != null)
        {
            if(GetWeapon().Name == "Spine Shooter")
            {
                if(Inventory.HasItem("Cactus Spines"))
                {
                    total += 10;
                }
            }
            else if (GetWeapon().EnabledActions == "Archery" && Inventory.HasArrows())
            {
                total += Inventory.GetStrongestArrow().WeaponInfo.Damage;                        
            }

        }
        return Math.Max(1, total);
    }
    public void ClearBoosts()
    {
        foreach(Skill s in Skills)
        {
            s.Boost = 0;
        }
    }
    public int PayArtisanPoints(int amountToPay)
    {
        if(ArtisanPoints >= amountToPay)
        {
            ArtisanPoints -= amountToPay;
        }
        else
        {
            return 0;
        }
        return amountToPay;
    }
    public int GetTotalLevel()
    {
        return Skills.Select(x => x.Level).Sum();
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
        else if (weapon.EnabledActions.Contains("Mining"))
        {
            GainExperience("Hammermanship", (int)(damageDealt * 0.1));
            GainExperience("Strength", (int)(damageDealt * 0.5));
            GainExperience("Mining", (int)(damageDealt * 0.08));
        }
        else
        {
            GainExperience("Strength", (int)(damageDealt * 0.5));
            GainExperience("Deftness", (int)(damageDealt * 0.5));
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
                Inventory.IncreaseMaxSizeBy(1);
                MessageManager.AddMessage("You feel much stronger. You can now carry 2 more items in your inventory.");
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
        if(GameState.CurrentGameMode == GameState.GameType.Hardcore)
        {
            MessageManager.AddMessage("Whoops! Looks like you died. As a hardcore account your journey is now over. Better luck next time!");
            JustDied = true;
            GameState.ShowStartMenu = true;
        }
        else
        {
            CurrentHP = MaxHP;
            JustDied = true;
            Deaths++;
            CurrentStatusEffects.Clear();
            if (Deaths == 1)
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

    }
    public bool FollowerGatherItem(GameItem item)
    {
        if (CurrentFollower != null && CurrentFollower.IsBanking == false)
        {
            if(CurrentFollower.InventorySize == 0)
            {
                return false;
            }
            if (CurrentFollower.Inventory.GetAvailableSpaces() <= 0)
            {
                CurrentFollower.SendToBank();
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
        if(item == null)
        {
            return false;
        }
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
    public GameItem GetItemInSlot(string slot)
    {
        return equippedItems.FirstOrDefault(x => x.EquipSlot == slot);
    }
    public bool HasSkillRequirement(string skill, int lvl)
    {
        Skill s = Skills.FirstOrDefault(x => x.Name == skill);
        if (s == null)
        {
            Console.WriteLine("Failed to find skill:" + skill);
            return false;
        }
        return s.GetSkillLevel() >= lvl;
    }
    public bool HasToolRequirement(GameItem item)
    {
        return Inventory.HasToolRequirement(item);
    }
    public bool HasToolRequirement(string action)
    {
        return Inventory.HasToolRequirement(action);
    }
    public bool HasStatusEffect(string name)
    {
        return CurrentStatusEffects.Any(x => x.Name == name);
    }
    public void AddStatusEffect(IStatusEffect effect)
    {
        CurrentStatusEffects.Add(effect.Copy());
    }
    public void TickStatusEffects()
    {
        List<IStatusEffect> endedEffects = new List<IStatusEffect>();
        foreach(IStatusEffect effect in CurrentStatusEffects)
        {
            effect.RemainingTime--;
            if(effect.RemainingTime <= 0)
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
    public void ResetStats()
    {
        foreach(Skill s in Skills)
        {
            s.SetSkillLevel(0);
            s.Experience = 0;
        }
    }
    public PlayerSaveData GetSaveData()
    {
        List<string> equipped = new List<string>();
        try
        {
            if(equippedItems.Count > 0) 
            {
                equipped = equippedItems.Select(x => x.Name).ToList();
            }
            
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            Console.WriteLine("Failed to set equipped items list.");
        }
        return new PlayerSaveData {
            ActiveFollowerName = CurrentFollower?.Name ?? "None",
            CurrentHP = CurrentHP,
            MaxHP = MaxHP,
            DeathCount = Deaths,
            ArtisanPoints = ArtisanPoints,
            InventorySize = Inventory.GetSize(),
            EquippedItems = equipped
        };
    }
    public void LoadSaveData(PlayerSaveData data)
    {
        if(data.ActiveFollowerName != "None")
        {
           SetFollower(FollowerManager.Instance.GetFollowerByName(data.ActiveFollowerName));
        }
        CurrentHP = data.CurrentHP;
        CalculateMaxHP();
        Deaths = data.DeathCount;
        ArtisanPoints = data.ArtisanPoints;
        CalculateInventorySpaces();
        try
        {
            if(data.EquippedItems.Count == 0)
            {
                return;
            }
            foreach (string s in data.EquippedItems)
            {
                Console.WriteLine("Trying to equip:" + s);
                if (s != null && s.Length > 1)
                {
                    Equip(Inventory.GetItems().FirstOrDefault(x => x.Key.Name == s).Key);
                    
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);

        }
    }
    private void CalculateMaxHP()
    {
        int hp = 50;
        for(int i = 1; i < GetLevel("HP"); i++)
        {
            if(i % 5 == 0)
            {
                hp += 10;
            }
            hp += 5;
        }
        MaxHP = hp;
    }
    private void CalculateInventorySpaces()
    {
        int spaces = 30;
        for(int i = 1; i < GetLevel("Strength"); i++)
        {
            if(i % 10 == 0)
            {
                spaces++;
            }
            spaces++;
        }
        Inventory.SetSize(spaces);
    }
}

