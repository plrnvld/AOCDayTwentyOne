using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class Program 
{
    static int MAX_LEVEL = 60;

    static int PLAYER_1_START = 10;
    static int PLAYER_2_START = 9;
    // static int PLAYER_1_START = 4; // Total 786,316,482,957,123 worlds
    // static int PLAYER_2_START = 8;

    static IEnumerable<int> allLevels = Enumerable.Range(0, MAX_LEVEL + 1);

    public static void Main (string[] args) 
    {
        var start = new Position(0, 0, PLAYER_1_START, PLAYER_2_START, false);
        
        var movesDict = new MovesDictionary { start };
        var levelPositions = new List<Position> { start };

        foreach (var level in allLevels)
        {
            Console.Write(".");

            while (levelPositions.Any())
            {
                var pos = levelPositions.First();
                levelPositions.RemoveAt(0);

                var nextPositions = pos.NextPositions;
                
                movesDict.AddRange(nextPositions);
            }
            
            levelPositions.AddRange(movesDict.FilterLevel(level + 1).Select(n => n.Position));          
        }

        Console.WriteLine();
        Console.WriteLine("All positions generated");
        Console.WriteLine();

        foreach (var level in allLevels)
        {
            Console.WriteLine($"Counting world for level {level}");

            var levelNodes = movesDict.FilterLevel(level);

            foreach (var node in levelNodes)
                node.UpdateWorldCount();
        }

        var winningWorlds = movesDict.WinningNodes.Sum(n => n.WorldCount);

        var player1Wins = movesDict.WinningNodes.Where(n => n.Position.Player1Moved).Sum(n => n.WorldCount);
        var player2Wins = movesDict.WinningNodes.Where(n => !n.Position.Player1Moved).Sum(n => n.WorldCount);

        Console.WriteLine($"Fin, {movesDict.Count()} nodes with {movesDict.WinningNodes.Count()} winning nodes with {winningWorlds} winning worlds.");

        Console.WriteLine($"Player 1 wins in {player1Wins} worlds, and player 2 in {player2Wins} worlds.");
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

    readonly IEnumerable<int> allowedMoves = new [] { 3, 4, 4, 4, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 8, 8, 8, 9 }; 

    public Position(int player1Score, int player2Score, int boardPos1, int boardPos2, bool player1moved)
    {
        Player1Score = player1Score;
        Player2Score = player2Score;
        BoardPosPlayer1 = boardPos1;
        BoardPosPlayer2 = boardPos2;
        Player1Moved = player1moved;
    }

    public (int, int, int, int, bool) Key => (Player1Score, Player2Score, BoardPosPlayer1, BoardPosPlayer2, Player1Moved);

    public int Level => Player1Score + Player2Score;

    public (int, int) Scores => (Player1Score, Player2Score);

    public bool IsWinning => Player1Wins || Player2Wins;

    public bool Player1Wins => Player1Score >= WINNING_SCORE;

    public bool Player2Wins => Player2Score >= WINNING_SCORE;

    public IEnumerable<(int, int, int, int, bool)> PrevKeys
    {
        get
        {
            if (Player1Score == 0 && Player2Score == 0)
                yield break;

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
    Dictionary<(int, int, int, int, bool), PositionNode> dict;

    public MovesDictionary()
    {
        dict = new Dictionary<(int, int, int, int, bool), PositionNode>();
    }

    public IEnumerator GetEnumerator() => dict.GetEnumerator();

    public void Add(Position position)
    {
        var key = position.Key;

        if (dict.ContainsKey(key))
            dict[key].IncrementCounter();
        else
            dict[key] = new PositionNode(position, this);
    }

    public void AddRange(IEnumerable<Position> positions)
    {
        foreach (var pos in positions)
            Add(pos);
    }

    public PositionNode GetNode(Position pos)
    {
        return GetNode(pos.Key);
    }

    public PositionNode GetNode((int, int, int, int, bool) key)
    {
        return dict.ContainsKey(key) ? dict[key] : null;        
    }


    public IEnumerable<PositionNode> FilterLevel(int maxLevel)
    {
        foreach (var (key, value) in dict)
        {
            if (value.Position.Level == maxLevel)
                yield return value;
        }
    }

    public IEnumerable<PositionNode> WinningNodes => AllNodes.Where(n => n.Position.IsWinning);

    public int Count() => AllNodes.Count();

    IEnumerable<PositionNode> AllNodes => dict.Values;
}

class PositionNode
{
    public long Counter { get; private set; }
    public Position Position { get; }
    public (long, long) Wins { get; private set; }
    public long WorldCountMultiplied => WorldCount * Counter;

    public long WorldCount { get; set; }
    MovesDictionary dict;

    public PositionNode(Position pos, MovesDictionary dict)
    {
        Position = pos;
        Counter = 1;
        Wins = (0, 0);
        WorldCount = 0;

        this.dict = dict;
    }

    public void IncrementCounter()
    {
        Counter++;
    }

    public void UpdateWorldCount()
    {
        var prevKeys = Position.PrevKeys;
        var prevNodes = prevKeys.Select(dict.GetNode).Where(n => n != null);

        if (!prevNodes.Any())
        {
            WorldCount = 1;
            Console.WriteLine($">> No prev worlds found for {Position.Key}");
            return;
        }

        WorldCount += prevNodes.Sum(n => n.WorldCount);
    }

    public void CollectWins(IEnumerable<PositionNode> reachablePositionNodes)
    {
        (long, long) SumTups((long, long) agg, (long, long) next)
        {
            var (agg1, agg2) = agg;
            var (next1, next2) = next;
            return (agg1 + next1, agg2 + next2);
        }

        var (sumWins1, sumWins2) = reachablePositionNodes.Select(p => p.Wins).Aggregate<(long, long), (long, long)>((0, 0), SumTups);

        Wins = (sumWins1 * Counter, sumWins2 * Counter);
    }

    public void RecordPlayer1Win()
    {
        if (Wins != (0, 0))
            throw new Exception($"Not possible for {Wins}");

        Wins = (1, 0);
    }

    public void RecordPlayer2Win()
    {
        if (Wins != (0, 0))
            throw new Exception($"Not possible for {Wins}");

        Wins = (1, 0);
    }
}

