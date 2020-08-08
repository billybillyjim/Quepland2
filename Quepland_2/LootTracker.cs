using System;

public class LootTracker
{
	private static readonly LootTracker instance = new LootTracker();
	private LootTracker() { }
	static LootTracker() { }
	public static LootTracker Instance
    {
        get
        {
            return instance;
        }
    }
    public Inventory Inventory { get; set; } = new Inventory(int.MaxValue);
    public bool TrackLoot { get; set; }
}
