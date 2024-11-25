using System;
using System.Linq;

namespace UnoOnline
{
    public static class MessageHandlers
    {
        public static void HandleUpdate(Message message)
        {
            // Add your handling logic here
        }

        public static void HandleNewMessageType(Message message)
        {
            // Add your handling logic here
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
            GameManager.InitializeGame();
        }

        public static void HandleDanhBai(Message message)
        {
            var danhBaiPlayer = ClientSocket.gamemanager.Players.FirstOrDefault(p => p.Name == message.Data[0]);
            if (danhBaiPlayer != null)
            {
                var danhBaiCard = new Card(message.Data[1], message.Data[2]);
                ClientSocket.gamemanager.PlayCard(danhBaiPlayer, danhBaiCard);
            }
        }

        public static void HandleRutBai(Message message)
        {
            var rutBaiPlayer = ClientSocket.gamemanager.Players.FirstOrDefault(p => p.Name == message.Data[0]);
            if (rutBaiPlayer != null)
            {
                var rutBaiCard = ClientSocket.gamemanager.Deck.Cards.FirstOrDefault();
                if (rutBaiCard != null)
                {
                    rutBaiPlayer.Hand.Add(rutBaiCard);
                    ClientSocket.gamemanager.Deck.Cards.Remove(rutBaiCard);
                }
            }
        }

        public static void HandleSpecialCardEffect(Message message)
        {
            // Add your handling logic here
        }

        public static void HandleYellUNO(Message message)
        {
            // Add your handling logic here
        }

        public static void HandleDrawPenalty(Message message)
        {
            // Add your handling logic here
        }

        public static void HandleDiem(Message message)
        {
            // Add your handling logic here
        }

        public static void HandleChat(Message message)
        {
            Console.WriteLine($"Chat from {message.Data[0]}: {message.Data[1]}");
        }

        public static void HandleRestart(Message message)
        {
            GameManager.InitializeGame();
        }

        public static void HandleFinish(Message message)
        {
            Console.WriteLine("Game has finished.");
        }
        public static void HandleCardDraw(Message message)
        {
            var drawingPlayer = ClientSocket.gamemanager.Players.FirstOrDefault(p => p.Name == message.Data[0]);
            if (drawingPlayer != null)
            {
                var drawnCard = ClientSocket.gamemanager.Deck.Cards.FirstOrDefault();
                if (drawnCard != null)
                {
                    drawingPlayer.Hand.Add(drawnCard);
                    ClientSocket.gamemanager.Deck.Cards.Remove(drawnCard);
                }
            }
        }
    }

}
