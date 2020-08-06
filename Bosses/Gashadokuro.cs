using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quepland_2.Bosses
{
    public class Gashadokuro : IBoss
    {
        public void OnDie(Monster monster) 
        {
            foreach(Monster m in Monsters)
            {
                if(m.Name == "Gashadokuro Spine" && monster.Name != "Gashadokuro Spine")
                {
                    m.Armor -= 46;
                    MessageManager.AddMessage("A bit of the creature's ribcage crumbles away, exposing more of the spine!");
                    Console.WriteLine("New Armor:" + m.Armor);
                }
            }
        }
        public void OnAttack() { }
        public void OnSpecialAttack() 
        {
            Player.Instance.TicksToNextAttack = Player.Instance.GetWeaponAttackSpeed();
            MessageManager.AddMessage("The Gashadokuro slams the bridge, you lose your balance and need to recover before attacking!", "red");
            TicksToNextSpecialAttack = SpecialAttackSpeed;
        }
        public void OnBeAttacked(Monster monster) 
        {
            if(monster.Name == "Gashadokuro Spine")
            {
                if(monster.Armor == 140)
                {
                    MessageManager.AddMessage("You try to attack the spine, but the ribcage blocks most of the damage!", "red");
                }               
            }
            

        }
        public int SpecialAttackSpeed { get; set; } = 150;
        public int TicksToNextSpecialAttack { get; set; } = 150;
        public List<Monster> Monsters {get;set;}
        public bool CustomAttacks { get; set; } = false;
    }
}
