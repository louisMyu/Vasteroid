using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorShooter
{
    public class PlayerShip : Entity
    {
        private static PlayerShip instance;
        int framesUntilRespawn = 0;
        public bool IsDead { get { return framesUntilRespawn > 0; } }

        const float SPEED = 15;

        const int cooldownFrames = 6;
        int cooldownRemaining = 0;
        static Random rand = new Random();

        public static PlayerShip Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlayerShip();

                return instance;
            }
        }

        private PlayerShip()
        {
            image = Art.Player;
            Position = Game1.ScreenSize / 2;
            Radius = 10;
        }

        public override void Update()
        {
            if (IsDead)
            {
                framesUntilRespawn--;
                return;
            }

            // ship logic goes here
            Vector2 acceleration = new Vector2(-Input.CurrentAccelerometerValues.Y, Input.CurrentAccelerometerValues.X);
            Vector2 MoveToward;
            Vector2 aim = new Vector2();

            if (acceleration.LengthSquared() > Input.Tilt_Threshold)
            {
                MoveToward = new Vector2(MathHelper.Clamp(acceleration.X * 30, -(Math.Abs(acceleration.X)), Math.Abs(acceleration.X)),
                                            -1 * MathHelper.Clamp(acceleration.Y * 30, -(Math.Abs(acceleration.Y)), Math.Abs(acceleration.Y)));
            }
            else
            {
                MoveToward = new Vector2(0, 0);
            }
            Velocity = SPEED * MoveToward;
            Position += Velocity;
            Position = Vector2.Clamp(Position, Size / 2, Game1.ScreenSize - Size / 2);
            if (Velocity.LengthSquared() > 0)
            {
                Orientation = Velocity.ToAngle();
            }
            bool firing = false;
            foreach (TouchLocation touch in Game1.Input.TouchState)
            {
                firing = true;
                break;
            }
            if (firing && cooldownRemaining <= 0)
            {
                cooldownRemaining = cooldownFrames;
                //float aimAngle = aim.ToAngle();
                float aimAngle = Orientation;
                Quaternion aimQuat = Quaternion.CreateFromYawPitchRoll(0, 0, aimAngle);

                float randomSpread = rand.NextFloat(-0.04f, 0.04f) + rand.NextFloat(-0.04f, 0.04f);
                Vector2 vel = MathUtil.FromPolar(aimAngle + randomSpread, 11f);

                Vector2 offset = Vector2.Transform(new Vector2(25, -8), aimQuat);
                EntityManager.Add(new Bullet(Position + offset, vel));

                offset = Vector2.Transform(new Vector2(25, 8), aimQuat);
                EntityManager.Add(new Bullet(Position + offset, vel));
            }

            if (cooldownRemaining > 0)
                cooldownRemaining--;
        }
        public void Kill()
        {
            framesUntilRespawn = 60;
            EnemySpawner.Reset();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsDead)
                base.Draw(spriteBatch);
        }
    }
}
