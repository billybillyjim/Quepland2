using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Player
{
    private static readonly Player instance = new Player();
    private Player() { }
    static Player() { }
    public static Player Instance
    {
        get
        {
            return instance;
        }
    }
    public Inventory Inventory = new Inventory(30);
}

