using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    internal class GameState
    {
        // Refers to the actual human playing the game.
        private Player _human;
        // Refers to the """AI""" playing the game against the human.
        private Computer _computer;
       
        // Get the bettor and the dealer respectively.
        // The computer and human take turns being bettor and dealer respectively.
        private Player _bettor { get => _human.IsDealer ? _computer : _human; }
        private Player _dealer { get => _human.IsDealer ? _human : _computer; }
        // The above is the best way I have found to do this, as my previous
        // method of using references like this:
        //      _bettor = _human;
        //      _dealer = _computer;
        // caused some issues... I think the garbage collector or references
        // didn't work the way I expected.

        private Deck _deck;

        // Whether or not the human has sold their soul to the Devil.
        // (Don't worry about it...)
        private bool _hasSoldSoul;

        public GameState()
        {
            _human = new Player(false, 500);
            _computer = new Computer(true, 15_000);
            _deck = new Deck();
            _hasSoldSoul = false;
        }

        /// <summary>
        /// This starts the game loop. It runs until error, an ending, or the player chooses to quit.
        /// </summary>
        public void Run()
        {
            // Inform the player about the game...
            _tellHuman(SGR.Bold + "Welcome to BlackJack!");
            _tellHuman("In this game aces are worth 11 points, unless they would make your hand go above 21, in which case they're worth 1 point!");
            _tellHuman("The game has four endingins -- try finding them all!");
            _tellHuman("Anyways... Let the game begin! Good luck!");
            _padLine();

            bool letGameContinueRunning = true;
            while (letGameContinueRunning)
            {
                // Inform the player of whether they are dealer or bettor.
                // This can be used for the player to choose diffrent strategies,
                // but also lessen any confusion about the game.
                string humansRole = _human.IsDealer ? "dealer" : "bettor";
                _tellHuman($"You are the {SGR.BrightCyan}{humansRole}{SGR.Reset} this round!");
                _padLine();

                // Give the player information about their balance.
                _printBalance();
                if (_human.Coins < 25)
                {
                    _tellHuman("You are very poor, maybe you should stop playing and get a jo-");
                    _tellHuman("Sorry, what I meant to say was: you should bet it all for higher rewards when you win!");
                }

                // Start the betting.
                _tellHuman("It is betting time!");
                _betting();
                if (_dealer.Bet > _bettor.Bet)
                {
                    _askIfBettorWantToMatchDealersBet();
                }
                _tellHuman("Final bets were:");
                _tellHuman($"({SGR.BrightBlue}Bettor{SGR.Reset}) {_bettor.Bet} -- {_dealer.Bet} ({SGR.BrightBlue}Dealer{SGR.Reset})");
                _padLine();

                // Deal two cards each to the bettor and dealer.
                _tellHuman("Alright, let's deal the initial cards...");
                _dealInitialCards();
                _padLine();

                // Now the computer and human can choose if they want to double their bet.
                _askToDoubleBet();
                _padLine();

                // Present the initial cards.
                // If the human is dealer it gets to know all the computer's cards.
                // If the human is bettor, it only gets to know one of the computer's cards.
                if (_human.IsDealer)
                {
                    _tellHuman($"Since you are the dealer, you'll get to see the {SGR.BrightBlue}bettor's{SGR.Reset} initial cards:" + _bettor.HandAsString());
                }
                else
                {
                    _tellHuman($"You are the {SGR.BrightBlue}bettor{SGR.Reset}, but you will get a preview of one of the {SGR.BrightBlue}dealer's{SGR.Reset} cards:");
                    _tellHuman($"One of the {SGR.BrightBlue}dealer's{SGR.Reset} cards is: " + _dealer.Hand[0].ToString());
                }
                _padLine();

                // Let the bettor draw more cards if they want to.
                _tellHuman($"Alright, let's ask if the {SGR.BrightBlue}bettor{SGR.Reset} wants more cards...");
                bool isDealerAbove21 = false;
                bool isBettorAbove21 = _askIfBettorWantsMoreCards();

                // Inform of the final hand the bettor has...
                _tellHuman($"Final hand of {SGR.BrightBlue}bettor{SGR.Reset}: {_bettor.HandAsString()} ({_bettor.TotalPointsInHand()} points)");
                _padLine();

                // There's only a need to let the dealer draw cards if the bettor gets busted.
                if (!isBettorAbove21)
                {
                    _tellHuman($"Alright, let's ask if the {SGR.BrightBlue}dealer{SGR.Reset} wants more cards...");
                    isDealerAbove21 = _letDealerTakeCards();
                }

                // Inform of the final hand the dealer has...
                _tellHuman($"Final hand of {SGR.BrightBlue}dealer{SGR.Reset}: {_dealer.HandAsString()} ({_dealer.TotalPointsInHand()} points)");
                _padLine();

                // Check who won -- or if it was a draw.
                _tellHuman("So... the game is over. Let's see who won!");
                Player[] winners = _checkWinners(isBettorAbove21, isDealerAbove21);
                _padLine();

                // Give out money to the winner(s).
                _tellHuman("Okay, let's turn out all prices!");
                _giveWinnersMoney(winners);
                _tellHuman($"Congratulations to the winner{(winners.Length == 1 ? "" : "s")}!");
                // Inform the human of their new balance.
                _printBalance();

                // Check if the player goes bankrupt...
                // The player can't continue playing without coins.
                bool isPlayerBankrupt = _checkPlayerBankruptcy();
                if (!isPlayerBankrupt)
                {
                   // Ask if the player wants to quit earlier...
                    letGameContinueRunning = _askPlayerIfWantsToPlayMore();
                }
                else
                {
                    letGameContinueRunning = false;
                }

                // The computer bankrupts, end the game.
                bool isComputerBankrupt = _checkComputerBankrupcy();
                if (isComputerBankrupt)
                {
                    letGameContinueRunning = false;
                }

                // This let's the human and computer switch roles.
                _switchBettorAndDealer();
                _padLine();
            }
        }

        /// <summary>
        /// Pad the terminal by adding an empty line.
        /// </summary>
        private void _padLine()
        {
            Thread.Sleep(100);
            Console.WriteLine();
        }

        /// <summary>
        /// This is a "glorified" `Console.WriteLine()`.
        /// What is does is prepend messages with `[Game Says]:`.
        /// It also writes out the message character by character.
        /// </summary>
        /// <param name="prompt"></param>
        private void _tellHuman(string prompt)
        {
            // Wait a bit before starting to write...
            Thread.Sleep(100);
            // Write one character at a time for an "rpg style" dialog system.
            foreach (char character in $"\u001b[?25l{SGR.BrightCyan}[{SGR.BrightGreen}Game Says{SGR.BrightCyan}]:{SGR.Reset} " + prompt + SGR.Reset)
            {
                Console.Write(character);
                Thread.Sleep(7);
            }
            // End the line.
            Console.WriteLine();
        }

        /// <summary>
        /// This asks for and grabs input from the user.
        /// It handles errors and won't crash the program (AFAIK).
        /// </summary>
        /// <returns></returns>
        private string _grabInput()
        {
            while (true)
            {
                try
                {
                    Console.Write($"{SGR.BrightCyan}[{SGR.BrightRed}You{SGR.BrightCyan}]:{SGR.Reset} \u001b[?25h");
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

        /// <summary>
        /// This grabs a singular key press from the user.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Asks the user for an integer.
        /// This should also be error safe (AFAIK).
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Asks the user for an integer with a min and max value.
        /// This should also be error safe (AFAIK).
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Let the human choose what to do from a set of actions.
        /// This should also be error safe (AFAIK).
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="options"></param>
        private void _letHumanChooseActionAndRunIt(string prompt, Option[] options)
        {
            _tellHuman(prompt);

            // Print all options
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

                    // This is null-safe since the code above checks if an option exists...
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

        /// <summary>
        /// Let the human know their and the bank's/computer's balance.
        /// </summary>
        private void _printBalance()
        {
            _tellHuman($"Your balance is {SGR.BrightYellow}{_human.Coins} coins{SGR.Reset}!");
            _tellHuman($"The computer's balance is {SGR.BrightYellow}{_computer.Coins} coins{SGR.Reset}!");
        }

        // Let the betting take place.
        // The bettor is the first on to place a bet.
        // The dealer must at least match the bettor's bet, but can go higher.
        // TODO: Ask if the bettor want to match the dealer's bet if it is higher.
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
                _tellHuman($"The {SGR.BrightBlue}bettor's{SGR.Reset} bet is {computerBet} coins.");
                int humanBet = _askHumanForInt("How much do you want to bet?", computerBet, _human.Coins);
                _human.PlaceBet(humanBet);
            }
        }

        /// <summary>
        /// Let the bettor match the dealer's bet if they bet higher.
        /// </summary>
        private void _askIfBettorWantToMatchDealersBet()
        {
            bool shouldMatch = false;
            if (_human.IsDealer)
            {
                shouldMatch = _computer.ChooseToMatchBet(_human.Bet);
            }
            // The human has to have enough coins to be able to match.
            else if (_human.Coins > (_computer.Bet - _human.Bet))
            {
                Option[] options = new Option[]
                    {
                        new Option('M', $"Match the {SGR.BrightBlue}dealer's{SGR.Reset} bet of {SGR.BrightYellow}{_dealer.Bet} coins{SGR.Reset}.", () => shouldMatch = true),
                        new Option('K', $"Keep your bet of {SGR.BrightYellow}{_bettor.Bet} coins{SGR.Reset}.", () => shouldMatch = false),
                    };
                _letHumanChooseActionAndRunIt($"Seems the {SGR.BrightBlue}dealer{SGR.Reset} bets higher ({SGR.BrightYellow}{_dealer.Bet} coins{SGR.Reset}), do you want to match their bet?", options);
            }

            if (shouldMatch)
            {
                _tellHuman($"Interesting, seems as if the {SGR.BrightBlue}bettor{SGR.Reset} has chosen wants to match the {SGR.BrightBlue}dealer's{SGR.Reset} bet of {SGR.BrightYellow}{_dealer.Bet} coins{SGR.Reset}!");
                _bettor.IncreaseBetTo(_dealer.Bet);
            }
        }

        /// <summary>
        /// Deal out the initial two cards each.
        /// </summary>
        private void _dealInitialCards()
        {
            List<Card> cardsForBettor = _deck.DrawMany(2);
            List<Card> cardsForDealer = _deck.DrawMany(2);

            _bettor.ReceiveCards(cardsForBettor);
            _dealer.ReceiveCards(cardsForDealer);
        }

        /// <summary>
        /// After the human and the computer have seen their cards they can choose to double their bet.
        /// This only works if they have enough coins to do it!
        /// </summary>
        private void _askToDoubleBet()
        {
            if (_human.Coins >= _human.Bet)
            {
                // Inform the user of their hand:
                _tellHuman($"You have the following cards: {_human.HandAsString()} ({_human.TotalPointsInHand()} points)");
                bool shouldDouble = false;
                Option[] options = new Option[]
                    {
                        new Option('D', $"Double it! ({SGR.BrightYellow}{_human.Bet * 2} coins{SGR.Reset})", () => shouldDouble = true),
                        new Option('K', $"Keep your bet! ({SGR.BrightYellow}{_human.Bet} coins{SGR.Reset})", () => shouldDouble = false)
                    };
                _letHumanChooseActionAndRunIt("Now when you've gotten to see the cards, do you want to double your bet?", options);

                if (shouldDouble)
                {
                    _human.IncreaseBetTo(_human.Bet * 2);
                    _tellHuman($"You chose to double your bet! It is now {SGR.BrightYellow}{_human.Bet} coins{SGR.Reset}!");
                }
            }

            if (_computer.Coins > _computer.Bet)
            {
                bool shouldDouble = _computer.ChooseToDouble();

                if (shouldDouble)
                {
                    _computer.IncreaseBetTo(_computer.Bet * 2);
                    _tellHuman($"The {SGR.BrightBlue}{(_computer.IsDealer ? "dealer" : "bettor")}{SGR.Reset} chose to double their bet! It is now {SGR.BrightYellow}{_computer.Bet} coins{SGR.Reset}!");
                }
            }
        }

        /// <summary>
        /// Asks if the human wants to hit or not.
        /// </summary>
        /// <returns>Returns true if the human wants to hit.</returns>
        private bool _askIfHumanWantsToHit()
        {
            bool shouldHit = false;
            Option[] options =
                {
                    new Option('H', "Hit", () => shouldHit = true),
                    new Option('S', "Stand", () => shouldHit = false)
                };

            // Inform the user of their hand:
            _tellHuman($"You have the following cards: {_human.HandAsString()} ({_human.TotalPointsInHand()} points)");

            // Let the human choose whether or not to hit.
            _letHumanChooseActionAndRunIt("Would you like more cards, or do you want to stop?", options);
            return shouldHit;
        }

        /// <summary>
        /// Returns true if the bettor goes above 21 points; busts.
        /// </summary>
        /// <returns></returns>
        private bool _askIfBettorWantsMoreCards()
        {
            bool shouldHit = false;

            if (_human.IsDealer)
            {
                // computer is bettor
                shouldHit = _computer.ChooseToHitOrNot();
            }
            else
            {
                // human is bettor
                shouldHit = _askIfHumanWantsToHit();
            }

            if (!shouldHit)
                return false;

            Card card = _deck.Draw();
            _tellHuman($"The {SGR.BrightBlue}bettor{SGR.Reset} drew a {card.ToString()}!");
            _bettor.ReceiveCard(card);

            if (_bettor.TotalPointsInHand() > 21)
                return true;

            // Recursively run until the bettor stop or their hand gets above 21.
            return _askIfBettorWantsMoreCards();
        }

        /// <summary>
        /// Returns true if the dealer goes above 21; busts.
        /// </summary>
        /// <returns></returns>
        private bool _letDealerTakeCards()
        {
            bool shouldHit = false;

            if (_computer.IsDealer)
            {
                // computer is dealer
                shouldHit = _computer.ChooseToHitOrNot(_human.TotalPointsInHand());
            }
            else
            {
                // human is dealer
                shouldHit = _askIfHumanWantsToHit();
            }

            if (!shouldHit)
                return false;

            Card card = _deck.Draw();
            _tellHuman($"The {SGR.BrightBlue}dealer{SGR.Reset} drew a {card.ToString()}!");
            _dealer.ReceiveCard(card);

            if (_dealer.TotalPointsInHand() > 21)
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
        /// <returns>Returns both the computer and human if it's a draw.</returns>
        private Player[] _checkWinners(bool isBettorAbove21, bool isDealerAbove21)
        {
            // Remind the human of their role, as all dialog is "role based".
            string humanRole = _human.IsDealer ? "dealer" : "bettor";
            _tellHuman($"Remember that you are the {SGR.BrightCyan}{humanRole}{SGR.Reset}!");

            if (isBettorAbove21)
            {
                _tellHuman($"Ouch! Seems like the {SGR.BrightBlue}bettor{SGR.Reset} has busted! The {SGR.BrightBlue}dealer{SGR.Reset} is the winner!");
                return new Player[] { _dealer };
            }

            if (isDealerAbove21)
            {
                _tellHuman($"Ouch! Seems like the {SGR.BrightBlue}dealer{SGR.Reset} has busted! The {SGR.BrightBlue}bettor{SGR.Reset} is the winner!");
                return new Player[] { _bettor };
            }

            int bettorSum = _bettor.TotalPointsInHand();
            int dealerSum = _dealer.TotalPointsInHand();

            if (bettorSum == dealerSum)
            {
                _tellHuman("Interesting! A tie! Seems like both of you won...");
                return new Player[] { _bettor, _dealer };
            }

            if (bettorSum > dealerSum)
            {
                _tellHuman($"The {SGR.BrightBlue}bettor{SGR.Reset} has won! With {bettorSum}-{dealerSum}");
                return new Player[] { _bettor };
            }

            _tellHuman($"The {SGR.BrightBlue}dealer{SGR.Reset} has won! With {dealerSum}-{bettorSum}");
            return new Player[] { _dealer };
        }

        /// <summary>
        /// Give out the coins to the winner(s).
        /// </summary>
        /// <param name="winners"></param>
        private void _giveWinnersMoney(Player[] winners)
        {
            // Price money is the computer's and player's bet plus a little extra.
            // The price money is divided between all winners (both if tie).
            int totalBets = _human.Bet + _computer.Bet;
            int priceMoney = (int)(totalBets * 1.25d) / winners.Length;
            _tellHuman($"The price money is {SGR.BrightYellow}{priceMoney} coins{SGR.Reset}!");

            foreach (Player winner  in winners)
            {
                if (winner.IsDealer)
                {
                    _tellHuman($"Let's give out money to the {SGR.BrightBlue}dealer{SGR.Reset}!");
                }
                else
                {
                    _tellHuman($"Let's give out money to the {SGR.BrightBlue}bettor{SGR.Reset}!");
                }

                winner.ReceiveCoins(priceMoney);
            }
        }

        /// <summary>
        /// Ask if the player wants to play more or quit.
        /// </summary>
        /// <returns>Returns true if the player wants to play more.</returns>
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

        /// <summary>
        /// Switch around the roles of bettor and dealer among the human and computer.
        /// </summary>
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

        /// <summary>
        /// Check if the player goes bankrupt.
        /// The player cannot continue playing if bankrupt.
        /// </summary>
        /// <returns>Returns true if the player is bankrupt.</returns>
        private bool _checkPlayerBankruptcy()
        {
            if (_human.Coins >= 1)
            {
                return false;
            }

            // Ending #2
            if (_hasSoldSoul)
            {
                _tellHuman("Oh no! Seems as if you're broke again...");
                _tellHuman("Well, that is what happens when you gamble too much...");
                _tellHuman($"Oh... who do we have here... It's the {SGR.Red}Devil{SGR.Reset}!");
                _tellHuman($"The {SGR.Red}Devil{SGR.Reset} tells you that you are to no worth; your soul has already been sold.");
                _tellHuman("So, it seems our journey ends here... Bye!");
                _tellHuman($"{SGR.BrightYellow}You have discovered {SGR.Green}Ending #2{SGR.BrightYellow}!");
                return true;
            }

            _tellHuman("Oh no! Seems as if you're broke now...");
            _tellHuman("Well, that is what happens when you gamble too much...");
            _tellHuman($"Oh... who do we have here... It's the {SGR.Red}Devil{SGR.Reset}!");
            bool shouldSellSoul = false;
            Option[] options =
                {
                    new Option('S', $"Sell your soul to the {SGR.Red}Devil{SGR.Reset} and receive {SGR.BrightYellow}5000 coins{SGR.Reset}.", () => shouldSellSoul = true),
                    new Option('Q', "Quit gambling and get a healthier life.", () => shouldSellSoul = false)
                };

            _letHumanChooseActionAndRunIt($"The {SGR.Red}Devil{SGR.Reset} offers you to sell your soul in return for {SGR.BrightYellow}5000 coins{SGR.Reset}... What do you do?", options);

            if (shouldSellSoul)
            {
                _tellHuman($"The {SGR.Red}Devil{SGR.Reset} seems satisfied with your deal. You now have {SGR.BrightYellow}5000 coins{SGR.Reset}!");
                _human.ReceiveCoins(5_000);
                _hasSoldSoul = true;
                return false;
            }
            else
            {
                // Ending #1
                _tellHuman($"The {SGR.Red}Devil{SGR.Reset} isn't pleased with you. He dismisses you...");
                _tellHuman("You wanted to live a healthy life without gambling...");
                _tellHuman("Sadly, no money means broke, broke means no food, no food means no life...");
                _tellHuman("I am sorry it had to end this way...");
                _tellHuman($"{SGR.BrightYellow}You have discovered {SGR.Green}Ending #1{SGR.BrightYellow}!");
                return true;
            }
        }

        private bool _checkComputerBankrupcy()
        {
            if (_computer.Coins > 0)
            {
                return false;
            }

            _tellHuman("Interesting... it seems the computer is bankrupt.");
            _tellHuman("This must mean you beat the casino!");
            _tellHuman("I am surprised. I didn't think this was possible...");

            if (_hasSoldSoul)
            {
                // Ending #3
                _tellHuman("Too bad it required selling your soul...");
                _tellHuman("I am sure you didn't need it anyways... congratulations!");
                _tellHuman($"{SGR.BrightYellow}You have discovered {SGR.Green}Ending #3{SGR.BrightYellow}!");
            }
            else
            {
                // Ending #4
                _tellHuman($"I am surprised you could do this without ... I don't know... something like selling you soul to the {SGR.Red}Devil{SGR.Reset}.");
                _tellHuman("Congratulations! And hat's off to you!");
                _tellHuman($"{SGR.BrightYellow}You have discovered {SGR.Green}Ending #4{SGR.BrightYellow}!");
            }

            return true;
        }
    }
}
