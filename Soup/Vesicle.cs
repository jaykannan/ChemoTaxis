


using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChemoTaxis.Soup
{
    /// <summary>
    /// This is the instruction code for the chemical monomer strands which
    /// combine with each other
    /// 
    /// an ideal simulation will simulate most of the important reactions listed 
    /// in this link relevant to primordial soup monomers
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


    public class Polymer : FloatingObject
    {
        public List<Monomer> Chain = new List<Monomer>();

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
        public Texture2D VTexture;
        public List<Polymer> Polymers = new List<Polymer>();

        public Vesicle(Texture2D texture)
        {
            VTexture = texture;

            // add first polymer chain
            var temp = new Polymer();
            Polymers.Add(temp);

        }

        public override void Update(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            foreach (var p in Polymers)
                foreach (var m in p.Chain)
                    m.vPosition = vPosition + (m.offset * 0.5f);

            base.Update(graphicsDevice, gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(VTexture, vPosition, null, color, 0.0f, new Vector2(VTexture.Bounds.Center.X,
                VTexture.Bounds.Center.Y), vRadius / 64.0f, SpriteEffects.None, 1.0f);

            foreach (var p in Polymers)
                foreach (var m in p.Chain)
                    m.Draw(spriteBatch);


            base.Draw(spriteBatch);
        }

    }

}
