using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quepland_2.Bosses
{
    public interface IBoss
    {
        void OnAttack();
        void OnBeAttacked(Monster monster);
        void OnDie(Monster monster);
        void OnSpecialAttack();
        List<Monster> Monsters { get; set; }
        int SpecialAttackSpeed { get; set; }
        int TicksToNextSpecialAttack { get; set; }
        bool CustomAttacks { get; set; }
    }
}
