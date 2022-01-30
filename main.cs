using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class Program 
{
    static int MAX_LEVEL = 30;

    static int PLAYER_1_START = 10;
    static int PLAYER_2_START = 9;

    public static void Main (string[] args) 
    {
        var start = new Position(0, 0, PLAYER_1_START, PLAYER_2_START, false);
        
        var movesDict = new MovesDictionary { start };
        var levelPositions = new List<Position> { start };
        
        for (var gen = 0; gen < MAX_LEVEL; gen++)
        {
            Console.WriteLine($"gen = {gen}");
            foreach (var pos in levelPositions)
                movesDict.AddRange(pos.CreateNextPositions());
            
            levelPositions.Clear();
            levelPositions.AddRange(movesDict.FilterLevel(gen + 1));          
        }

        Console.WriteLine($"Fin, count = {movesDict.Count()}");
    }
}

class Position 
{
    public int Player1Score { get; }
    public int Player2Score { get; }
    public int BoardPosPlayer1 { get; }
    public int BoardPosPlayer2 { get; }
    public bool Player1Moved { get; }

    readonly IEnumerable<int> allowedMoves = Enumerable.Range(1, 3);

    public Position(int player1Score, int player2Score, int boardPos1, int boardPos2, bool player1moved)
    {
        Player1Score = player1Score;
        Player2Score = player2Score;
        BoardPosPlayer1 = boardPos1;
        BoardPosPlayer2 = boardPos1;
        Player1Moved = player1moved;
    }

    public Tuple<int, int, int, int, bool> Key => new Tuple<int, int, int, int, bool>(Player1Score, Player2Score, BoardPosPlayer1, BoardPosPlayer2, Player1Moved);

    public (int, int) Scores => (Player1Score, Player2Score);

    public bool IsValid => Player1Score >= 0 && Player2Score >= 0;

    public bool IsWinning => Player1Score >= 21 || Player2Score >= 21;

    public IEnumerable<Position> CreatePreviousPositions()
    {
        foreach (var move in allowedMoves)
        {
            var previousPos = PrevPos(Player1Moved ? BoardPosPlayer1 : BoardPosPlayer2, move);
            var prevPlayer1Score = Player1Moved ? Player1Score - BoardPosPlayer1 : Player1Score;
            var prevPlayer2Score = Player1Moved ? Player2Score : Player2Score - BoardPosPlayer2;
            var prevPosPlayer1 = Player1Moved ? previousPos : BoardPosPlayer1;
            var prevPosPlayer2 = Player1Moved ? BoardPosPlayer2 : previousPos;

            yield return new Position(prevPlayer1Score, prevPlayer2Score, prevPosPlayer1, prevPosPlayer2, !Player1Moved);          
        }
    }

    public IEnumerable<Position> CreateNextPositions()
    {
        if (IsWinning)
           yield break;

        foreach (var move in allowedMoves)
        {
            var nextPos = NextPos(Player1Moved ? BoardPosPlayer2 : BoardPosPlayer1, move);
            var nextPlayer1Score = Player1Moved ? Player1Score : Player1Score + nextPos;
            var nextPlayer2Score = Player1Moved ? Player2Score + nextPos : Player2Score;
            var nextPosPlayer1 = Player1Moved ? BoardPosPlayer1 : nextPos;
            var nextPosPlayer2 = Player1Moved ? nextPos : BoardPosPlayer2;

            yield return new Position(nextPlayer1Score, nextPlayer2Score, nextPosPlayer1, nextPosPlayer2, !Player1Moved);          
        }
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
}

class MovesDictionary : IEnumerable 
{
    Dictionary<Tuple<int, int, int, int, bool>, List<Position>> dict;

    public MovesDictionary()
    {
        dict = new Dictionary<Tuple<int, int, int, int, bool>, List<Position>>();
    }

    public IEnumerator GetEnumerator()
    {
        return dict.GetEnumerator();
    }

    public void Add(Position position)
    {
        var key = position.Key;

        if (dict.ContainsKey(key))
            dict[key].Add(position);
        else
            dict[key] = new List<Position>{ position };
    }

    public void AddRange(IEnumerable<Position> positions)
    {
        foreach (var pos in positions)
            Add(pos);
    }

    public IEnumerable<Position> FilterLevel(int maxLevel)
    {
        foreach (var (key, value) in dict)
        {
            var (score1, score2, _, _, _) = key;
            if (Math.Max(score1, score2) == maxLevel)
                foreach (var pos in value)
                    yield return pos;
        }
    }

    public int Count() => dict.Values.SelectMany(x => x).Count();
}

