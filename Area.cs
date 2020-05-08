﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class Area
{
    public string Name { get; set; } = "Unset";
    private string _areaURL;
    public string AreaURL { get
        {
            if(_areaURL != null)
            {
                return _areaURL;
            }
            return Name;
        }
        set
        {
            _areaURL = value;
        }
    }
    public int ID { get; set; }
    public string Image { get; set; } = "NoImage";
    public string Description { get; set; } = "This place is indescribable... Or maybe the dev just forgot to describe it.";
    public bool IsUnlocked { get; set; }
    public bool IsHidden { get; set; }
    public List<string> Actions { get; set; } = new List<string>();

}

