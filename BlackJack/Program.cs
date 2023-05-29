using System.Text;

namespace BlackJack
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Use UTF-8 so I can have fancy characters... :)
            Console.OutputEncoding = Encoding.UTF8;
            // Create a game state.
            GameState game = new GameState();
            game.Run();
        }
    }
}