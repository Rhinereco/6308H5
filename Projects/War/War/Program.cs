using System;
using System.Collections.Generic;

namespace WarCardGame
{
    class Program
    {
        static void Main(string[] args)
        {
            // initialize suits and values
            string[] suits = { "Hearts", "Clubs", "Spades", "Diamonds" };
            int[] values = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }; // 2 to A

            List<(int value, string suit)> deck = new List<(int, string)>();
            foreach (var suit in suits)
            {
                foreach (var value in values)
                {
                    deck.Add((value, suit));
                }
            }

            Shuffle(deck);//shuffle the deck

            int player1Score = 0;
            int player2Score = 0;

            while (deck.Count > 0)
            {
                Console.WriteLine("Press Enter to continue...");
                Console.ReadKey();
                var player1Card = deck[0];
                var player2Card = deck[1];
                string player1Display = GetCardDisplay(player1Card.value);
                string player2Display = GetCardDisplay(player2Card.value);
                deck.RemoveRange(0, 2); // remove drawed card
                Console.WriteLine($"Player 1: {player1Display} of {player1Card.suit} | Player 2: {player2Display} of {player2Card.suit}");
                if (player1Card.value > player2Card.value)// compare value
                {
                    player1Score++;
                    Console.WriteLine($"Player 1 wins with {player1Display} of {player1Card.suit} vs {player2Display} of {player2Card.suit}");
                    Console.WriteLine("======================================================");
                }
                else if (player1Card.value < player2Card.value)
                {
                    player2Score++;
                    Console.WriteLine($"Player 2 wins with {player2Display} of {player2Card.suit} vs {player1Display} of {player1Card.suit}");
                    Console.WriteLine("======================================================");
                }
                else
                {
                    Console.WriteLine($"It's a tie with {player1Display} of {player1Card.suit} vs {player2Display} of {player2Card.suit}");
                    Console.WriteLine("======================================================");
                }
            }
            Console.WriteLine("\nGame Over!");//game end, output the final grade
            Console.WriteLine($"Player 1 Score: {player1Score}");
            Console.WriteLine($"Player 2 Score: {player2Score}");
            if (player1Score > player2Score)// check who is the winner
            {
                Console.WriteLine("Player 1 wins the game!");
            }
            else if (player1Score < player2Score)
            {
                Console.WriteLine("Player 2 wins the game!");
            }
            else
            {
                Console.WriteLine("The game is a tie!");
            }
        }
        static string GetCardDisplay(int value)// Convert card value to display value
        {
            return value switch
            {
                11 => "J",
                12 => "Q",
                13 => "K",
                14 => "A",
                _ => value.ToString()
            };
        }
        static void Shuffle<T>(List<T> list)// shuffle
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}