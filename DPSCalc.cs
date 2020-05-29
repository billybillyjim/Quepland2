using System;
using System.Collections.Generic;

public class DPSCalc
{

	public static int NumOfBattles = 100;
	public List<Monster> Opponents = new List<Monster>();
    public int TotalTicksTaken = 0;
    public double AverageKillTime = 0;
	public void CalculateDPS()
    {
        TotalTicksTaken = 0;
		for(int i = 0; i < NumOfBattles; i++)
        {
            foreach(Monster o in Opponents)
            {
                o.CurrentHP = o.HP;
            }
            BattleManager.Instance.StartBattle(Opponents);
            while (BattleManager.Instance.BattleHasEnded == false)
            {
                BattleManager.Instance.DoBattle();
                TotalTicksTaken++;
            }

        }
        AverageKillTime = (double)TotalTicksTaken / NumOfBattles;
    }
}
