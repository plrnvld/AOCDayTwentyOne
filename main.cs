using System;
using System.Collections.Generic;

class Program 
{
    public static void Main (string[] args) 
    {
        var movesDict = new Dictionary<(int, int), List<(int, int)>>();

        Console.WriteLine(PrevPos(4, 2)); // 2
        Console.WriteLine(PrevPos(4, 3)); // 1
        Console.WriteLine(PrevPos(4, 4)); // 10
        Console.WriteLine(PrevPos(1, 9)); // 2
        Console.WriteLine(PrevPos(1, 10)); // 1
        Console.WriteLine(PrevPos(1, 20)); // 1
        Console.WriteLine(PrevPos(1, 21)); // 10
        Console.WriteLine(PrevPos(1, 11)); // 10
        Console.WriteLine(PrevPos(1, 12)); // 9

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

    static int PrevPos(int currentPos, int remove)
    {
        var sum = currentPos - remove;
        if (sum >= 1)
            return sum;

        var tooLow = 1 - sum;
        
        if (tooLow % 10 == 0)
            return 1;

        return sum + (tooLow/10 + 1)*10;
    }

    static IEnumerable<(int, int)> GetReachableFrom((int, int) score)
    {
        return new List<(int, int)>();
    }

    static IEnumerable<Position> CreatePlayerOneEndPositions()
    {
        yield break;
    }
}

class Position 
{
    public int Player1Score { get; }
    public int Player2Score { get; }
    public int BoardPos { get; }
    public Position NextPosition { get; }
    public bool Player1Moved { get; }

    public Position(int player1Score, int player2Score, int boardPos, bool player1moved, Position nextPosition)
    {
        Player1Score = player1Score;
        Player2Score = player2Score;
        BoardPos = boardPos;
        Player1Moved = player1moved;
        NextPosition = nextPosition;
    }

    public Position(int player1Score, int player2Score, int boardPos, bool player1moved) 
        : this(player1Score, player2Score, boardPos, player1moved, null)
    {
    }

    public (int, int) Scores => (Player1Score, Player2Score);

    public IEnumerable<Position> CreatePreviousPositions()
    {
        if (HasNext)
        {
            var next = NextPosition;

        }
        else
        {


        }

        yield break;
    }

    bool HasNext => NextPosition != null;


}
