namespace BlackJack
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Create a game state.
            GameState game = new GameState();
            game.Run();
        }
    }
}