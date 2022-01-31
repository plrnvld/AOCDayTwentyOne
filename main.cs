using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class Program 
{
    static int MAX_LEVEL = 30;

    // static int PLAYER_1_START = 10;
    // static int PLAYER_2_START = 9;
    static int PLAYER_1_START = 4;
    static int PLAYER_2_START = 8;

    public static void Main (string[] args) 
    {
        var start = new Position(0, 0, PLAYER_1_START, PLAYER_2_START, false);
        
        var movesDict = new MovesDictionary { start };
        var levelPositions = new List<Position> { start };
        
        for (var gen = 0; gen <= MAX_LEVEL; gen++)
        {
            Console.WriteLine($"Calculating gen = {gen}");
            foreach (var pos in levelPositions)
                movesDict.AddRange(pos.NextPositions);
            
            levelPositions.Clear();
            levelPositions.AddRange(movesDict.FilterLevel(gen + 1));          
        }

        /*
        for (var gen = MAX_LEVEL; gen >= 0; gen--)
        {
            var winning = movesDict.FilterLevel(gen).Where(p => p.IsWinning);
            Console.WriteLine($"At gen {gen} => winning {winning.Count()}");         
        }
        */


        long sumWins1 = 0;
        long sumWins2 = 0;

        var winningPositions = movesDict.WinningPositions;

        Console.WriteLine($"{winningPositions.Count()} winning positions in total");

        foreach (var win in winningPositions)
        {
            var (wins1, wins2) = movesDict.CountWinWorlds(win, start);
            sumWins1 += wins1;
            sumWins2 += wins2;
        }

        Console.WriteLine($"Fin, wins 1 = {sumWins1}, wins 2 = {sumWins2}");
    }
}

class Position 
{
    static int WINNING_SCORE = 21;

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
        BoardPosPlayer2 = boardPos2;
        Player1Moved = player1moved;
    }

    public (int, int, int, int, bool) Key => (Player1Score, Player2Score, BoardPosPlayer1, BoardPosPlayer2, Player1Moved);

    public (int, int) Scores => (Player1Score, Player2Score);

    public bool IsWinning => Player1Wins || Player2Wins;

    public bool Player1Wins => Player1Score >= WINNING_SCORE;

    public bool Player2Wins => Player2Score >= WINNING_SCORE;

    public IEnumerable<(int, int, int, int, bool)> PreviousKeys
    {
        get
        {
            foreach (var move in allowedMoves)
            {
                if (Player1Moved)
                {
                    var prevPosPlayer1 = PrevPos(BoardPosPlayer1, move);
                    var prevPlayer1Score = Player1Score - BoardPosPlayer1;
                    
                    yield return (prevPlayer1Score, Player2Score, prevPosPlayer1, BoardPosPlayer2, false);
                }
                else 
                {
                    var prevPosPlayer2 = PrevPos(BoardPosPlayer2, move);
                    var prevPlayer2Score = Player2Score - BoardPosPlayer2;
                    
                    yield return (Player1Score, prevPlayer2Score, BoardPosPlayer1, prevPosPlayer2, true);
                }          
            }
        }
    }

    public IEnumerable<Position> NextPositions
    {
        get 
        {
            if (IsWinning)
                yield break;

            foreach (var move in allowedMoves)
            {
                if (Player1Moved)
                {
                    var nextPosPlayer2 = NextPos(BoardPosPlayer2, move);
                    var nextPlayer2Score = Player2Score + nextPosPlayer2;
                    
                    yield return new Position(Player1Score, nextPlayer2Score, BoardPosPlayer1, nextPosPlayer2, false);

                }
                else 
                {
                    var nextPosPlayer1 = NextPos(BoardPosPlayer1, move);
                    var nextPlayer1Score = Player1Score + nextPosPlayer1;
                    
                    var result = new Position(nextPlayer1Score, Player2Score, nextPosPlayer1, BoardPosPlayer2, true);

                    yield return result;
                }      
            }
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

    public bool IsEqualTo(Position pos)
    {
        return Key == pos.Key;
    } 
}

class MovesDictionary : IEnumerable 
{
    Dictionary<(int, int, int, int, bool), List<Position>> dict;

    public MovesDictionary()
    {
        dict = new Dictionary<(int, int, int, int, bool), List<Position>>();
    }

    public IEnumerator GetEnumerator() => dict.GetEnumerator();

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

    public IEnumerable<Position> WinningPositions => AllPositions.Where(p => p.IsWinning);

    public (long, long) CountWinWorlds(Position pos, Position startPos)
    {
        if (!pos.IsWinning)
            throw new Exception($"Not winning: {pos}");
        
        var worldsCount = CountReachableWorlds(pos, startPos);

        return pos.Player1Wins ? (worldsCount, 0) : (0, worldsCount);
    }

    long CountReachableWorlds(Position pos, Position startPos)
    {
        // Console.WriteLine($"  {pos.Key} => counting reachable worlds");

        if (!dict.ContainsKey(pos.Key))
        {   
            // Console.WriteLine($"    Pos {pos.Key} not in dict");
            return 0;
        }

        if (pos.IsEqualTo(startPos))
        {
            // Console.WriteLine($"    Pos {pos.Key} is equal to start");
            return 1;
        }

        var reachablePrevKeys = pos.PreviousKeys.Where(dict.ContainsKey);

        if (!reachablePrevKeys.Any())
        {
            var allKeysInfo = string.Join(" - ", pos.PreviousKeys.Select(k => k.ToString()));
            // Console.WriteLine($"    {pos.Key} => nothing reachable, all prevs: {allKeysInfo}");
            return 0;
        }

        var separateWorlds = reachablePrevKeys.Select(key => CountWorldsForKey(key, startPos));

        if (separateWorlds.Count() > 1)
            Console.WriteLine($"      Separate worlds: {string.Join(",", separateWorlds)}");

        return separateWorlds.Aggregate((a, x) => a * x);

        long CountWorldsForKey((int, int, int, int, bool) key, Position startPos)
        {
            var values = dict[key];

            return CountReachableWorlds(values.First(), startPos) * values.Count();
        }
    }

    public int Count() => AllPositions.Count();

    IEnumerable<Position> AllPositions => dict.Values.SelectMany(x => x);
}

