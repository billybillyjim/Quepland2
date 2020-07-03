using System;

public class ContextButton
{
	public string ActionText { get; set; } = "Unset";
	public string Color { get; set; } = "#282828";
	public Action OnClick { get; set; }

	public ContextButton(string text, Action click)
    {
		ActionText = text;
		OnClick = click;
    }
}
