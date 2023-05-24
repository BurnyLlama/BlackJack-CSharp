using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    internal class GameState
    {
        struct Option
        {
            public readonly char KeyToPress;
            public readonly string Name;
            public readonly Action Func;

            public Option(char keyToPress, string name, Action func)
            {
                KeyToPress = keyToPress;
                Name = name;
                Func = func;
            }
        }

        private Player _human;
        private Computer _computer;
       
        // Get the bettor and the dealer respectively.
        private Player _bettor { get => _human.IsDealer ? _computer : _human; }
        private Player _dealer { get => _human.IsDealer ? _human : _computer; }

        private Deck _deck;

        private bool _hasSoldSoul;

        public GameState()
        {
            _human = new Player(false, 500);
            _computer = new Computer(true, 5_000_000);
            _deck = new Deck();
            _hasSoldSoul = false;
        }

        /// <summary>
        /// This starts the game loop.
        /// </summary>
        public void Run()
        {
            bool letGameContinueRunning = true;
            while (letGameContinueRunning)
            {
                string humansRole = _human.IsDealer ? "dealer" : "bettor";
                _tellHuman($"You are the {humansRole} this game!");

                _tellHuman($"Your bank account balance is {_human.Coins} coins.");
                if (_human.Coins < 25)
                {
                    _tellHuman("You are very poor, maybe you should stop playing and get a jo-");
                    _tellHuman("Sorry, what I meant to say was: you should bet it all for higher rewards when you win!");
                }

                _tellHuman("It is betting time!");
                _betting();
                _tellHuman("Final bets were:");
                _tellHuman($"(Bettor) {_bettor.Bet} -- {_dealer.Bet} (Dealer)");

                _tellHuman("Alright, let's deal the initial cards...");
                _dealInitialCards();

                // Present the initial cards
                if (_human.IsDealer)
                {
                    _tellHuman("Since you are the dealer, you'll get to see the bettor's initial cards:");
                    _tellHuman("The bettor's initial cards are: " + _bettor.HandAsString());
                }
                else
                {
                    _tellHuman("You are the bettor, but you will get a preview of one of the dealer's cards:");
                    _tellHuman("One of the dealer's cards is: " + _dealer.Hand[0].ToString());
                }

                _tellHuman("Alright, let's ask if the bettor wants more cards...");
                bool isDealerAbove21 = false;
                bool isBettorAbove21 = _askIfBettorWantsMoreCards();
                
                _tellHuman($"Final hand of bettor: {_bettor.HandAsString()} ({_bettor.TotalPointsInHand()} points)");
                
                // There's only a need to let the dealer draw cards if the bettor gets busted.
                if (!isBettorAbove21)
                {
                    _tellHuman("Alright, let's ask if the dealer wants more cards...");
                    isDealerAbove21 = _letDealerTakeCards();
                }

                _tellHuman($"Final hand of dealer: {_dealer.HandAsString()} ({_dealer.TotalPointsInHand()} points)");

                _tellHuman("So... the game is over. Let's see who won!");
                Player[] winners = _checkWinners(isBettorAbove21, isDealerAbove21);

                _tellHuman("Okay, let's turn out all prices!");
                _giveWinnersMoney(winners);
                _tellHuman($"Congratulations to the winner{(winners.Length == 1 ? "" : "s")}");

                bool isBankrupt = _checkBankruptcy();
                if (!isBankrupt)
                { 
                    letGameContinueRunning = _askPlayerIfWantsToPlayMore();
                }
                else
                {
                    letGameContinueRunning = false;
                }

                _switchBettorAndDealer();
            }
        }

        private string _grabInput()
        {
            while (true)
            {
                try
                {
                    Console.Write("> ");
                    string input = Console.ReadLine();
                    
                    if (input == null)
                    {
                        _tellHuman("Sorry, you need to provide a value! Please try again:");
                        continue;
                    }

                    return input;
                }
                catch
                {
                    _tellHuman("Something went wrong! Please try again:");
                }
            }
        }

        private char _grabKey()
        {
            while (true)
            {
                try
                {
                    Console.Write("Please press a key... ");
                    char input = Console.ReadKey().KeyChar;
                    // Write a new line so the next dialog isn't prepended with the user's option.
                    Console.WriteLine();
                    return input;
                }
                catch
                {
                    _tellHuman("Something went wrong! Please try again:");
                }
            }
        }

        private void _tellHuman(string prompt)
        {
            Thread.Sleep(150);
            // Write one character at a time for an "rpg style" dialog system.
            foreach (char character in "[Game Says]: " + prompt)
            {
                Console.Write(character);
                Thread.Sleep(8);
            }
            // End the line.
            Console.WriteLine();
        }

        private int _askHumanForInt()
        {
            while (true)
            {
                try
                {
                    string input = _grabInput();
                    int number = Convert.ToInt32(input);
                    return number;
                }
                catch
                {
                    Console.WriteLine("Sorry, that doesn't look like a number to me! Please try again.");
                }
            }
        }

        private int _askHumanForInt(string prompt, int min, int max)
        {
            _tellHuman(prompt);
            _tellHuman($"Your input should be between {min} and {max}.");
            while (true)
            {
                int number = _askHumanForInt();
                
                if (number < min)
                {
                    _tellHuman("Sorry, that number is too small! Please select a higher one.");
                    continue;
                }

                if (number > max)
                {
                    _tellHuman("Sorry, that number is too big! Please select a smaller one.");
                    continue;
                }

                return number;
            }
        }

        private void _letHumanChooseActionAndRunIt(string prompt, Option[] options)
        {
            _tellHuman(prompt);

            foreach (Option option in options)
            {
                Console.WriteLine($"  [{option.KeyToPress}] {option.Name}");
            }

            while (true)
            {
                // Scuffed way to get lowercase version of a char...
                char userChoice = _grabKey().ToString().ToLower().First();

                try
                {
                    if (!options.Any(option => option.KeyToPress.ToString().ToLower().First() == userChoice))
                    {
                        _tellHuman("Sorry, that is an invalid option! Please try again...");
                        continue;
                    }

                    Option chosenOption = Array.Find(options, option => option.KeyToPress.ToString().ToLower().First() == userChoice);
                    chosenOption.Func();
                    return;
                }
                catch
                {
                    _tellHuman("Sorry, that doesn't seem like a valid option. Please try another.");
                    continue;
                }
            }
        }

        private void _betting()
        {
            if (!_human.IsDealer)
            {
                int humanBet = _askHumanForInt("How much do you want to bet?", 1, _human.Coins);
                _human.PlaceBet(humanBet);
                int computerBet = _computer.ChooseBet(_human.Coins, _human.Bet);
                _computer.PlaceBet(computerBet);
            }
            else
            {
                int computerBet = _computer.ChooseBet(_human.Coins, 0);
                _computer.PlaceBet(computerBet);
                _tellHuman($"The bettor's bet is {computerBet} coins.");
                int humanBet = _askHumanForInt("How much do you want to bet?", computerBet, _human.Coins);
                _human.PlaceBet(humanBet);
            }
        }

        private void _dealInitialCards()
        {
            List<Card> cardsForBettor = _deck.DrawMany(2);
            List<Card> cardsForDealer = _deck.DrawMany(2);

            _bettor.ReceiveCards(cardsForBettor);
            _dealer.ReceiveCards(cardsForDealer);
        }

        private bool _askIfHumanWantsToHit()
        {
            bool shouldHit = false;
            Option[] options =
                {
                    new Option('H', "Hit", () => shouldHit = true),
                    new Option('S', "Stand", () => shouldHit = false)
                };
            _tellHuman("You have the following cards: " + _human.HandAsString());
            _tellHuman($"You have {_human.TotalPointsInHand()} points!");
            _letHumanChooseActionAndRunIt("Would you like more cards, or do you want to stop?", options);
            return shouldHit;
        }

        /// <summary>
        /// Returns true if the bettor gets above 21 points.
        /// </summary>
        /// <returns></returns>
        private bool _askIfBettorWantsMoreCards()
        {
            bool shouldHit = false;

            if (_human.IsDealer)
            {
                shouldHit = _computer.ChooseToHitOrNot(_human.Hand);
            }
            else
            {
                shouldHit = _askIfHumanWantsToHit();
            }

            if (!shouldHit)
                return false;

            Card card = _deck.Draw();
            _tellHuman($"The bettor drew a {card.ToString()}!");
            _bettor.ReceiveCard(card);

            if (_bettor.Hand.Sum(card => card.Points) > 21)
                return true;

            // Recursively run until the bettor stop or their hand gets above 21.
            return _askIfBettorWantsMoreCards();
        }

        /// <summary>
        /// Returns true if the dealer goes above 21.
        /// </summary>
        /// <returns></returns>
        private bool _letDealerTakeCards()
        {
            bool shouldHit = false;

            if (!_human.IsDealer)
            {
                shouldHit = _computer.ChooseToHitOrNot();
            }
            else
            {
                shouldHit = _askIfHumanWantsToHit();
            }

            if (!shouldHit)
                return false;

            Card card = _deck.Draw();
            _tellHuman($"The dealer drew a {card.ToString()}!");
            _dealer.ReceiveCard(card);

            if (_dealer.Hand.Sum(card => card.Points) > 21)
                return true;

            // Recursively run until the bettor stop or their hand gets above 21.
            return _letDealerTakeCards();
        }

        /// <summary>
        /// Returns an array of the winners. If both the player and the computer have the same
        /// amount of points, it's a draw and both are "winners".
        /// </summary>
        /// <param name="isBettorAbove21"></param>
        /// <param name="isDealerAbove21"></param>
        /// <returns></returns>
        private Player[] _checkWinners(bool isBettorAbove21, bool isDealerAbove21)
        {
            string humanRole = _human.IsDealer ? "dealer" : "bettor";
            _tellHuman($"Remember that you are the {humanRole}!");

            if (isBettorAbove21)
            {
                _tellHuman("Ouch! Seems like the bettor has busted! The dealer is the winner!");
                return new Player[] { _dealer };
            }

            if (isDealerAbove21)
            {
                _tellHuman("Ouch! Seems like the dealer has busted! The bettor is the winner!");
                return new Player[] { _bettor };
            }

            int bettorSum = _bettor.Hand.Sum(card => card.Points);
            int dealerSum = _dealer.Hand.Sum(card => card.Points);

            if (bettorSum == dealerSum)
            {
                _tellHuman("Interesting! A tie! Seems like both of you won...");
                return new Player[] { _bettor, _dealer };
            }

            if (bettorSum > dealerSum)
            {
                _tellHuman($"The bettor has won! With {bettorSum}-{dealerSum}");
                return new Player[] { _bettor };
            }

            _tellHuman($"The dealer has won! With {dealerSum}-{bettorSum}");
            return new Player[] { _dealer };
        }

        private void _giveWinnersMoney(Player[] winners)
        {
            int priceMoney = (_human.Bet + _computer.Bet) / winners.Length;
            _tellHuman($"The price money is {priceMoney} coins!");

            foreach (Player winner  in winners)
            {
                if (winner.IsDealer)
                {
                    _tellHuman("Let's give out money to the dealer!");
                }
                else
                {
                    _tellHuman("Let's give out money to the bettor!");
                }

                winner.ReceiveCoins(priceMoney);
            }

            _tellHuman($"Your new balance is {_human.Coins} coins!");
        }

        private bool _askPlayerIfWantsToPlayMore()
        {
            bool playerWantsOneMoreGame = false;
            Option[] options =
            {
                new Option('C', "Continue playing", () =>  playerWantsOneMoreGame = true),
                new Option('Q', "Quit game", () =>  playerWantsOneMoreGame = false),
            };
            _letHumanChooseActionAndRunIt("Do you want to play one more game?", options);
            return playerWantsOneMoreGame;
        }

        private void _switchBettorAndDealer()
        {
            // "Refresh" the player and computer...
            // Also make sure to switch who is the dealer.
            _human = new Player(!_human.IsDealer, _human.Coins);
            _computer = new Computer(!_computer.IsDealer, _computer.Coins);

            // Refresh the deck.
            _deck = new Deck();

            // Also clear their hands:
            _bettor.ClearHand();
            _dealer.ClearHand();
        }

        private bool _checkBankruptcy()
        {
            if (_human.Coins >= 1)
            {
                return false;
            }

            if (_hasSoldSoul)
            {
                _tellHuman("Oh no! Seems as if you're broke again...");
                _tellHuman("Well, that is what happens when you gamble too much...");
                _tellHuman("Oh... who do we have here... It's the Devil!");
                _tellHuman("The Devil tells you that you are to no worth; your soul has already been sold.");
                _tellHuman("So, it seems our journey ends here... Bye!");
                return true;
            }

            _tellHuman("Oh no! Seems as if you're broke now...");
            _tellHuman("Well, that is what happens when you gamble too much...");
            _tellHuman("Oh... who do we have here... It's the Devil!");
            bool shouldSellSoul = false;
            Option[] options =
                {
                    new Option('S', "Sell your soul to the devil and receive 5 000 coins.", () => shouldSellSoul = true),
                    new Option('Q', "Quit gambling and get a healthier life.", () => shouldSellSoul = false)
                };

            _letHumanChooseActionAndRunIt("The Devil offers you to sell your soul in return for 5 000 coins... What do you do?", options);

            if (shouldSellSoul)
            {
                _tellHuman("The Devil seems satisfied with your deal. You now have 5 000 coins!");
                _human.ReceiveCoins(5_000);
                _hasSoldSoul = true;
                return false;
            }
            else
            {
                _tellHuman("The Devil isn't pleased with you. He dismisses you...");
                _tellHuman("You wanted to live a healthy life without gambling...");
                _tellHuman("Sadly, no money means broke, broke means no food, no food means no life...");
                _tellHuman("I am sorry it had to end this way...");
                return true;
            }
        }
    }
}
