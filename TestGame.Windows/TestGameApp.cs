using Stride.Engine;

namespace TestGame.Windows
{
    class TestGameApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
