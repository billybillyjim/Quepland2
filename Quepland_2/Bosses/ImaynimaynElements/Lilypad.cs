using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quepland_2.Bosses.ImaynimaynElements
{
    public class Lilypad
    {
        public string Position { get; set; }
        public int TicksToFall { get; set; } = 2;
        public bool HasFallen { get; set; }
        public bool Fall { get; set; }
        public int TicksToRise { get; set; } = 15;
        public int CurrentTick { get; set; }

        public Lilypad(string name)
        {
            Position = name;
        }
        public void Tick()
        {
            if (HasFallen)
            {
                CurrentTick++;
                if(CurrentTick >= TicksToRise)
                {
                    HasFallen = false;
                }
            }
            else
            {
                if (Fall)
                {
                    CurrentTick--;
                    if(CurrentTick <= 0)
                    {
                        MessageManager.AddMessage("The " + Position + " lilypad falls under the water!", "red");
                        HasFallen = true;
                        Fall = false;
                    }
                }
            }
        }
    }
}
