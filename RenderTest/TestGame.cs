using DampEngine;
using System;
using System.Linq;

namespace RenderTest
{
    public class TestGame : Game
    {
        public TestGame()
        {
            TestScene item = new TestScene();
            scenes.Add(item);
        }

        public override void EndStart()
        {
            ActivateScene(scenes.First());
        }

        public override void Update(Update update)
        {
            foreach (var a in ActiveScenes.ToArray())
            {
                a.Update(update);
            }
        }
    }
}
