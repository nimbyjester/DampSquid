using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DampEngine
{
    public abstract class Game
    {
        private readonly Update update = new Update();
        protected readonly List<Scene> scenes = new List<Scene>();
        public IEnumerable<Scene> ActiveScenes => scenes.Where(s => s.IsActive);

        public void Start()
        {
            BeginStart();
            update.Start();
            EndStart();
        }
        public virtual void BeginStart() { }
        public virtual void EndStart() { }

        public void ActivateScene(Scene scene)
        {
            scene.CreateResources();
            scene.IsActive = true;
        }

        public void DeactivateScene(Scene scene)
        {
            scene.IsActive = false;
            scene.DisposeResources();
        }

        public void Dispose()
        {
            update.Stop();
            scenes.ForEach(s => s.DisposeResources());
        }

        internal void Draw()
        {
            foreach (var a in ActiveScenes.ToArray())
            {
                a.Draw();
            }
        }
        public void Update()
        {
            update.StartUpdateDelta();
            Update(update);
            update.EndUpdateDelta();
        }

        public abstract void Update(Update update);
    }
}
