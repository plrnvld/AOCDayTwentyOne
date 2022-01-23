using System;
using System.Collections.Generic;

class Program 
{
    public static void Main (string[] args) 
    {
        var movesDict = new Dictionary<(int, int), List<(int, int)>>();


        Console.WriteLine("Fin");
    }

    static int NextPos(int currentPos, int add)
    {
        var sum = currentPos + add;
        if (sum <= 10)
            return sum;

        if (sum % 10 == 0)
            return 10;

        return sum - (sum/10) * 10;
    }

    static IEnumerable<(int, int)> GetReachableFrom((int, int) score)
    {
        return new List<(int, int)>();
    }

    static IEnumerable<(int, int)> CreatePlayerOneEndPositions()
    {
    }
}



// 399672 (366*1092) too low