using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace GameDynamics
{
    class Sprite
    {
        public Texture2D Image { get; set; }

        protected Vector2 position;
        public Vector2 Position
        {
          get { return position; }
          set { position = value; }
        }

        protected Vector2 initialVelocity;
        public Vector2 InitialVelocity
        {
            get { return initialVelocity; }
            set { initialVelocity = value; }
        }

        protected Vector2 velocity;
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        protected Vector2 acceleration;
        public Vector2 Acceleration
        {
            get { return acceleration; }
            set { acceleration = value; }
        }

        protected Vector2 force;
        public Vector2 Force
        {
            get { return force; }
            set { force = value; }
        }

        protected float mass;
        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        public float Rotation { get; set; }

        protected Vector2 center;
        public Vector2 Center
        {
            get { return center; }
            set { center = value; }
        }

        protected float scale;

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public bool Alive { get; set; }
        public bool HasBeenHit { get; set; }
        public bool Ditto { get; set; }

        public BoundingSphere bounds;

        public int orientation { get; set; } // -1 left, 1 right

        public Sprite(Texture2D loadedTexture)
        {
            Rotation = 0.0f;
            position = Vector2.Zero;
            Image = loadedTexture;
            center = new Vector2(Image.Width / 2, Image.Height / 2);
            velocity = Vector2.Zero;
            acceleration = Vector2.Zero;
            mass = 1.0f;
            Alive = false;
            HasBeenHit = false;
            scale = 1.0f;
            orientation = 1;
        }

        public Sprite(Texture2D loadedTexture, Vector2 pos)
        {
            Rotation = 0.0f;
            position = pos;
            Image = loadedTexture;
            center = new Vector2(Image.Width / 2, Image.Height / 2);
            velocity = Vector2.Zero;
            acceleration = Vector2.Zero;
            mass = 1.0f;
            Alive = true;
            HasBeenHit = false;
            scale = 1.0f;
            orientation = 1;

            bounds = new BoundingSphere(new Vector3(pos.X + center.X, pos.Y + center.Y, 0f), Image.Height / 2);
        }

        public void resolveStaticCollision(Sprite enemy, float restitution)
        {
            // get the normal vector
            Vector2 unitNormal = position - enemy.Position;

            // normalizes the normal
            unitNormal.Normalize();
            // calculate dot product of the normal
            float dotProduct = Vector2.Dot(initialVelocity, unitNormal);

            float magA = (float)Math.Sqrt(Math.Pow(initialVelocity.X, 2) + Math.Pow(initialVelocity.Y, 2));
            float magB = (float)Math.Sqrt(Math.Pow(unitNormal.X, 2) + Math.Pow(unitNormal.Y, 2));
            
            float impactAngle = (float)Math.Acos(dotProduct / (magA + magB));

            float angle = impactAngle * 180.0f / MathHelper.Pi;

            if (impactAngle > MathHelper.PiOver2 && impactAngle < MathHelper.Pi)
            {
                //multiplies the normalized normal by the dot product
                Vector2 viNormal = dotProduct * unitNormal;

                //makes the velocity = vi - 2viN
                velocity = initialVelocity - (2 * viNormal) * restitution;
            }
            else
            {
                Console.WriteLine(angle);
            }

            if (unitNormal.X < 0)
            {
                orientation = -1;
                enemy.orientation = -1;
            }
            else if (unitNormal.X > 0)
            {
                orientation = 1;
                enemy.orientation = 1;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Image, position, null, Color.White, Rotation, center, scale, SpriteEffects.None, 1f);
        }
    }
}
