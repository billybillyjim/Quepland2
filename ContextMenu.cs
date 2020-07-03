using System;
using System.Collections.Generic;

public class ContextMenu
{
	public List<ContextButton> Buttons = new List<ContextButton>();

	public double GetHeight()
    {
        double h = 20;
        foreach(ContextButton button in Buttons)
        {
            h += (button.ActionText.Length / 6) * 18;
        }
        return h;
    }
}
