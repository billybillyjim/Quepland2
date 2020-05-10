using System;
using System.Collections.Generic;

public class NPC
{
	public string Name { get; set; }
	public int ID { get; set; }
	public List<Dialog> Dialogs { get; set; } = new List<Dialog>();
}
