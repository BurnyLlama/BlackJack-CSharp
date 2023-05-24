using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    /// <summary>
    /// Holds a card deck.
    /// You can draw one or multiple cards.
    /// </summary>
    internal class Deck
    {
        private Stack<Card> _cards;
        public Deck()
        {
            List<Card> deck = _createSortedDeck();
            List<Card> shuffledDeck = _shuffleDeck(deck);
            _cards = new Stack<Card>(shuffledDeck);
        }

        private List<Card> _createSortedDeck()
        {
            List<Card> cards = new List<Card>();

            // Generate all possible cards in a deck in a sorted order.
            foreach (Card.EHouses house in Enum.GetValues(typeof(Card.EHouses)))
            {
                foreach (Card.EValues value in Enum.GetValues(typeof(Card.EValues)))
                {
                    Card card = new Card(house, value);
                    cards.Add(card);
                }
            }

            return cards;
        }

        private static List<Card> _shuffleDeck(List<Card> cards)
        {
            // Create a copy of all cards as to not consume the input.
            // This is to minimise any potential side effects, even though
            // it won't do anything for the current project except for
            // making it slightly slower.
            List<Card> cardsCopy = cards.Select(card => new Card(card.House, card.Value)).ToList();
            List<Card> shuffledCards = new List<Card>();

            int randomSeed = (int) DateTime.Now.ToBinary();
            Random rand = new Random(randomSeed);

            while (cardsCopy.Count > 0)
            {
                // Select a random card from the original sorted deck.
                int randomCardIndex = rand.Next(cardsCopy.Count);
                Card randomCard = cardsCopy[randomCardIndex];

                // Add the card to the shuffled deck and remove it from the cardsCopy array.
                shuffledCards.Add(randomCard);
                // Leads to `cardsAsList.Length -= 1` so that the while-loop eventually ends.
                cardsCopy.RemoveAt(randomCardIndex);
            }

            return shuffledCards;
        }

        public Card Draw()
        {
            Card card = _cards.Pop();
            return new Card(card.House, card.Value);
        }

        public List<Card> DrawMany(int amount)
        {
            List<Card> cards = new List<Card>();

            // Call the `Draw()` method `amount` times and return the drawn cards.
            for (int i = 0; i < amount; ++i)
            {
                cards.Add(Draw());
            }

            return cards;
        }
    }
}
