/////////////////////////////////////////////////////////////////////////////
//  Simulation of the Miller–Urey experiment based Abiogenesis where 
//  fatty acid vesicles pick monomers in a primordial soup environment
//  and evolve individual reactive behaviors 
//
//  This simulation will also have a graphical fron-end for behavioral study
//


using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace Soup
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PrimordialSoup : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Random rand = new Random();
        SpriteFont Soupfont;

        /// <summary>
        /// PrimordialSoup specific content
        /// 
        /// </summary>
        // This is a texture we can render.
        // Set the coordinates to draw the sprite at.
        Vector2 myPosition = Vector2.Zero;
        Environment primordialSoup;


        public PrimordialSoup()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //myVesicle = Content.Load<Texture2D>("Sprites/bubble");
            Soupfont = Content.Load<SpriteFont>("Font1");
            primordialSoup = new Environment(5000, GraphicsDevice, Content, Soupfont);

            

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();


            if (keyState.IsKeyDown(Keys.Escape))
            {
                // save all logs
                //for (int i = 0; i < primordialSoup.logger.Count; i++)
                //{
                //    StreamWriter sw = new StreamWriter(@"c:\temp\alife\" + i.ToString() + ".txt", false);
                //    sw.WriteLine("Timer\tRadius\tGrowth\tDistance\tMonomers");
                //    for (int j = 0; j < primordialSoup.logger[i].logs.Count; j++)
                //        sw.WriteLine(
                //            primordialSoup.logger[i].logs[j].lifeTimer + "\t" +
                //            primordialSoup.logger[i].logs[j].radius + "\t" + 
                //            primordialSoup.logger[i].logs[j].volumeGrowth + "\t" +
                //            primordialSoup.logger[i].logs[j].distanceTravelled + "\t" +
                //            primordialSoup.logger[i].logs[j].numberofMonomersEaten);
                //    sw.Close();
                //}

                this.Exit();
            }

            primordialSoup.Update(GraphicsDevice, gameTime);


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the sprite.
            
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            primordialSoup.Draw(spriteBatch);

            foreach (FloatingObject f in primordialSoup.floatingObjects)
                if (f.type == FType.VESICLE && f.vRadius > 30.0f)            // large vesicles
                {
                    spriteBatch.DrawString(Soupfont, (f.ID + " " + f.log.lifeTimer + "\n" + f.log.distanceTravelled + "\n" +
                        f.log.volumeGrowth + "\n" + f.log.numberofMonomersEaten).ToString()
                        , f.vPosition, Color.Red, 0.0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0.0f);
                }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
