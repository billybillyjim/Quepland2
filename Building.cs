using System;
using System.Collections.Generic;

public class Building
{
	public string Name { get; set; }
	private string _buttonText;
	public string ButtonText { get { if (_buttonText == null) { return Name; } return _buttonText; } set { _buttonText = value; } }
	public string Description { get; set; }
	public string URL { get; set; }
	public List<string> NPCs { get; set; } = new List<string>();
	public List<Shop> Shops { get; set; } = new List<Shop>();
}
