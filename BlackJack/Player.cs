using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    /// <summary>
    /// Holds all properties for a player.
    /// Note: Both the human and the computer playing the game are technically players.
    /// </summary>
    internal class Player
    {
        public bool IsDealer;

        protected int _coins;
        public int Coins { get => _coins; }

        protected List<Card> _hand;
        // Copy the hand so that it cannot be modified elsewhere!
        // This might add some (in this case not noticable) slowdown, but is worth it for security.
        public List<Card> Hand { get => _hand.Where(card => card != null).Select(card => new Card(card.House, card.Value)).ToList(); }

        protected int _bet;
        public int Bet { get => _bet; }

        public Player(bool isDealer, int coins)
        {
            IsDealer = isDealer;
            _coins = coins;

            _hand = new List<Card>();
            _bet = 0;
        }

        /// <summary>
        /// Adds one card to the player's hand-
        /// </summary>
        /// <param name="card"></param>
        public void ReceiveCard(Card card)
        {
            _hand.Add(new Card(card.House, card.Value));
        }

        /// <summary>
        /// Adds multiple cards to the player's hand.
        /// </summary>
        /// <param name="cards"></param>
        public void ReceiveCards(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                ReceiveCard(card);
            }
        }

        /// <summary>
        /// Removes all cards from the player's hand.
        /// </summary>
        public void ClearHand()
        {
            _hand = new List<Card>();
        }

        /// <summary>
        /// Place a bet. This also withdraws money from the player's balance.
        /// </summary>
        /// <param name="bet"></param>
        public void PlaceBet(int bet)
        {
            _bet = bet;
            _coins -= bet;
        }

        /// <summary>
        /// Increases the bet to a new bet.
        /// NOTE: This is increase TO, not increase BY!
        /// Withdraws the extra money needed from the player's balance.
        /// </summary>
        /// <param name="newBet"></param>
        public void IncreaseBetTo(int newBet)
        {
            int betDifference = newBet - _bet;
            _bet += betDifference;
            _coins -= betDifference;
        }

        
        /// <summary>
        /// Add coins to the player's balance.
        /// </summary>
        /// <param name="coins"></param>
        public void ReceiveCoins(int coins)
        {
            if (coins < 0)
            {
                return;
            }

            _coins += coins;
        }

        /// <summary>
        /// Returns the cards as a space-separated string.
        /// See Card.ToString() for format of card.
        /// </summary>
        /// <returns></returns>
        public string HandAsString()
        {
            return _hand.Aggregate("", (acc, card) => acc += card.ToString() + " ");
        }

        /// <summary>
        /// Calculate total sum of cards on hand.
        /// An ace is worth 11 points, unless that would make the sum go above 21,
        /// in which case the ace is worth one point.
        /// </summary>
        /// <returns></returns>
        public int TotalPointsInHand()
        {
            //return _hand.Sum(card => card.Points);
            int sum = 0;
            // Sort the cards by highest cards first (ace = 1 point) to account
            // so that we know if the ace would make the sum go above 21.
            foreach (Card card in _hand.OrderByDescending(card => card.Points))
            {
                if (card.Value == Card.EValues.A && sum <= 10)
                {
                    sum += 11;
                }
                else
                {
                    sum += card.Points;
                }
            }
            return sum;
        }
    }
}
