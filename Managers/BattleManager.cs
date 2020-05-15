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
        Monsters.AddRange(await Http.GetJsonAsync<Monster[]>("data/Monsters.json"));
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
        StartBattle();
        
    }
    public void DoBattle()
    {
        Console.WriteLine("Doing battle");
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
                EndBattle();
            }
        }

    }

    public void Attack()
    {
        CurrentOpponent.CurrentHP -= Player.Instance.GetTotalDamage();
    }
    public void BeAttacked()
    {
        Player.Instance.CurrentHP -= CurrentOpponent.Damage;
    }
    public void EndBattle()
    {
        BattleHasEnded = true;
    }

    public Monster GetMonsterByName(string name)
    {
        return Monsters.FirstOrDefault(x => x.Name == name);
    }

}

