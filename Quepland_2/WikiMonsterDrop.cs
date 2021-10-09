using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quepland_2
{
    public class WikiMonsterDrop
    {
        public WikiMonsterDrop(Monster m, Drop d)
        {
            monster = m;
            drop = d;
        }
        public Monster monster;
        public Drop drop;
    }
}
