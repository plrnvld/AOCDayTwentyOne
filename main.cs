using System;

class Program 
{
    public static void Main (string[] args) 
    {
        var player1Pos = 10;
        var player2Pos = 9;
        
        var player1Score = 0;
        var player2Score = 0;
        
        var dicePos = 1;
        var turnForPlayer1 = true;
        var rolls = 0;

        while (player1Score < 1000 && player2Score < 1000)
        {
            var posAddition = ((dicePos+1)*3);

            if (turnForPlayer1) 
            {
                player1Pos = NextPos(player1Pos, posAddition);
                player1Score += player1Pos;
            }
            else 
            {
                player2Pos = NextPos(player2Pos, posAddition);
                player2Score += player2Pos;
            }

            dicePos += 3;
            dicePos %= 100;
            rolls += 3;  

            if (rolls/3 < 10 || rolls >= 993 - 12)
            {
                Console.WriteLine($"Player {(turnForPlayer1 ? 1 : 2)} rolls {dicePos-3}+{dicePos-2}+{dicePos-1} and moves to space {(turnForPlayer1 ? player1Pos : player2Pos)} for a total score of {(turnForPlayer1 ? player1Score : player2Score)}. ({rolls} rolls)");
            }

            turnForPlayer1 = !turnForPlayer1;
        }

        var lowestScore = Math.Min(player1Score, player2Score);

        Console.WriteLine($"Player 1 score: {player1Score}, player 2 score {player2Score}.");
        Console.WriteLine($"Result = {lowestScore} * {rolls} = {lowestScore * rolls}.");
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
}

// 399672 (366*1092) too low