using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace UnoOnline
{
    public static class MessageHandlers
    {
        public static void HandleTurnMessage(Message message)
        {
            string playerId = message.Data[0];
            if (playerId == Program.player.Name)
            {
                //Enable các lá bài có thể đánh
            }
        }
        public static void HandleNewMessageType(Message message)
        {
            // Add your handling logic here
            Console.WriteLine("Handling new message type.");
            // Example: Log the message data
            foreach (var data in message.Data)
            {
                Console.WriteLine(data);
            }
        }

        public static void HandleConnect(Message message)
        {
            var newPlayer = new Player(message.Data[0]);
            ClientSocket.gamemanager.AddPlayer(newPlayer);
        }

        public static void HandleDisconnect(Message message)
        {
            var disconnectingPlayer = ClientSocket.gamemanager.Players.FirstOrDefault(p => p.Name == message.Data[0]);
            if (disconnectingPlayer != null)
            {
                ClientSocket.gamemanager.Players.Remove(disconnectingPlayer);
            }
        }

        public static void HandleStart(Message message)
        {
            //GameManager.InitializeGame();
        }

        public static void HandleYellUNO(Message message)
        {
            string playerId = message.Data[0];
            Player player = GameManager.Instance.Players.FirstOrDefault(p => p.Name == playerId);
            if (player != null)
            {
                Console.WriteLine($"{player.Name} yelled UNO!");
                // Implement additional logic if needed
            }
            else
            {
                Console.WriteLine($"Player with ID {playerId} not found.");
            }
        }

        public static void HandleResult(Message message)
        {
            string playerId = message.Data[0];
            int points = int.Parse(message.Data[1]);

            Player player = GameManager.Instance.Players.FirstOrDefault(p => p.Name == playerId);
            if (player != null)
            {
                Console.WriteLine($"{player.Name} scored {points} points.");
                // Implement additional logic to update the player's score if needed
            }
            else
            {
                Console.WriteLine($"Player with ID {playerId} not found.");
            }
        }
        public static void HandleEndMessage(Message message)
        {
            string[] data = message.Data.ToArray();
            string winnerName = data[0];
            if (winnerName == Program.player.Name)
            {
                //Hiển thị màn hình thắng
            }
            else
            {
                //Hiển thị màn hình thua
            }
        }

        public static void HandleChat(Message message)
        {
            Console.WriteLine($"Chat from {message.Data[0]}: {message.Data[1]}");
        }

        public static void HandleRestart(Message message)
        {
            //GameManager.InitializeGame();
        }

        public static void HandleFinish(Message message)
        {
            Console.WriteLine("Game has finished.");
        }
        /*
        public static void HandlePenalty(Message message)
        {
            string playerId = message.Data[0];
            int penaltyCount = int.Parse(message.Data[1]);

            Player player = GameManager.Instance.Players.FirstOrDefault(p => p.Name == playerId);
            if (player != null)
            {
                for (int i = 0; i < penaltyCount; i++)
                {
                    Card penaltyCard = GameManager.Instance.Deck.Cards.FirstOrDefault();
                    if (penaltyCard != null)
                    {
                        player.Hand.Add(penaltyCard);
                        GameManager.Instance.Deck.Cards.Remove(penaltyCard);
                        Console.WriteLine($"{player.Name} drew a {penaltyCard.Color} {penaltyCard.Value} card as a penalty.");
                    }
                    else
                    {
                        Console.WriteLine("No cards left in the deck to draw.");
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Player with ID {playerId} not found.");
            }
        }
        public static void HandleCardDraw(Message message)
        {
            string playerId = message.Data[0];
            Player player = GameManager.Instance.Players.FirstOrDefault(p => p.Name == playerId);
            if (player != null)
            {
                Card drawnCard = GameManager.Instance.Deck.Cards.FirstOrDefault();
                if (drawnCard != null)
                {
                    player.Hand.Add(drawnCard);
                    GameManager.Instance.Deck.Cards.Remove(drawnCard);
                    Console.WriteLine($"{player.Name} drew a {drawnCard.Color} {drawnCard.Value} card.");
                }
                else
                {
                    Console.WriteLine("No cards left in the deck to draw.");
                }
            }
            else
            {
                Console.WriteLine($"Player with ID {playerId} not found.");
            }
        }
        public static void HandleSpecialDraw(Message message)
        {
            var specialDrawPlayer = ClientSocket.gamemanager.Players.FirstOrDefault(p => p.Name == message.Data[0]);
            if (specialDrawPlayer != null)
            {
                var specialDrawCard = ClientSocket.gamemanager.Deck.Cards.FirstOrDefault();
                if (specialDrawCard != null)
                {
                    specialDrawPlayer.Hand.Add(specialDrawCard);
                    ClientSocket.gamemanager.Deck.Cards.Remove(specialDrawCard);
                }
            }
        }
        */
    }
}
