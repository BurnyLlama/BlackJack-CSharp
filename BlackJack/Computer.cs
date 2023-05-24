using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BlackJack
{   
    /// <summary>
    /// Some extra functions that the computer needs to be able to play.
    /// Unlike the human, it cannot think for itself, so it needs "AI".
    /// </summary>
    internal class Computer : Player
    {
        // I dunno if there is a nicer way to do this...
        public Computer(bool isDealer, int coins) : base(isDealer, coins) { }

        /// <summary>
        /// Returns true if the computer wants to hit.
        /// </summary>
        /// <returns></returns>
        public bool ChooseToHitOrNot()
        {
            int sumOfCardsInHand = _hand.Sum(card => card.Points);

            switch (sumOfCardsInHand)
            {
                case int n when n < 11: return true;
                // TODO: This should not be false by default.
                // I will come up with a "Good Black Jack Algorithm (tm)".
                case int n when n >= 11 && n < 21: return false;
                case int n when n >= 21: return false;
                // Deafult to not hit...
                default: return false;
            }
        }

        /// <summary>
        /// Same as above, but takes the players hand into account.
        /// </summary>
        /// <param name="playerHand"></param>
        /// <returns></returns>
        public bool ChooseToHitOrNot(List<Card> playerHand)
        {
            int sumOfCardsInHand = _hand.Sum(card => card.Points);
            int sumOfCardsInPlayerHand = playerHand.Sum(card => card.Points);

            // Don't take any more cards if the computer
            // already has more points than the player.
            if (sumOfCardsInHand > sumOfCardsInPlayerHand)
            {
                return false;
            }

            // Always safe to draw if sum <= 11
            if (sumOfCardsInHand <= 11)
            {
                return true;
            }

            // Else use an algorithm to choose whether or not to hit.
            if (sumOfCardsInHand < 21)
            {
                int randomSeed = (int)DateTime.Now.ToBinary();
                Random rand = new Random(randomSeed);
                // This algorithm chooses whether to hit or not depending on how many values.
                // It gets a percentage of how many card values would be "good" -- as in not cause a loss.
                // There are 13 different values. The good cards are the values that are less than
                // the difference between 21 (max points before a bust) and the current sum of hand.
                // The graph is plotted here: https://www.desmos.com/calculator/i322sajfng
                // It is valid where 11 < sumOfCardsInHand < 21.
                // A random number is then generated and if that number is less than the percentage
                // generated above, a card is drawn.
                bool shouldHit = rand.NextDouble() <= ((21.0d - (double) sumOfCardsInHand) / 13.0d);
                return shouldHit;
            }

            // If the execuion gets to this part the sum of cards on hand is <= 21,
            // which means there should be no more hits.
            return false;
        }

        public int ChooseBet(int playerMoney, int playerBet)
        {
            int randomSeed = (int)System.DateTime.Now.ToBinary();
            Random rand = new Random(randomSeed);

            // If `playerBet == 0`, the player hasn't chosen bet.
            // That means the computer will choose bet first.
            if (playerBet == 0)
            {
                // Choose how big part of the `playerMoney` to ask to bet for, randomly.
                int betDivisor = rand.Next(2, 8);
                int bet = playerMoney / betDivisor;
                return bet;
            }

            // Choose whether to not bet higher than the player. If so, choose an increase factor.
            bool shouldBetHigher = rand.Next(1, 12) > 5;
            if (shouldBetHigher)
            {
                double increaseFactor = 1.0d + (1.0d / rand.Next(10, 15));
                return (int)(playerBet * increaseFactor);
            }

            // If no increase, meet the player's bet.
            return playerBet;
        }
    }
}
