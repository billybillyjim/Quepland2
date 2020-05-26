using Microsoft.AspNetCore.Components;
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
    public Monster CurrentOpponent;
    public Area CurrentArea;
    public bool BattleHasEnded;
    private static readonly Random random = new Random();
    public async Task LoadMonsters(HttpClient Http)
    {
        Monsters.AddRange(await Http.GetJsonAsync<Monster[]>("data/Monsters/Overworld.json"));
    }
    public void StartBattle()
    {
        if (CurrentOpponent == null)
        {
            return;
        }
        else
        {
            CurrentOpponent.CurrentHP = CurrentOpponent.HP;
            CurrentOpponent.TicksToNextAttack = CurrentOpponent.AttackSpeed;
            BattleHasEnded = false;
        }

    }
    public void StartBattle(Area area)
    {
        CurrentArea = area;
        int r = random.Next(0, CurrentArea.Monsters.Count);
        CurrentOpponent = Monsters.FirstOrDefault(x => x.Name == CurrentArea.Monsters[r]);
        if(CurrentOpponent == null)
        {
            Console.WriteLine("No monsters found for area.");
        }
        StartBattle();
        
    }
    public void DoBattle()
    {
        if(BattleHasEnded == false)
        {
            CurrentOpponent.TicksToNextAttack--;
            Player.Instance.TicksToNextAttack--;
            if (Player.Instance.TicksToNextAttack < 0)
            {
                Attack();
                Player.Instance.TicksToNextAttack = Player.Instance.GetWeaponAttackSpeed();
            }

            if (CurrentOpponent.CurrentHP <= 0)
            {
                CurrentOpponent.CurrentHP = 0;
                MessageManager.AddMessage("You defeated the " + CurrentOpponent.Name + ".");
                Player.Instance.Inventory.AddItems(CurrentOpponent.DropTable.GetAlwaysDrops());

                Player.Instance.Inventory.AddDrop(CurrentOpponent.DropTable.GetDrop());

                EndBattle();
            }
            if (CurrentOpponent.TicksToNextAttack < 0)
            {
                BeAttacked();
                CurrentOpponent.TicksToNextAttack = CurrentOpponent.AttackSpeed;
            }
            if (Player.Instance.CurrentHP <= 0)
            {
                Player.Instance.CurrentHP = Player.Instance.MaxHP;
                MessageManager.AddMessage("Whoops! Looks like you died. Don't worry, you don't lose anything but pride when you die in Quepland.");
                EndBattle();
            }
        }

    }

    public void Attack()
    {
        int total = Math.Min(Player.Instance.GetTotalDamage().ToRandomDamage(), CurrentOpponent.CurrentHP);
        CurrentOpponent.CurrentHP -= total;
        if(Player.Instance.GetWeapon() == null)
        {
            Player.Instance.GainExperience("Strength", total * 2);
        }
        else
        {
            Player.Instance.GainExperience(Player.Instance.GetWeapon().GetSkillForWeaponExp(), total * 2);
        }
        
        MessageManager.AddMessage("You hit the " + CurrentOpponent.Name + " for " + total + " damage!");

    }
    public void BeAttacked()
    {
        int total = CurrentOpponent.Damage.ToRandomDamage();
        Player.Instance.CurrentHP -= total;
        Player.Instance.GainExperience("HP", total);
        MessageManager.AddMessage("The " + CurrentOpponent.Name + " hit you for " + total + " damage!");
    }
    public void EndBattle()
    {
        Console.WriteLine("Ending Battle");
        BattleHasEnded = true;
    }

    public Monster GetMonsterByName(string name)
    {
        return Monsters.FirstOrDefault(x => x.Name == name);
    }
    
}

