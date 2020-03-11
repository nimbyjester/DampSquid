using DampEngine;

namespace RenderTest
{
    class Program
    {
        static void Main()
        {
            DampCore.Window.Title = "Render Test";            
            DampCore.Run(new TestGame());
        }
    }
}
