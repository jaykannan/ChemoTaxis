using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChemoTaxis.Soup
{
    /// <summary>
    /// Standard class for floating objects in the Primordial Soup
    /// </summary>
    public class FloatingObject
    {
        public FType type;
        public float temperature;
        public float drift, rise;

        // common logging data
        public LogData log = new LogData();
        public int ID;

        public Vector2 vPosition;
        public Vector2 vCurrentForce;                          // direction of the force
        public Vector2 vMechanicalForce = new Vector2(0, 0);   // mechanical force which averages out  
        public Color color = Color.White;
        public float vRadius = 1.0f;                    // radius of the vesicle
        public float vRadiusThreshold = 3.0f;                  // maximum size of the vesicle before it breaks due to external forces
        public Random rand = new Random();
        public int currentPartition;
        

        // also a type of object
        public int objectType;                                // could be a random type of molecule that affects other monomers around it

        public virtual void LoadContent()
        {

        }

        public virtual void Update(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            // maintain a timer
            log.lifeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // calculate distance travelled bu current and mechanical force
            log.distanceTravelled += Vector2.Distance(Vector2.Zero, vCurrentForce) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            log.distanceTravelled += Vector2.Distance(Vector2.Zero, vMechanicalForce) * (float)gameTime.ElapsedGameTime.TotalSeconds;

            vPosition += vCurrentForce * (float)gameTime.ElapsedGameTime.TotalSeconds;

            vPosition += vMechanicalForce;
            vMechanicalForce *= 0.95f;      // average out any mechanical force that is caused by monomers

            if (type != FType.ABSORBED)          // if it is a polymer, it rides with the vesicle
            { 
                // temperature heats up the molecule - the closer to the bottom the hotter it gets
                // the closer to the surface the colder it gets
                temperature = Sigmoid(vPosition.Y / (float)graphicsDevice.Viewport.Height) * 100;
                drift = -((vPosition.Y / (float)graphicsDevice.Viewport.Height) - 0.5f);

                vCurrentForce.X = drift * rand.Next(Globals.DRIFTMIN, Globals.DRIFTMAX);

                if (temperature < rand.Next(Globals.TEMPERATURE_LMIN, Globals.TEMPERATURE_LMAX))
                    vCurrentForce.Y += rand.Next(Globals.RISE);
                else
                if (temperature > rand.Next(Globals.TEMPERATURE_UMIN, Globals.TEMPERATURE_UMAX))
                    vCurrentForce.Y -= rand.Next(Globals.RISE);
                else
                    vPosition.Y += vCurrentForce.Y *
                                   (float)gameTime.ElapsedGameTime.TotalSeconds * rand.Next(Globals.RISE / 2);

                // warp them so that the simulation looks consistant
                if (vPosition.X < 0)
                {
                    vPosition.X = (float)graphicsDevice.Viewport.Width;
                }
                if (vPosition.X > (float)graphicsDevice.Viewport.Width)
                {
                    vPosition.X = 0;
                }
                if (vPosition.Y < 0)
                {
                    vPosition.Y = 0;
                    vCurrentForce.Y *= -0.5f;
                }
                if (vPosition.Y > (float)graphicsDevice.Viewport.Height)
                {
                    vPosition.Y = (float)graphicsDevice.Viewport.Height;
                    vCurrentForce.Y *= -0.5f;
                }
            }

        }


        

        public float Sigmoid(double x)
        {
            return (float)(2 / (1 + Math.Exp(-2 * x)) - 1);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }
    }
}