namespace BlackJack
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to BlackJack!\n");

            Console.WriteLine("Here's a list of all symbols and what they mean:");
            Console.WriteLine("%\tSpades");
            Console.WriteLine("#\tHearts");
            Console.WriteLine("¤\tClubs");
            Console.WriteLine("*\tDiamonds");

            Console.WriteLine("\nLet the game begin!\n");

            GameState game = new GameState();
            game.Run();
        }
    }
}