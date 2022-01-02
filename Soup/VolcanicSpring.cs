using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ChemoTaxis.Soup
{
    
    public enum Dir
    {        
        TOP = 0,
        TOP_RIGHT,
        RIGHT,
        BOTTOM_RIGHT,
        BOTTOM,
        BOTTOM_LEFT,
        LEFT,
        TOP_LEFT
    }

    // rapidly manipulated class for quick access to nearby objects
    public class SpacePartition
    {
        public int ID;          // ID and the serial of the partition block
        public Rectangle Bounds;
        public List<int> ObjectList = new List<int>();

        // neighbour IDs
        public int[] Neighbour = new int[8];
        
    }

    public class PartitionManager
    {
        public const int WIDTH = 10;
        public const int HEIGHT = 8;
        

        public List<SpacePartition> Partitions = new List<SpacePartition>();

        public int GetPartitionId(GraphicsDevice gd, float x, float y)
        {
            if (x < 0 || y < 0) return -1;
            if (x >= gd.Viewport.Width || y >= gd.Viewport.Height) return -1;

            int a, b;

            a = (int)((x / gd.Viewport.Width) * (float)WIDTH);
            b = (int)((y / gd.Viewport.Height) * (float)HEIGHT);

            return (b * WIDTH) + a;
        }

        public PartitionManager(GraphicsDevice gd)
        {
            int width = gd.Viewport.Width / WIDTH;
            int height = gd.Viewport.Height / HEIGHT;
            // initialize partitions
            for (var j = 0; j < HEIGHT; j++)
            {
                for (var i = 0; i < WIDTH; i++)
                {
                    var temp = new SpacePartition();
                    temp.Bounds.X = (int)(i * width);
                    temp.Bounds.Y = (int)(j * height);
                    temp.Bounds.Width = width;
                    temp.Bounds.Height = height;
                    temp.ID = Partitions.Count;

                    temp.Neighbour[(int)Dir.TOP] = GetPartitionId(gd, temp.Bounds.Center.X, temp.Bounds.Y - height);
                    temp.Neighbour[(int)Dir.TOP_RIGHT] = GetPartitionId(gd, temp.Bounds.Center.X + width, temp.Bounds.Y - height);
                    temp.Neighbour[(int)Dir.RIGHT] = GetPartitionId(gd, temp.Bounds.Center.X + width, temp.Bounds.Y);
                    temp.Neighbour[(int)Dir.BOTTOM_RIGHT] = GetPartitionId(gd, temp.Bounds.Center.X + width, temp.Bounds.Y + height);
                    temp.Neighbour[(int)Dir.BOTTOM] = GetPartitionId(gd, temp.Bounds.Center.X, temp.Bounds.Y + height);
                    temp.Neighbour[(int)Dir.BOTTOM_LEFT] = GetPartitionId(gd, temp.Bounds.Center.X - width, temp.Bounds.Y + height);
                    temp.Neighbour[(int)Dir.LEFT] = GetPartitionId(gd, temp.Bounds.Center.X - width, temp.Bounds.Y);
                    temp.Neighbour[(int)Dir.TOP_LEFT] = GetPartitionId(gd, temp.Bounds.Center.X - width, temp.Bounds.Y - height);

                    Partitions.Add(temp);
                }
            }
        }
 
    }

    
    /// <summary>
    /// logging framework
    /// </summary>
    public class Logger
    {
        public List<LogData> logs = new List<LogData>();
        public bool updated = false;
    };


    /// <summary>
    /// This is the main container which contains all the objects
    /// Simulates the soup atmosphere
    /// </summary>
    public class Environment
    {
        // all the floating objects
        public List<FloatingObject> FloatingObjects = new();
        public PartitionManager PartitionManager;
        SpriteFont sf;
        public float logTimer = 0.0f;
        public float gameTimer = 0.0f;

        public List<Logger> logger = new();
        
        Random rand = new();

        public Texture2D texVesicle;
        public Texture2D texMonomer;

        // enum randomizer
        public T RandomEnum<T>()
        {
            T[] values = (T[])Enum.GetValues(typeof(T));
            return values[rand.Next(0, values.Length)];
        }

        public Environment(int numObjects, GraphicsDevice graphicsDevice, ContentManager Content, SpriteFont font)
        {
            texVesicle = Content.Load<Texture2D>("bubble");
            texMonomer = Content.Load<Texture2D>("monomer");
            sf = font;

            // initialize partitions 10x8 (change this if needed)
            PartitionManager = new PartitionManager(graphicsDevice);


            // create 1% of them as vesicles, create logData array to update every second
            for (int i = 0; i < (int)(numObjects / 100); i++)
            {
                Vesicle vesicle = new Vesicle(texVesicle);
                vesicle.vRadius = vesicle.vRadiusThreshold = (float)rand.Next(10, 25);
                vesicle.vCurrentForce = new Vector2(0, rand.Next(10) - 5);
                vesicle.vPosition = new Vector2(rand.Next(graphicsDevice.Viewport.Width),
                    rand.Next(graphicsDevice.Viewport.Height));
                vesicle.type = FType.VESICLE;
                vesicle.ID = i;
                FloatingObjects.Add(vesicle);
                logger.Add(new Logger());
            }

            // create 99% of the objects as monomers
            for (int i = 0; i < (int)((numObjects * 99) / 100); i++)
            {
                Monomer monomer = new Monomer(texMonomer, RandomEnum<CProperty>());
                monomer.vPosition = new Vector2(rand.Next(graphicsDevice.Viewport.Width),
                    rand.Next(graphicsDevice.Viewport.Height));
                monomer.type = FType.MONOMER;
                FloatingObjects.Add(monomer);
            }


        }

        public void Update(GraphicsDevice graphicsDevice, GameTime gameTime)
        {

            logTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            gameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (logTimer > 1.0f)        // log data every second
            {

                foreach (var log in logger)
                    log.updated = false;

                foreach (var floatingObject in FloatingObjects.Where(floatingObject => floatingObject.type == FType.VESICLE))
                {
                    floatingObject.log.radius = floatingObject.vRadius;
                    floatingObject.log.gameTime = gameTimer;
                    logger[floatingObject.ID].logs.Add(floatingObject.log);
                    logger[floatingObject.ID].updated = true;
                }
                logTimer -= 1.0f;
            }


            // update normally
            for (var j = 0; j < FloatingObjects.Count;j++ )
            {
                var floatingObject = FloatingObjects[j];
                // if it is a vesicle try to gobble up nearby monomer
                if (floatingObject.type == FType.VESICLE && floatingObject.currentPartition >= 0)          // if this is a vesicle
                {
                    // check nearby objects
                    for (var i = 0; i < PartitionManager.Partitions[floatingObject.currentPartition].ObjectList.Count; i++)
                    {
                        var target = FloatingObjects[PartitionManager.Partitions[floatingObject.currentPartition].ObjectList[i]];

                        // call eatOtherVesicles to perform the eating function
                        EatOtherVesicles(gameTime, j, floatingObject, i, target);
                        // eat monomers only if it can grow that much
                        EatMonomers(j, floatingObject, i, target);
                    }

                    // model a stochastic mechanical event which causes a break in the vesicle larger than 50
                    if (((float)rand.Next(100) / 100.0f) < Globals.MECHANICAL_EVENT && floatingObject.vRadius > rand.Next(50,75))
                    {
                        // create new baby vesicles
                        var babyVesicle = new Vesicle(((Vesicle)floatingObject).VTexture);
                        ((Vesicle)babyVesicle).Polymers.Add(new Polymer());
                        var fRadius = 0.0f;

                        for (var i = 0; i < ((Vesicle)floatingObject).Polymers[0].Chain.Count; i++)
                        {
                            fRadius += 1.0f;
                            // if it has split instruction
                            if (((Vesicle) floatingObject).Polymers[0].Chain[i].property == CProperty.SPLIT)
                            {
                                // copy the monomer
                                ((Vesicle) babyVesicle).Polymers[0].Chain
                                    .Add(((Vesicle) floatingObject).Polymers[0].Chain[i]);

                                // add vesicles to list
                                babyVesicle.vPosition = ((Vesicle) floatingObject).vPosition;
                                babyVesicle.vMechanicalForce = new Vector2(rand.Next(20) - 10, rand.Next(20) - 10);
                                babyVesicle.vRadius = babyVesicle.vRadiusThreshold = fRadius;
                                fRadius = 0.0f;
                                babyVesicle.type = FType.VESICLE;

                                // copy all the log values to the new creature preferably to the largest one
                                babyVesicle.log.lifeTimer =
                                    ((Vesicle) floatingObject).log
                                    .lifeTimer; // retain the timer only for the first baby
                                babyVesicle.log.distanceTravelled =
                                    ((Vesicle) floatingObject).log
                                    .distanceTravelled; // retain the dist only for the first baby
                                ((Vesicle) floatingObject).log.lifeTimer = 0.0f;
                                babyVesicle.log.numberofMonomersEaten = floatingObject.log.numberofMonomersEaten;
                                babyVesicle.log.volumeGrowth = floatingObject.log.volumeGrowth;
                                babyVesicle.ID = floatingObject.ID;

                                FloatingObjects.Add(babyVesicle);
                                babyVesicle = new Vesicle(((Vesicle) floatingObject).VTexture);
                                ((Vesicle) babyVesicle).Polymers.Add(new Polymer());
                            }
                            else
                            {
                                // move it inside the vesicle
                                ((Vesicle) floatingObject).Polymers[0].Chain[i].offset = new Vector2(
                                    rand.Next((int) fRadius) - fRadius / 2,
                                    rand.Next((int) fRadius) - fRadius / 2);

                                // copy the monomer
                                ((Vesicle) babyVesicle).Polymers[0].Chain
                                    .Add(((Vesicle) floatingObject).Polymers[0].Chain[i]);
                            }
                        }

                        // destroy mother vesicle
                        ((Vesicle)floatingObject).type = FType.ABSORBED;
                    }
                }



                // update polymer
                FloatingObjects[j].Update(graphicsDevice, gameTime);
            }

            // remove monomers already absorbed
            for (int i = FloatingObjects.Count - 1; i >= 0; i--)
                if (FloatingObjects[i].type == FType.ABSORBED)
                    FloatingObjects.RemoveAt(i);

            // create new locations
            foreach (var s in PartitionManager.Partitions)
                s.ObjectList.Clear();

            // add appropriate objects into partitions
            for(var i=0;i<FloatingObjects.Count;i++)
            {
                FloatingObjects[i].currentPartition = PartitionManager.GetPartitionId(graphicsDevice, 
                    FloatingObjects[i].vPosition.X, FloatingObjects[i].vPosition.Y);
                if (FloatingObjects[i].currentPartition >= 0 && FloatingObjects[i].currentPartition < PartitionManager.Partitions.Count)
                    PartitionManager.Partitions[FloatingObjects[i].currentPartition].ObjectList.Add(i);
            }

        }

        // eat monomers
        private void EatMonomers(int j, FloatingObject vesicle, int i, FloatingObject target)
        {
            if (!(vesicle.vRadius < vesicle.vRadiusThreshold)) return;
            if (target.type != FType.MONOMER ||
                j == PartitionManager.Partitions[vesicle.currentPartition].ObjectList[i]) return;
            // attach to vesicle and make it a polymer
            if (!(Vector2.Distance(vesicle.vPosition, target.vPosition) < vesicle.vRadius)) return;
            var m = (Monomer)target;
            m.offset = new Vector2((rand.Next((int)(vesicle.vRadius * Globals.GROWTH_FACTOR)) - 
                                    (int)(vesicle.vRadius * Globals.GROWTH_FACTOR / 2)),
                rand.Next((int)(vesicle.vRadius * Globals.GROWTH_FACTOR))
                - (int)(vesicle.vRadius * Globals.GROWTH_FACTOR / 2));

            // add monomer to chain 0 - it should divide itself later
            ((Vesicle)vesicle).Polymers[0].Chain.Add(m);
            ((Vesicle)vesicle).vRadius += Globals.GROWTH_PER_MONOMER;
            ((Vesicle)vesicle).log.numberofMonomersEaten++;
            //((Vesicle)f).vRadiusThreshold = ((Vesicle)f).vRadius;

            // schedule for deletion
            target.type = FType.ABSORBED;


            // pick only one object per frame per vesicle
            return;
        }

        // eat other vesicles slowly
        private void EatOtherVesicles(GameTime gameTime, int j, FloatingObject f, int i, FloatingObject target)
        {
            // eat other vesicles if encountered within radius and not itself
            if (target.type != FType.VESICLE ||
                j == PartitionManager.Partitions[f.currentPartition].ObjectList[i]) return;
            if (!(f.vRadius > target.vRadius)) return;
            if (!(Vector2.Distance(f.vPosition, target.vPosition) < f.vRadius)) return;
            // grow the border allowing it to eat more stuff and delete the eaten vesicle
            // borrow the monomers as well
            f.vRadiusThreshold += (float)gameTime.ElapsedGameTime.TotalSeconds * Globals.ABSORB_RATE;
            f.log.volumeGrowth += (float)gameTime.ElapsedGameTime.TotalSeconds * Globals.ABSORB_RATE;
            f.color = Color.White;                        
            target.vRadius -= (float)gameTime.ElapsedGameTime.TotalSeconds * Globals.ABSORB_RATE;
            target.vRadiusThreshold = target.vRadius;
            target.color = Color.Green;

            // if it has become significantly smaller, borrow one (or some) monomers
            if (target.vRadius < target.vRadiusThreshold )
            {
                // transfer first n monomers
                var n = rand.Next(Globals.MONOMER_MOVE_RATE);
                if (((Vesicle)target).Polymers[0].Chain.Count > n)
                {
                    for (var count = 0; count < n; count++)
                    {
                        // insert at end of chain
                        ((Vesicle)f).Polymers[0].Chain.Add(/*rand.Next(((Vesicle)f).polymers[0].chain.Count),*/
                            ((Vesicle)target).Polymers[0].Chain[0]);
                        ((Vesicle)target).Polymers[0].Chain.RemoveAt(0);
                        f.log.numberofMonomersEaten++;
                    }
                    target.vRadiusThreshold = target.vRadius;
                }
            }


            // delete the object if it becomes too small to eat up
            if (target.vRadius < 1.0f)
                target.type = FType.ABSORBED;
        }
        

        public void Draw(SpriteBatch spriteBatch)
        {
            // for render debug text
            var keyState = Keyboard.GetState();

            foreach (var f in FloatingObjects)
                f.Draw(spriteBatch);

            // show seperation grid
            if (!keyState.IsKeyDown(Keys.D1)) return;
            foreach (var s in PartitionManager.Partitions)
                spriteBatch.DrawString(sf, $"{s.ID} {s.ObjectList.Count}", 
                    new Vector2(s.Bounds.Center.X, s.Bounds.Center.Y) , 
                    Color.Red, 0.0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0.0f);
        }

    }
}
