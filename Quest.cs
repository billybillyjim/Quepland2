using System;
using System.Collections.Generic;

public class Quest
{
	public string Name { get; set; }
	public int Progress { get; set; }
	public List<Requirement> Requirements = new List<Requirement>();
}
