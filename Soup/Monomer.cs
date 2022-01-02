using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChemoTaxis.Soup
{
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
}