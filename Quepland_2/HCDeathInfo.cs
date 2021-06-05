using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class HCDeathInfo
{
    public List<Skill> FinalLevels { get; set; } = new List<Skill>();
    public int TotalPlaytime { get; set; }
    public string CauseOfDeath { get; set; } = "";
}

