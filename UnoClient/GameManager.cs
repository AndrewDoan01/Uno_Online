using System.Collections.Generic;

public class GameManager
{
    public List<Player> Players { get; set; }
    public Deck Deck { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public static GameManager Instance { get; private set; }

    public GameManager()
    {
        Players = new List<Player>();
        Deck = new Deck();
        Deck.Shuffle();
        CurrentPlayerIndex = 0;
    }

    public static void InitializeGame()
    {
        if (Instance == null)
        {
            Instance = new GameManager();
        }
    }

    public void UpdateOtherPlayerName(string OtherPlayerName)
    {
        bool playerExists = Players.Exists(p => p.Name == OtherPlayerName);
        if (!playerExists)
        {
            Players.Add(new Player(OtherPlayerName));
        }
    }

    public void AddPlayer(Player player)
    {
        Players.Add(player);
    }

    public void StartTurn()
    {
        var currentPlayer = Players[CurrentPlayerIndex];
        currentPlayer.IsTurn = true;
    }

    public void NextTurn()
    {
        var currentPlayer = Players[CurrentPlayerIndex];
        currentPlayer.IsTurn = false;
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
        StartTurn();
    }

    public bool PlayCard(Player player, Card card)
    {
        if (IsValidMove(card))
        {
            player.Hand.Remove(card);
            return true;
        }
        return false;
    }

    public bool IsValidMove(Card card)
    {
        return true;
    }

    public void SkipTurn()
    {
        NextTurn();
    }

    public void ReverseOrder()
    {
        Players.Reverse();
        NextTurn();
    }
}