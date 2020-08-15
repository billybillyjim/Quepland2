using System;

public class Book
{
	public Skill Skill { get; set; }
	public int Difficulty { get; set; }
	public int Length { get; set; }
    public int Progress { get; set; }
    private string desc;
	public string Description
    {
        get
        {
            if(desc == null)
            {
                string[] sizes = { "very thin ", "thin ", "", "thick ", "very thick " };
                string cover = ItemManager.Instance.GetBookCover(Skill);
                desc = "a " + sizes[Difficulty] + "book with " + cover + " on the cover.";
            }
            return desc;
        }
    }
    public Book(Skill skill, int difficulty, int lengthModifier)
    {
        Skill = skill;
        Difficulty = difficulty;
        Length = Math.Max(10, (difficulty * 100) + lengthModifier);
    }
}
