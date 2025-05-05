namespace BlackjackGame
{
class GameManager
{
    private static RoundManager _RoundManager = new RoundManager();
    public static void Main(string[] args)
    {
        while (true)
        {
            if (_RoundManager.NewRound() == false)
            {
                break;
            }
        }
    }
}
class Card
{
    public Rank Rank {get;}
    public Suit Suit {get;}
    public byte Value {get;}
    public Card(Rank Rank, Suit Suit, byte Value)
    {
        this.Rank = Rank;
        this.Suit = Suit;
        this.Value = Value;
    }
    public override string ToString() => $"{Rank} of {Suit}";
}
public enum Rank : byte
{
    Ace = 1, Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10, King = 11, Queen = 12, Jack = 13
}
public enum Suit : byte
{
    Hearts = 1, Diamonds = 2, Clubs = 3, Spades = 4
}
public enum RoundState : byte
{
    NobodyWins = 0, PlayerWins = 1, DealerWins = 2
}
public enum GameRules
{
    Hit = 1, Stay = 2, Exit = 3, Blackjack = 21
}
class Hand
{
    private Card[] Cards = new Card[11];
    private int TopIndex = 0;
    private byte Value;
    public void AddCard(Card Card)
    {
        Cards[TopIndex] = Card;
        TopIndex++;
    }
    public void ClearHand()
    {
        TopIndex = 0;
        Array.Clear(Cards, 0, Cards.Length);
    }
    public void ShowHand()
    {
        foreach (Card Card in Cards)
        {
            if (Card != null)
            {
                Console.Write(Card.ToString() + ", ");
            }
        }
        Console.Write("\n");
    }
    public byte CalculateValue()
    {
        Value = 0;
        foreach (Card Card in Cards)
        {
            if (Card != null)
            {
                if (Card.Rank == Rank.Ace && Value <= 10)
                {
                    Value += 11;
                }
                else if (Card.Rank == Rank.Ace && Value > 10)
                {
                    Value += 1;
                }
                else
                {
                    Value += Card.Value;
                }
            }
        }
        return Value;
    }
}
class CardDeck
{
    private Card[] Deck = new Card[52];
    private static readonly Random RNG = new Random();
    private int TopIndex;
    public CardDeck()
    {
        int CardIndex = 0;
        for (byte SuitValue = 1; SuitValue <= 4; SuitValue++)
        {
            for (byte RankValue = 1; RankValue <= 13; RankValue++)
            {
                if (RankValue == (byte)Rank.Ace)
                {
                    Deck[CardIndex] = new Card((Rank)RankValue, (Suit)SuitValue, 0);
                }
                else if (RankValue == (byte)Rank.King || RankValue == (byte)Rank.Queen || RankValue == (byte)Rank.Jack)
                {
                    Deck[CardIndex] = new Card((Rank)RankValue, (Suit)SuitValue, 10);
                }
                else
                {
                    Deck[CardIndex] = new Card((Rank)RankValue, (Suit)SuitValue, RankValue);
                }
                CardIndex++;
            }
        }
    }
    public void ResetDeck() // TODO: Implement Fisher-Yates shuffle.
    {
        TopIndex = 0;
        for (int ResetLoop = Deck.Length - 1; ResetLoop > 0; ResetLoop--)
        {
        int Selected = RNG.Next(ResetLoop + 1);
        var Temporary = Deck[ResetLoop];
        Deck[ResetLoop] = Deck[Selected];
        Deck[Selected] = Temporary;
        }
    }
    public void AddCardToHand(Hand Target)
    {
        Target.AddCard(Deck[TopIndex]); 
        TopIndex++;
    }
}
class RoundManager
{
    private static readonly Random RNG = new Random();
    private Hand PlayerHand = new Hand();
    private Hand DealerHand = new Hand();
    private bool isPlayerActive = true;
    private byte PlayerInput = 0;
    private byte ScoreCheckResult = 0;
    CardDeck Deck = new CardDeck();
    public bool NewRound()
    {
        while (true)
        {
            Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine("Starting new round...");
            ConfigureRound();
            GiveStartCards();
            DisplayHands();

            ScoreCheckResult = CheckScores();
            if (ScoreCheckResult == (byte)RoundState.PlayerWins)
            {
                Console.WriteLine("\nPlayer wins with a score of " + PlayerHand.CalculateValue() + ".\n");
                break;
            }
            else if (ScoreCheckResult == (byte)RoundState.DealerWins)
            {
                Console.WriteLine("\nDealer wins with a score of " + DealerHand.CalculateValue() + ".\n");
                break;
            }

            for (int Turns = 0; Turns < 10; Turns++)
            {
                if (Turns > 0)
                {
                    DisplayHands();
                }
                ScoreCheckResult = CheckScores();
                if (ScoreCheckResult == (byte)RoundState.PlayerWins)
                {
                    Console.WriteLine("\nPlayer wins with a score of " + PlayerHand.CalculateValue() + ".\n");
                    break;
                }
                else if (ScoreCheckResult == (byte)RoundState.DealerWins)
                {
                    Console.WriteLine("\nDealer wins with a score of " + DealerHand.CalculateValue() + ".\n");
                    break;
                }
                if (isPlayerActive == true)
                {
                    PlayerInput = GetPlayerInput();
                    if (PlayerInput == (byte)GameRules.Hit)
                    {
                        Console.WriteLine("Player has chosen to hit.");
                    }
                    else if (PlayerInput == (byte)GameRules.Stay)
                    {
                        Console.WriteLine("Player has chosen to stay.");
                        isPlayerActive = false;
                    }
                    else if (PlayerInput == (byte)GameRules.Exit)
                    {
                        Console.WriteLine("Thanks for playing! Exiting game...");
                        return false;
                    }
                }
                GiveCards();
            }
        }
        return true;
    }
    private void ConfigureRound()
    {
        isPlayerActive = true;
        PlayerHand.ClearHand();
        DealerHand.ClearHand();
        Deck.ResetDeck();
    }
    private void GiveStartCards()
    {
        for (int StartCards = 0; StartCards < 2; StartCards++)
        {            
            GiveCards();
        }
    }
    private void GiveCards()
    {
        if (isPlayerActive == true)
        {
            Deck.AddCardToHand(PlayerHand); 
        }
        Deck.AddCardToHand(DealerHand);
    }
    private byte CheckScores()
    {   
        byte PlayerScore = PlayerHand.CalculateValue();
        byte DealerScore = DealerHand.CalculateValue();
        if (PlayerScore == (byte)GameRules.Blackjack)
        {
            return (byte)RoundState.PlayerWins;
        }
        else if (PlayerScore > (byte)GameRules.Blackjack)
        {
            return (byte)RoundState.DealerWins;
        }
        if (DealerScore == (byte)GameRules.Blackjack)
        {
            return (byte)RoundState.DealerWins;
        }
        else if (DealerScore > (byte)GameRules.Blackjack)
        {
            return (byte)RoundState.PlayerWins;
        }
        else
        {
            return (byte)RoundState.NobodyWins;
        }
    }
    private byte GetPlayerInput()
    {
        while (true)
        {
            Console.WriteLine("\nType 1 to draw another card, 2 to stop drawing cards, or 3 to quit the game.\n");
            byte Input = byte.Parse(Console.ReadLine());
            if (Input == (byte)GameRules.Hit)
            {
                return (byte)GameRules.Hit;
            }
            else if (Input == (byte)GameRules.Stay)
            {
                return (byte)GameRules.Stay;
            }
            else if (Input == (byte)GameRules.Exit)
            {
                return (byte)GameRules.Exit;
            }
            else
            {
                Console.WriteLine("Input not recognised! Your last input: '" + Input + "'\n");
            }
        }
    }
    private void DisplayHands()
    {
        Console.WriteLine("\nPlayer score: " + PlayerHand.CalculateValue());
        PlayerHand.ShowHand();
        Console.WriteLine("\nDealer score: " + DealerHand.CalculateValue());
        DealerHand.ShowHand();
    }
}
}