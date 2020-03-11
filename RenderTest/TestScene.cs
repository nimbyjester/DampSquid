using DampEngine;
using DampEngine.Drawing.Visual;
using DampEngine.Input;
using System;
using System.Numerics;
using Veldrid;

namespace RenderTest
{
    public class TestScene : Scene
    {
        TestQuad item = new TestQuad();
        public TestScene()
        {
            //TestQuad item = new TestQuad();
            item.transform.Position += new Vector3(5, 0, 0);
            item.speed = 0f;
            renderables.Add(item);

            //TestQuad item1 = new TestQuad();
            //item1.speed = -0.015f;
            //renderables.Add(item1);

            //TestQuad item2 = new TestQuad();
            //item2.transform.Position += new Vector3(-5, 0, 0);
            //item2.speed = -0.005f;
            //renderables.Add(item2);
        }

        public override void CreateResources()
        {
            renderables.ForEach(r => r.CreateResources());
        }

        public override void DisposeResources()
        {
            renderables.ForEach(r => r.DisposeResources());
            factory.DisposeCollector.DisposeAll();
        }

        public override void Update(Update gameTime)
        {
            if(InputTracker.GetKey(Key.W))
            {
                item.transform.Velocity += new Vector3(0, 0.001f, 0);
            }
            if (InputTracker.GetKey(Key.S))
            {
                item.transform.Velocity += new Vector3(0, -0.001f, 0);
            }
            if (InputTracker.GetKey(Key.A))
            {
                item.transform.Velocity += new Vector3(-0.001f, 0, 0);
            }
            if (InputTracker.GetKey(Key.D))
            {
                item.transform.Velocity += new Vector3(0.001f, 0, 0);
            }
            if (InputTracker.GetKey(Key.KeypadAdd))
            {
                item.transform.RotationVelocity += new Vector3(0, 0, 0.001f);
                // item.transform.RotationVelocity = Quaternion.Concatenate(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.1f), item.transform.RotationVelocity);
            }
            if (InputTracker.GetKey(Key.KeypadMinus))
            {
                item.transform.RotationVelocity += new Vector3(0, 0, -0.001f);
                // item.transform.RotationVelocity = Quaternion.Concatenate(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -0.1f), item.transform.RotationVelocity);
            }
            renderables.ForEach(r => r.Update(gameTime));
        }
    }
}
