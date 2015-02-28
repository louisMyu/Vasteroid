using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorShooter
{
    public class ParticleManager<T>
    {
        public class Particle
        {
            public Texture2D Texture;
            public Vector2 Position;
            public float Orientation;

            public Vector2 Scale = Vector2.One;

            public Color Color;
            public float Duration;
            public float PercentLife = 1f;
            public T State;
        }

        private class CircularParticleArray
        {
            private int start;
            public int Start
            {
                get { return start; }
                set { start = value % list.Length; }
            }

            public int Count { get; set; }
            public int Capacity { get { return list.Length; } }
            private Particle[] list;

            public CircularParticleArray(int capacity)
            {
                list = new Particle[capacity];
            }

            public Particle this[int i]
            {
                get { return list[(start + i) % list.Length]; }
                set { list[(start + i) % list.Length] = value; }
            }
        }

        // This delegate will be called for each particle.
        private Action<Particle> updateParticle;
        private CircularParticleArray particleList;

        public ParticleManager(int capacity, Action<Particle> updateParticle)
        {
            this.updateParticle = updateParticle;
            particleList = new CircularParticleArray(capacity);

            // Populate the list with empty particle objects, for reuse.
            for (int i = 0; i < capacity; i++)
                particleList[i] = new Particle();
        }

		public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, float scale, T state, float theta = 0)
		{
			CreateParticle(texture, position, tint, duration, new Vector2(scale), state, theta);
		}

		public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, Vector2 scale, T state, float theta = 0)
		{
			Particle particle;
			if (particleList.Count == particleList.Capacity)
			{
				// if the list is full, overwrite the oldest particle, and rotate the circular list
				particle = particleList[0];
				particleList.Start++;
			}
			else
			{
				particle = particleList[particleList.Count];
				particleList.Count++;
			}

			// Create the particle
			particle.Texture = texture;
			particle.Position = position;
			particle.Color = tint;

			particle.Duration = duration;
			particle.PercentLife = 1f;
			particle.Scale = scale;
			particle.Orientation = theta;
			particle.State = state;
		}

        public void Update()
        {
            int removalCount = 0;
            for (int i = 0; i < particleList.Count; i++)
            {
                var particle = particleList[i];
                updateParticle(particle);
                particle.PercentLife -= 1f / particle.Duration;

                // sift deleted particles to the end of the list
                Swap(particleList, i - removalCount, i);

                // if the particle has expired, delete this particle
                if (particle.PercentLife < 0)
                    removalCount++;
            }
            particleList.Count -= removalCount;
        }

        private static void Swap(CircularParticleArray list, int index1, int index2)
        {
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < particleList.Count; i++)
            {
                var particle = particleList[i];

                Vector2 origin = new Vector2(particle.Texture.Width / 2, particle.Texture.Height / 2);
                spriteBatch.Draw(particle.Texture, particle.Position, null, particle.Color, particle.Orientation, origin, particle.Scale, 0, 0);
            }
        }
    }

    public enum ParticleType { None, Enemy, Bullet, IgnoreGravity }

    public struct ParticleState
    {
        public Vector2 Velocity;
        public ParticleType Type;
        public float LengthMultiplier;

        public static void UpdateParticle(ParticleManager<ParticleState>.Particle particle)
		{
			var vel = particle.State.Velocity;
			float speed = vel.Length();

			// using Vector2.Add() should be slightly faster than doing "x.Position += vel;" because the Vector2s
			// are passed by reference and don't need to be copied. Since we may have to update a very large 
			// number of particles, this method is a good candidate for optimizations.
			Vector2.Add(ref particle.Position, ref vel, out particle.Position);

			// fade the particle if its PercentLife or speed is low.
			float alpha = Math.Min(1, Math.Min(particle.PercentLife * 2, speed * 1f));
			alpha *= alpha;

			particle.Color.A = (byte)(255 * alpha);

			// the length of bullet particles will be less dependent on their speed than other particles
			if (particle.State.Type == ParticleType.Bullet)
				particle.Scale.X = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.1f * speed + 0.1f), alpha);
			else
				particle.Scale.X = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.2f * speed + 0.1f), alpha);

			particle.Orientation = vel.ToAngle();

			var pos = particle.Position;
			int width = (int)Game1.ScreenSize.X;
			int height = (int)Game1.ScreenSize.Y;

			// collide with the edges of the screen
			if (pos.X < 0)
				vel.X = Math.Abs(vel.X);
			else if (pos.X > width)
				vel.X = -Math.Abs(vel.X);
			if (pos.Y < 0)
				vel.Y = Math.Abs(vel.Y);
			else if (pos.Y > height)
				vel.Y = -Math.Abs(vel.Y);

			if (particle.State.Type != ParticleType.IgnoreGravity)
			{
                //foreach (var blackHole in EntityManager.BlackHoles)
                //{
                //    var dPos = blackHole.Position - pos;
                //    float distance = dPos.Length();
                //    var n = dPos / distance;
                //    vel += 10000 * n / (distance * distance + 10000);

                //    // add tangential acceleration for nearby particles
                //    if (distance < 400)
                //        vel += 45 * new Vector2(n.Y, -n.X) / (distance + 100);
                //}
			}

			if (Math.Abs(vel.X) + Math.Abs(vel.Y) < 0.00000000001f)	// denormalized floats cause significant performance issues
				vel = Vector2.Zero;
			else if (particle.State.Type == ParticleType.Enemy)
				vel *= 0.94f;
			else
				vel *= 0.96f + Math.Abs(pos.X) % 0.04f;	// rand.Next() isn't thread-safe, so use the position for pseudo-randomness

			particle.State.Velocity = vel;
		}
    }
}
