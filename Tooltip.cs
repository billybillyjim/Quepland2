public class Tooltip
{
    public string Name { get; set; }
    public string TitleText { get; set; }
    public string DataText { get; set; }
    public bool RightAlignData { get; set; }
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
        TitleText = title;
        DataText = data;
    }
    
}

