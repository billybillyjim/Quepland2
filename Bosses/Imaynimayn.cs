using Quepland_2.Bosses.ImaynimaynElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quepland_2.Bosses
{
    public class Imaynimayn : IBoss
    {
        public void OnDie(Monster monster)
        {
            Drop d = ItemManager.Instance.GetMinigameDropTable("Sotu Nran Waterfall").DropTable.GetDrop();
            MessageManager.AddMessage("The creature screams in rage and disappears under the water. The waterfall opens up to reveal a pile of treasure. You grab " + d.Amount + " " + d.Item + " quickly and escape before the creature returns.");
            Player.Instance.Inventory.AddDrop(d);
        }
        public void OnAttack() 
        {
            currentTick++;
            foreach(Lilypad pad in Lilypads)
            {
                pad.Tick();
                if(PlayerPosition == pad.Position && pad.HasFallen)
                {
                    MessageManager.AddMessage("The water feels like its draining your life force!", "red");
                    Player.Instance.CurrentHP -= (Player.Instance.MaxHP / 5);
                    Monsters[0].CurrentHP += Player.Instance.MaxHP / 4;
                }
            }
            if(currentTick % attackRatio == 0)
            {
                BattleManager.Instance.BeAttacked(BattleManager.Instance.GetMonsterByName("Imaynimayn"));
            }
        }
        public void OnSpecialAttack()
        {
            Player.Instance.CurrentHP -= (Player.Instance.MaxHP / 7);
            Monsters[0].CurrentHP += Player.Instance.MaxHP / 7;
            MessageManager.AddMessage("The creature roars and draws your life away.");
            if(NextLilypadTarget != null)
            {
                if(Monsters[0].CurrentHP % 2 == 0)
                {
                    NextLilypadTarget.Fall = true;
                    NextLilypadTarget.CurrentTick = NextLilypadTarget.TicksToFall;
                    MessageManager.AddMessage("You feel the lilypad beneath your feet begin to tremble.", "red");
                }
            }
            TicksToNextSpecialAttack = SpecialAttackSpeed;
            
        }
        public void OnBeAttacked(Monster monster)
        {

        }
        public int SpecialAttackSpeed { get; set; } = 25;
        public int TicksToNextSpecialAttack { get; set; } = 25;
        private int attackRatio = 13;
        private int currentTick = 0;
        public List<Monster> Monsters { get; set; }
        public Lilypad NextLilypadTarget;
        public List<Lilypad> Lilypads = new List<Lilypad>();
        public string PlayerPosition { get; set; } = "Top";
        public bool CustomAttacks { get; set; } = true;
    }
}
