using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;


public static class Extensions
{

    private static Random rand = new Random();

    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
    this IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector,
    IEqualityComparer<TKey> comparer)
    {
        HashSet<TKey> knownKeys = new HashSet<TKey>(comparer);
        foreach (TSource element in source)
        {
            if (knownKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
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

    // Deep clone from Neil on StackOverflow
    //https://stackoverflow.com/questions/129389/how-do-you-do-a-deep-copy-of-an-object-in-net
    public static T DeepClone<T>(this T a)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, a);
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
        }
    }
    public static string CustomFormat(this int num)
    {
        int digits = (int)Math.Floor(Math.Log10(num) + 1);
        string letter = "K";
        if (digits < 4)
        {
            return num.ToString();
        }
        else if (digits < 7)
        {
            letter = "K";
        }
        else if (digits < 10)
        {
            letter = "M";
        }
        else if (digits < 13)
        {
            letter = "B";
        }
        else if (digits < 16)
        {
            letter = "T";
        }
        else
        {
            return num.ToString("0.00E+0");
        }

        if (digits % 3 == 0)
        {
            return num.ToString("000.00000E+0").Substring(0, 3) + letter;
        }
        else if (digits % 3 == 1)
        {
            return num.ToString("0.0000000E+0").Substring(0, 4) + letter;
        }
        else
        {
            return num.ToString("00.000000E+0").Substring(0, 4) + letter;
        }

    }
    public static string CustomFormat(this long num)
    {
        int digits = (int)Math.Floor(Math.Log10(num) + 1);
        string letter = "K";
        if (digits < 4)
        {
            return num.ToString();
        }
        else if (digits < 7)
        {
            letter = "K";
        }
        else if (digits < 10)
        {
            letter = "M";
        }
        else if (digits < 13)
        {
            letter = "B";
        }
        else if (digits < 16)
        {
            letter = "T";
        }
        else if (digits < 19)
        {
            letter = "Qa";
        }
        else if (digits < 22)
        {
            letter = "Qi";
        }
        else if (digits < 25)
        {
            letter = "s";
        }      
        else
        {
            return num.ToString("0.00E+0");
        }

        if (digits % 3 == 0)
        {
            return num.ToString("000.00000E+0").Substring(0, 3) + letter;
        }
        else if (digits % 3 == 1)
        {
            return num.ToString("0.0000000E+0").Substring(0, 4) + letter;
        }
        else
        {
            return num.ToString("00.000000E+0").Substring(0, 4) + letter;
        }

    }

}

