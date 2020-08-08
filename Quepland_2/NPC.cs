using System;
using System.Collections.Generic;
using System.Linq;

public class NPC
{
	public string Name { get; set; }
	public int ID { get; set; }
	public int ConversationDepth { get; set; }
	public List<Dialog> Dialogs { get; set; } = new List<Dialog>();
	public Shop Shop { get; set; }

	public int AvailableDialogCount()
    {
		return Dialogs.Where(x => x.HasRequirements()).ToList().Count;
    }
	public List<Dialog> GetDialogsAtDepth(int depth)
    {
		return Dialogs.Where(x => x.Depth == depth).ToList();
    }
}
