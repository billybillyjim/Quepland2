using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Converters;
using Quepland_2.Bosses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class BattleManager
{
    private static readonly BattleManager instance = new BattleManager();
    private BattleManager() { }
    static BattleManager() { }
    public static BattleManager Instance { get { return instance; } }
    public List<Monster> Monsters = new List<Monster>();
    public List<Monster> CurrentOpponents { get; set; } = new List<Monster>();
    public IBoss CurrentBoss { get; set; }
    public Monster Target { get; set; }
    public Area CurrentArea;
    public bool BattleHasEnded = true;
    private static readonly Random random = new Random();
    public async Task LoadMonsters(HttpClient Http)
    {
        Monsters.AddRange(await Http.GetJsonAsync<Monster[]>("data/Monsters/Overworld.json"));
        Monsters.AddRange(await Http.GetJsonAsync<Monster[]>("data/Monsters/Bosses.json"));
    }
    public void StartBattle()
    {
        if (CurrentOpponents == null || CurrentOpponents.Count == 0)
        {
            Console.WriteLine("Opponents were null or nonexistent.");
            return;
        }
        else
        {
            foreach(Monster monster in CurrentOpponents)
            {
                monster.CurrentHP = monster.HP;
                monster.TicksToNextAttack = monster.AttackSpeed;
                monster.IsDefeated = false;
            }
            BattleHasEnded = false;
        }

    }
    public void StartBattle(Area area)
    {
        CurrentArea = area;
        int r = random.Next(0, CurrentArea.Monsters.Count);
        CurrentOpponents.Clear();
        CurrentOpponents.Add(Monsters.FirstOrDefault(x => x.Name == CurrentArea.Monsters[r]));
        if(CurrentOpponents[0] == null)
        {
            Console.WriteLine("No monsters found for area.");
        }
        else
        {
            foreach(Monster m in CurrentOpponents)
            {
                m.IsDefeated = false;
            }
           
        }
        StartBattle();
        
    }
    public void StartBattle(List<Monster> opponents)
    {
        CurrentOpponents.Clear();
        CurrentOpponents.AddRange(opponents);
        StartBattle();
    }
    public void DoBattle()
    {
        if(BattleHasEnded == false)
        {
            foreach(Monster opponent in CurrentOpponents)
            {
                if(opponent.IsDefeated)
                {
                    opponent.TicksToNextAttack = opponent.AttackSpeed;
                }
                else
                {
                    opponent.TicksToNextAttack--;
                }
            }
            Player.Instance.TicksToNextAttack--;
            if (Player.Instance.TicksToNextAttack < 0)
            {
                Attack();
                if (CurrentBoss != null)
                {
                    CurrentBoss.OnBeAttacked(Target);
                }
                Player.Instance.TicksToNextAttack = Player.Instance.GetWeaponAttackSpeed();
            }
            foreach(Monster opponent in CurrentOpponents)
            {
                if (opponent.CurrentHP <= 0 && opponent.IsDefeated == false)
                {
                    opponent.CurrentHP = 0;                   
                    List<GameItem> alwaysDrops = opponent.DropTable.GetAlwaysDrops();
                    Drop drop = opponent.DropTable.GetDrop();

                    if (LootTracker.Instance.TrackLoot)
                    {
                        LootTracker.Instance.Inventory.AddItems(alwaysDrops);
                        LootTracker.Instance.Inventory.AddDrop(drop);
                    }
                    else
                    {
                        MessageManager.AddMessage("You defeated the " + opponent.Name + ".");
                        Player.Instance.Inventory.AddItems(alwaysDrops);
                        Player.Instance.Inventory.AddDrop(drop);
                    }
                    
                    opponent.IsDefeated = true;
                    if(CurrentBoss != null)
                    {
                        CurrentBoss.OnDie(opponent);
                    }
                }
                else if (opponent.TicksToNextAttack < 0 && opponent.IsDefeated == false)
                {
                    if (CurrentBoss != null)
                    {
                        CurrentBoss.OnAttack();
                    }
                    BeAttacked(opponent);
                    opponent.TicksToNextAttack = opponent.AttackSpeed;
                }
            }
            if (AllOpponentsDefeated())
            {
                EndBattle();
            }
            if (CurrentBoss != null)
            {
                CurrentBoss.TicksToNextSpecialAttack--;
                if(CurrentBoss.TicksToNextSpecialAttack <= 0)
                {
                    CurrentBoss.OnSpecialAttack();
                }
                
            }

            if (Player.Instance.CurrentHP <= 0)
            {
                Player.Instance.Die();
            }

        }

    }

    public void Attack()
    {
        if(Target == null || Target.IsDefeated)
        {
            if (CurrentOpponents == null || CurrentOpponents.Count == 0)
            {
                return;
            }
            Target = GetNextTarget();
        }
        int total = (int)Math.Min(Player.Instance.GetTotalDamage().ToRandomDamage() * Extensions.CalculateArmorDamageReduction(Target), Target.CurrentHP);
        Target.CurrentHP -= total;
        if(Player.Instance.GetWeapon() == null)
        {
            Player.Instance.GainExperience("Strength", total);
            MessageManager.AddMessage("You punch the " + Target.Name + " for " + total + " damage!");
        }
        else
        {
            if(Player.Instance.GetWeapon().EnabledActions == "Archery" && Player.Instance.Inventory.HasArrows() == false)
            {
                Player.Instance.GainExperience("Strength", total);
                MessageManager.AddMessage("You whack the " + Target.Name + " with your bow for " + total + " damage!");
            }
            else
            {
                Player.Instance.GainExperienceFromWeapon(Player.Instance.GetWeapon(), total);
                MessageManager.AddMessage("You hit the " + Target.Name + " for " + total + " damage!");
            }
            
        }
        
        
        if (Target.IsDefeated)
        {
            Target = GetNextTarget();
        }
    }
    public void BeAttacked(Monster opponent)
    {
        int total = (int)(opponent.Damage.ToRandomDamage() * Extensions.CalculateArmorDamageReduction());
        Player.Instance.CurrentHP -= total;
        Player.Instance.GainExperience("HP", total);
        MessageManager.AddMessage("The " + opponent.Name + " hit you for " + total + " damage!");
    }
    public bool AllOpponentsDefeated()
    {
        if(CurrentOpponents == null || CurrentOpponents.Count == 0)
        {
            return true;
        }
        foreach(Monster opponent in CurrentOpponents)
        {
            if(opponent.IsDefeated == false)
            {
                return false;
            }
        }
        return true;
    }
    public void SetBoss(Quepland_2.Bosses.IBoss boss)
    {
        CurrentBoss = boss;
        CurrentBoss.Monsters = CurrentOpponents;
    }
    public void EndBattle()
    {
        BattleHasEnded = true;
        CurrentBoss = null;
    }
    private Monster GetNextTarget()
    {
        if(CurrentOpponents == null || CurrentOpponents.Count == 0)
        {
            return null;
        }
        foreach(Monster m in CurrentOpponents)
        {
            if(m.IsDefeated == false)
            {
                return m;
            }
        }
        return null;
    }

    public Monster GetMonsterByName(string name)
    {
        return Monsters.FirstOrDefault(x => x.Name == name);
    }
    
}

