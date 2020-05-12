using System;

public class Ingredient
{
	public string Item { get; set; }
	public int Amount { get; set; } = 1;
	public bool DestroyOnUse { get; set; } = true;
}
