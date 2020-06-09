using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Extensions
{
    private static string _validCharString;
    private static List<char> _validCharacters;
    private static Random rand = new Random();
    public static List<char> ValidCharacters
    {
        get
        {
            if(_validCharacters != null)
            {
                return _validCharacters;
            }
            _validCharString = "";
            for (int i = 33; i < 3500; i++)
            {
                string c = char.ConvertFromUtf32(i);
                if(c != ":" && c != ",")
                {
                    _validCharString += c;
                }

            }
            _validCharacters = _validCharString.ToCharArray().ToList();
            return _validCharacters;
        }
    }
    public static string ToEncodedString(this ulong number)
    {
        var buffer = new StringBuilder();
        var quotient = number;
        ulong remainder;
        while(quotient != 0)
        {
            remainder = quotient % (ulong)ValidCharacters.Count;
            quotient = quotient / (ulong)ValidCharacters.Count;
            buffer.Insert(0, ValidCharacters[(int)remainder].ToString());
            Console.WriteLine("Character:" + ValidCharacters[(int)remainder] +",R:" + remainder + "Q:" + quotient);
        }
        return buffer.ToString();
    }
    public static ulong FromEncodedString(this string input)
    {
        ulong output = 0;

        foreach(char c in input)
        {

                output = (ulong)(output * (ulong)ValidCharacters.Count) + (ulong)ValidCharacters.IndexOf(c);
            
        }

        return output;
    }
    public static int ToGaussianRandom(this double input)
    {
        double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                     Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        double randNormal =
                     Math.Abs(input + (input / 2.5d) * randStdNormal); //random normal(mean,stdDev^2)

        return (int)(randNormal + 1);
    }
    /// <summary>
    /// Returns a positive gaussian random integer; always at least 1.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static int ToGaussianRandom(this int input)
    {
        double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                     Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        double randNormal =
                     Math.Abs(input + (input / 2.5d) * randStdNormal); //random normal(mean,stdDev^2)

        return (int)(randNormal + 1);
    }
    public static int ToRandomDamage(this int input)
    {
        double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                     Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        double randNormal =
                     Math.Abs(input + (input / 2.5d) * randStdNormal); //random normal(mean,stdDev^2)

        return (int)Math.Abs(randNormal);
    }
    public static string RemoveWhitespace(this string str)
    {
        return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
    }
    public static double CalculateArmorDamageReduction()
    {
        double armorTotal = 0;
        foreach (KeyValuePair<GameItem, int> item in Player.Instance.Inventory.GetItems())
        {
            if (item.Key.IsEquipped)
            {
                if(item.Key.ArmorInfo != null)
                {
                    armorTotal += item.Key.ArmorInfo.ArmorBonus;
                }
                if(item.Key.WeaponInfo != null)
                {
                    armorTotal += item.Key.WeaponInfo.ArmorBonus;
                }
                
            }

        }
        double reduction = ((armorTotal * 0.04d) / (1 + (armorTotal * 0.04d)));
        return 1 - reduction;
    }
    public static double CalculateArmorDamageReduction(Monster monster)
    {
        return 1 - (((monster.Armor * 0.07d) / (1 + (monster.Armor * 0.07d))) / 2);

    }
}

