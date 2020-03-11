using System;
using System.Diagnostics;

namespace DampEngine
{
    public class Update
    {
        private Stopwatch gameTime = new Stopwatch();
        private long previousFrameTicks = 0;
        private long currentFrameTicks = 0;
        public Double ElapsedSeconds
        {
            get => gameTime.ElapsedTicks / (double)Stopwatch.Frequency;
        }
        public Double ElapsedDeltaSeconds
        {
            get => (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
        }

        internal void Start()
        {            
            gameTime.Start();
        }
        internal void StartUpdateDelta()
        {
            currentFrameTicks = gameTime.ElapsedTicks;
        }
        internal void EndUpdateDelta()
        {
            previousFrameTicks = gameTime.ElapsedTicks;
        }
        internal void Stop()
        {
            gameTime.Stop();
        }
    }
}
