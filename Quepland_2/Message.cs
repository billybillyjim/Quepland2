using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Message
{
    public string Text { get; set; }
    public string Color { get; set; }
    public string Style { get; set; } = "";
    public string Tag { get; set; }
    public Message(string message)
    {
        Text = message;
    }
    public Message(string message, string color, string tag)
    {
        Text = message;
        Color = color;
        Tag = tag;
    }
}

