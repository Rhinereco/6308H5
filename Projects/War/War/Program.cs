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
            List<(int value, string suit)> deck = new List<(int, string)>();// Create a deck as a list of tuples
            foreach (var suit in suits)// Loop through each suit
            {
                foreach (var value in values)// Loop through each card value
                {
                    deck.Add((value, suit));// Add card to deck
                }
            }

            Shuffle(deck);//shuffle the deck

            int player1Score = 0;
            int player2Score = 0;

            while (deck.Count > 0)// Continue while there are cards in the deck
            {
                Console.WriteLine("Press Enter to continue...");
                Console.ReadKey();
                var player1Card = deck[0];// Draw the first card for Player 1
                var player2Card = deck[1];// Draw the second card for Player 2
                string player1Display = GetCardDisplay(player1Card.value);// Convert Player 1's card value for display
                string player2Display = GetCardDisplay(player2Card.value);// Convert Player 2's card value for display
                deck.RemoveRange(0, 2); // remove drawed card
                Console.WriteLine($"Player 1: {player1Display} of {player1Card.suit} | Player 2: {player2Display} of {player2Card.suit}");
                if (player1Card.value > player2Card.value)// compare value
                {
                    player1Score++;
                    Console.WriteLine($"Player 1 wins with {player1Display} of {player1Card.suit} vs {player2Display} of {player2Card.suit}");
                    Console.WriteLine("======================================================");//separator
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
            Console.WriteLine("\nGame Over!");
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
                _ => value.ToString()//directly convert other int values to string
            };
        }
        static void Shuffle<T>(List<T> list)// shuffle
        {
            Random rng = new Random();// Create random number generator
            int n = list.Count;// counter, get number of cards in deck
            while (n > 1)// While there are cards left to shuffle
            {
                n--;// Decrement counter
                int k = rng.Next(n + 1);// Generate random index
                T value = list[k];// Swap values
                list[k] = list[n];//put n to k
                list[n] = value;//put k to n
            }
        }
    }
}