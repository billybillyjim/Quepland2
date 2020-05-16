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
}

