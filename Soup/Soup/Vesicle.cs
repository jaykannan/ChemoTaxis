/// Wikipedia Definition
/// 
/// 
/// The aim of this project is to analyze and simulate the formation / evolution of Protobionts from Organic Vesicles
/// given the necessary conditions with as little intelligence programming (cheating) as possible.
/// 
/// There is no fitness function and the fitter ones just eat the less fit, vesicles with radii below 5 units will 
/// be allowed to die.
/// 
/// Simulation inspired by ALife experiments referring to Alexander Oparin's The Origin of Life
/// 
/// 
/// the scope of this project is to stay in the artificial domain, to help with balancing and problemsolving
/// rather than simulate life.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Soup
{
    /// <summary>
    /// This is the instruction code for the chemical monomer strands which
    /// combine with each other
    /// 
    /// an ideal simulation will simulate most of the important reactions listed 
    /// in this link relevant to primoridal soup monomers
    /// http://en.wikipedia.org/wiki/List_of_organic_reactions
    /// </summary>
    public enum CProperty
    {
        NONE = 0,               //  No Behavior
        ATTACH,                 //  Attaches Monomer of type immediately below position
        SUBTRACT,                //  Subtracts all monomers of type given from the polymer (including itself?)
        ATTRACT,                //  moves closer to the Monomers of type given
        REPEL,                  //  moves away from the Monomers of type given 
        SPLIT,                  //  Splits the polymer strand at point into multiple strands upon discovery of type given
        COMBINE,                //  Combine the polymer strand to the same chain
        COPY,                   //  Copies itself and neighbours onto type given (bit dodgy, needs to be more realistic) 
        INCREASE_PH,            //  Dissolve over time and increase pH of the surrounding
        DECREASE_PH             //  Dissolve over time and decrease pH of the surrounding
    };

    /// <summary>
    /// This is the type of floating object
    /// </summary>
    public enum FType
    { 
        MONOMER = 0,
        ABSORBED,
        VESICLE
    }

    public class Globals
    {
        public static int TEMPERATURE_LMIN = 5;
        public static int TEMPERATURE_LMAX = 25;
        public static int TEMPERATURE_UMIN = 65;
        public static int TEMPERATURE_UMAX = 75;
        public static int DRIFTMIN = 200;
        public static int DRIFTMAX = 350;
        public static int RISE = 5;
        public static float GROWTH_PER_MONOMER = 0.25f;
        public static float GROWTH_FACTOR = 1.5f;
        public static float ABSORB_RATE = 2.5f;
        public static float MECHANICAL_EVENT = 0.2f;
        public static int MONOMER_MOVE_RATE = 5;
    }


    public class LogData
    {
        // logging variables
        public float gameTime = 0.0f;
        public float radius = 0.0f;
        public float lifeTimer = 0.0f;
        public float distanceTravelled = 0.0f;
        public float volumeGrowth = 0.0f;
        public int numberofMonomersEaten = 0;
    }



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

    /// <summary>
    /// Monomers are free floating chemicals wth spoecial properties
    /// they will get absorbed by organic vesicles and will be mised together inside
    /// the properties above are assumed properties that will be exhibited
    /// when each monomer is paired with another absorbed monomer.
    /// 
    /// they have specific functionality like Lego components and the program will 
    /// randomly assemble functioning Protobionts. Vesicles are created by lightning strikes
    /// 
    /// this will not be a tree structure as the properties will copy strands as how 
    /// it would be processed. 
    /// </summary>
    public class Monomer : FloatingObject 
    {
        public Texture2D vTexture;
        public CProperty property;
        public int affectsType;
        public Vector2 offset;

        public Monomer(Texture2D texture, CProperty p)
        {
            vTexture = texture;
            property = p;
            vCurrentForce = new Vector2(0, rand.Next(10) - 5);

            // 10 different types of monomers for now
            objectType = rand.Next() % 10;
            affectsType = rand.Next() % 10;

            // pick a color based on 
            switch (p)
            {
                case CProperty.ATTACH:  { color = Color.Blue;break;}
                case CProperty.ATTRACT: { color = Color.HotPink; break; }
                case CProperty.COMBINE: { color = Color.Purple; break; }
                case CProperty.COPY: { color = Color.Red; break; }
                case CProperty.DECREASE_PH: { color = Color.Yellow; break; }
                case CProperty.INCREASE_PH: { color = Color.Orange; break; }
                case CProperty.REPEL: { color = Color.Olive; break; }
                case CProperty.SPLIT: { color = Color.Brown; break; }
                case CProperty.SUBTRACT: { color = Color.Black; break; }
            };
        }

        public override void Update(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            base.Update(graphicsDevice, gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(vTexture, vPosition, null, color, 0.0f, new Vector2(vTexture.Bounds.Center.X, 
                vTexture.Bounds.Center.Y), vRadius / 4.0f, SpriteEffects.None, 1.0f);

            base.Draw(spriteBatch);
        }
    }

    public class Polymer : FloatingObject
    {
        public List<Monomer> chain = new List<Monomer>();

    }

    /// <summary>
    /// Dumb Lipid Vesicle coacervates that can absorb monomers randomly upon contact and mimic combined behavior
    /// 
    /// Higher Temperature causes it to rise and lower temperature causes it to fall
    /// High pH causus it to increase absorbtion and lower pH causes it to reduce it
    /// High mechanical forces causes it to break into smaller vesicles 
    /// low radius' vesicles will get absorbed by bigger ones upon contact
    /// Monomers will be absorbed on contact
    ///     
    /// After the limit is reached the new random vesicle will replace another random vesicle
    /// due to computational limit which can be considered "special conditions"
    /// 
    /// </summary>
    public class Vesicle: FloatingObject
    {        
        public Texture2D vTexture;
        public List<Polymer> polymers = new List<Polymer>();         // should really be a list

        public Vesicle(Texture2D texture)
        {
            vTexture = texture;

            // add first polymer chain
            Polymer temp = new Polymer();
            polymers.Add(temp);

        }

        public override void Update(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            foreach (Polymer p in polymers)
                foreach (Monomer m in p.chain)
                    m.vPosition = vPosition + (m.offset * 0.5f);

            base.Update(graphicsDevice, gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(vTexture, vPosition, null, color, 0.0f, new Vector2(vTexture.Bounds.Center.X,
                vTexture.Bounds.Center.Y), vRadius / 64.0f, SpriteEffects.None, 1.0f);

            foreach (Polymer p in polymers)
                foreach (Monomer m in p.chain)
                    m.Draw(spriteBatch);


            base.Draw(spriteBatch);
        }

    }

}
