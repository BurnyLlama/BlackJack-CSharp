﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    internal class Player
    {
        public bool IsDealer;

        protected int _coins;
        public int Coins { get => _coins; }

        protected List<Card> _hand;
        // Copy the hand so that it cannot be modified!
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

        public void ReceiveCard(Card card)
        {
            _hand.Add(new Card(card.House, card.Value));
        }

        public void ReceiveCards(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                ReceiveCard(card);
            }
        }

        public void ClearHand()
        {
            _hand = new List<Card>();
        }

        public void PlaceBet(int bet)
        {
            _bet = bet;
            _coins -= bet;
        }

        public void ReceiveCoins(int coins)
        {
            if (coins < 0)
            {
                return;
            }

            _coins += coins;
        }

        public string HandAsString()
        {
            return _hand.Aggregate("", (acc, card) => acc += card.ToString() + " ");
        }

        public int TotalPointsInHand()
        {
            return _hand.Sum(card => card.Points);
        }
    }
}