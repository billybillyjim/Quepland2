public class Tooltip
{
    public string Name { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public bool RightAlignData { get; set; }
    public bool ShowAbove { get; set; }
    public string Alignment { get
        {
            if (RightAlignData)
            {
                return "right";
            }
            else
            {
                return "left";
            }
        } 
    }
    public Tooltip()
    {

    }
    public Tooltip(string name, string title, string data)
    {
        Name = name;
        Title = title;
        Text = data;
    }
    
}

