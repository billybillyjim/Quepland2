using System;
using System.Text.Json.Serialization;

public class Skill
{

    public string Name { get; set; }

    public int Level { get; set; }
    private long _experience;

    public long Experience {
        get { return _experience; }
        set {
            if (value >= 0)
            {
                _experience = Math.Min(value, long.MaxValue - 20000000);
            }
            else
            {
                _experience = long.MaxValue - 20000000;
            }
        }
    }
    public string Description { get; set; }
    public int Boost { get; set; }
    public double Progress
    {
        get
        {
            double expLastLevel = GetExperienceRequired(Level - 1);
            double expToLevel = GetExperienceRequired(Level) - expLastLevel;
            double expProgress = _experience - expLastLevel;

            return ((expProgress / expToLevel) * 100);
            
        }
    }
    public static double GetExperienceRequired(long level)
    {
        double exp = 0;

        for (int i = 0; i < level; i++)
        {
            exp += (100.0d * Math.Pow(1.1, i));
        }
        return exp;
    }
    /// <summary>
    /// Returns a skill's level including boost and bed boost. Use GetSkillLevelUnboosted() for the real level.
    /// </summary>
    /// <returns></returns>
    public int GetSkillLevel()
    {
        return Level + Boost;
    }
    public int GetSkillLevelUnboosted()
    {
        return Level;
    }
    public void SetSkillLevel(int level)
    {
        Level = level;
    }
    public override string ToString()
    {
        return Name;
    }
    public void LoadExperience(long amount)
    {
        //Console.WriteLine("Loading EXP for " + Name + ":" + amount);
        if (amount < 0)
        {
            return;
        }
        Level = 1;
        Experience = 0;
        Experience += amount;

        while(Experience >= (long)Skill.GetExperienceRequired(GetSkillLevelUnboosted()))
        {

            if(Level > 350)
            {
                break;
            }
            Level++;
        }
    }
}
