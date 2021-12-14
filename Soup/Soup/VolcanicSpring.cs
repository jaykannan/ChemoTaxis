using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace Soup
{
    
    public enum dir
    {        
        TOP = 0,
        TOPRIGHT,
        RIGHT,
        BOTTOMRIGHT,
        BOTTOM,
        BOTTOMLEFT,
        LEFT,
        TOPLEFT
    }

    // rapidly manipulated class for quick access to nearby objects
    public class SpacePartition
    {
        public int ID;          // ID and the serial of the partition block
        public Rectangle bounds;
        public List<int> objectList = new List<int>();
        public int pixelSize;

        // neighbour IDs
        public int[] neighbour = new int[8];
        
    }

    public class PartitionManager
    {
        public const int WIDTH = 10;
        public const int HEIGHT = 8;
        

        public List<SpacePartition> partitions = new List<SpacePartition>();

        public int getPartitionID(GraphicsDevice gd, float x, float y)
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
            for (int j = 0; j < HEIGHT; j++)
            {
                for (int i = 0; i < WIDTH; i++)
                {
                    SpacePartition temp = new SpacePartition();
                    temp.bounds.X = (int)(i * width);
                    temp.bounds.Y = (int)(j * height);
                    temp.bounds.Width = width;
                    temp.bounds.Height = height;
                    temp.ID = partitions.Count;

                    temp.neighbour[(int)dir.TOP] = getPartitionID(gd, temp.bounds.Center.X, temp.bounds.Y - height);
                    temp.neighbour[(int)dir.TOPRIGHT] = getPartitionID(gd, temp.bounds.Center.X + width, temp.bounds.Y - height);
                    temp.neighbour[(int)dir.RIGHT] = getPartitionID(gd, temp.bounds.Center.X + width, temp.bounds.Y);
                    temp.neighbour[(int)dir.BOTTOMRIGHT] = getPartitionID(gd, temp.bounds.Center.X + width, temp.bounds.Y + height);
                    temp.neighbour[(int)dir.BOTTOM] = getPartitionID(gd, temp.bounds.Center.X, temp.bounds.Y + height);
                    temp.neighbour[(int)dir.BOTTOMLEFT] = getPartitionID(gd, temp.bounds.Center.X - width, temp.bounds.Y + height);
                    temp.neighbour[(int)dir.LEFT] = getPartitionID(gd, temp.bounds.Center.X - width, temp.bounds.Y);
                    temp.neighbour[(int)dir.TOPLEFT] = getPartitionID(gd, temp.bounds.Center.X - width, temp.bounds.Y - height);

                    partitions.Add(temp);
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
        public List<FloatingObject> floatingObjects = new List<FloatingObject>();
        public PartitionManager partitionManager;
        SpriteFont sf;
        public float logTimer = 0.0f;
        public float gameTimer = 0.0f;

        public List<Logger> logger = new List<Logger>();
        
        Random rand = new Random();

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
            partitionManager = new PartitionManager(graphicsDevice);


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
                floatingObjects.Add(vesicle);
                logger.Add(new Logger());
            }

            // create 99% of the objects as monomers
            for (int i = 0; i < (int)((numObjects * 99) / 100); i++)
            {
                Monomer monomer = new Monomer(texMonomer, RandomEnum<CProperty>());
                monomer.vPosition = new Vector2(rand.Next(graphicsDevice.Viewport.Width),
                    rand.Next(graphicsDevice.Viewport.Height));
                monomer.type = FType.MONOMER;
                floatingObjects.Add(monomer);
            }


        }

        public void Update(GraphicsDevice graphicsDevice, GameTime gameTime)
        {

            logTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            gameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (logTimer > 1.0f)        // log data every second
            {

                for (int i = 0; i < logger.Count; i++)  // nullify update flags
                    logger[i].updated = false;

                for (int j = 0; j < floatingObjects.Count; j++)
                {
                    if (floatingObjects[j].type == FType.VESICLE)
                    {
                        floatingObjects[j].log.radius = floatingObjects[j].vRadius;
                        floatingObjects[j].log.gameTime = gameTimer;
                        logger[floatingObjects[j].ID].logs.Add(floatingObjects[j].log);
                        logger[floatingObjects[j].ID].updated = true;
                    }
                }
                logTimer -= 1.0f;
            }


            // update normally
            for (int j = 0; j < floatingObjects.Count;j++ )
            {
                FloatingObject fobj = floatingObjects[j];
                // if it is a vesicle try to gobble up nearby monomer
                if (fobj.type == FType.VESICLE && fobj.currentPartition >= 0)          // if this is a vesicle
                {
                    // check nearby objects
                    for (int i = 0; i < partitionManager.partitions[fobj.currentPartition].objectList.Count; i++)
                    {
                        FloatingObject target = floatingObjects[partitionManager.partitions[fobj.currentPartition].objectList[i]];

                        // call eatOtherVesicles to perform the eating function
                        eatOtherVesicles(gameTime, j, fobj, i, target);
                        // eat monomers only if it can grow that much
                        eatMonomers(j, fobj, i, target);
                    }

                    // model a stochastic mechanical event which causes a break in the vesicle larger than 50
                    if (((float)rand.Next(100) / 100.0f) < Globals.MECHANICAL_EVENT && fobj.vRadius > rand.Next(50,75))
                    {
                        // create new baby vesicles
                        Vesicle babyVesicle = new Vesicle(((Vesicle)fobj).vTexture);
                        ((Vesicle)babyVesicle).polymers.Add(new Polymer());
                        float fRadius = 0.0f;

                        for (int i = 0; i < ((Vesicle)fobj).polymers[0].chain.Count; i++)
                        {
                            fRadius += 1.0f;
                            // if it has split instruction
                            if (((Vesicle)fobj).polymers[0].chain[i].property != CProperty.SPLIT)
                            {
                                // move it inside the vesicle
                                ((Vesicle)fobj).polymers[0].chain[i].offset = new Vector2(rand.Next((int)fRadius) - fRadius / 2, 
                                    rand.Next((int)fRadius) - fRadius / 2);

                                // copy the monomer
                                ((Vesicle)babyVesicle).polymers[0].chain.Add(((Vesicle)fobj).polymers[0].chain[i]);
                            }
                            else
                            {
                                // copy the monomer
                                ((Vesicle)babyVesicle).polymers[0].chain.Add(((Vesicle)fobj).polymers[0].chain[i]);

                                // add vesicles to list
                                babyVesicle.vPosition = ((Vesicle)fobj).vPosition;
                                babyVesicle.vMechanicalForce = new Vector2(rand.Next(20) - 10, rand.Next(20) - 10);
                                babyVesicle.vRadius = babyVesicle.vRadiusThreshold = fRadius;
                                fRadius = 0.0f;
                                babyVesicle.type = FType.VESICLE;

                                // copy all the log values to the new creature preferably to the largest one
                                babyVesicle.log.lifeTimer = ((Vesicle)fobj).log.lifeTimer; // retain the timer only for the first baby
                                babyVesicle.log.distanceTravelled = ((Vesicle)fobj).log.distanceTravelled; // retain the dist only for the first baby
                                ((Vesicle)fobj).log.lifeTimer = 0.0f;
                                babyVesicle.log.numberofMonomersEaten = fobj.log.numberofMonomersEaten;
                                babyVesicle.log.volumeGrowth = fobj.log.volumeGrowth;
                                babyVesicle.ID = fobj.ID;

                                floatingObjects.Add(babyVesicle);
                                babyVesicle = new Vesicle(((Vesicle)fobj).vTexture);
                                ((Vesicle)babyVesicle).polymers.Add(new Polymer());
                            }
                        }

                        // destroy mother vesicle
                        ((Vesicle)fobj).type = FType.ABSORBED;
                    }
                }



                // update polymer
                floatingObjects[j].Update(graphicsDevice, gameTime);
            }

            // remove monomers already absorbed
            for (int i = floatingObjects.Count - 1; i >= 0; i--)
                if (floatingObjects[i].type == FType.ABSORBED)
                    floatingObjects.RemoveAt(i);

            // create new locations
            foreach (SpacePartition s in partitionManager.partitions)
                s.objectList.Clear();

            // add appropriate objects into partitions
            for(int i=0;i<floatingObjects.Count;i++)
            {
                floatingObjects[i].currentPartition = partitionManager.getPartitionID(graphicsDevice, 
                    floatingObjects[i].vPosition.X, floatingObjects[i].vPosition.Y);
                if (floatingObjects[i].currentPartition >= 0 && floatingObjects[i].currentPartition < partitionManager.partitions.Count)
                    partitionManager.partitions[floatingObjects[i].currentPartition].objectList.Add(i);
            }

        }

        // eat monomers
        private void eatMonomers(int j, FloatingObject vesicle, int i, FloatingObject target)
        {
            if (vesicle.vRadius < vesicle.vRadiusThreshold)          // only if the radius can grow - growth function
                if (target.type == FType.MONOMER && j != partitionManager.partitions[vesicle.currentPartition].objectList[i])
                {
                    // attach to vesicle and make it a polymer
                    if (Vector2.Distance(vesicle.vPosition, target.vPosition) < vesicle.vRadius)
                    {
                        Monomer m = (Monomer)target;
                        m.offset = new Vector2((rand.Next((int)(vesicle.vRadius * Globals.GROWTH_FACTOR)) - 
                            (int)(vesicle.vRadius * Globals.GROWTH_FACTOR / 2)),
                            rand.Next((int)(vesicle.vRadius * Globals.GROWTH_FACTOR))
                            - (int)(vesicle.vRadius * Globals.GROWTH_FACTOR / 2));

                        // add monomer to chain 0 - it should divide itself later
                        ((Vesicle)vesicle).polymers[0].chain.Add(m);
                        ((Vesicle)vesicle).vRadius += Globals.GROWTH_PER_MONOMER;
                        ((Vesicle)vesicle).log.numberofMonomersEaten++;
                        //((Vesicle)f).vRadiusThreshold = ((Vesicle)f).vRadius;

                        // schedule for deletion
                        target.type = FType.ABSORBED;


                        // pick only one object per frame per vesicle
                        return;


                    }
                }
        }

        // eat other vesciles slowly
        private void eatOtherVesicles(GameTime gameTime, int j, FloatingObject f, int i, FloatingObject target)
        {
            // eat other vesicles if encountered within radius and not itself
            if (target.type == FType.VESICLE && j != partitionManager.partitions[f.currentPartition].objectList[i])
                if (f.vRadius > target.vRadius) // bigger eats smaller
                    if (Vector2.Distance(f.vPosition, target.vPosition) < f.vRadius)     // swallow only if it is inside the vesicle
                    {
                        // grow the border allowing it to eat more stuff and delete the eaten vesicle
                        // borrow the monomers as well
                        f.vRadiusThreshold += (float)gameTime.ElapsedGameTime.TotalSeconds * Globals.ABSORB_RATE;
                        f.log.volumeGrowth += (float)gameTime.ElapsedGameTime.TotalSeconds * Globals.ABSORB_RATE;
                        f.color = Color.White;                        
                        target.vRadius -= (float)gameTime.ElapsedGameTime.TotalSeconds * Globals.ABSORB_RATE;
                        target.vRadiusThreshold = target.vRadius;
                        target.color = Color.Green;

                        // if it has become significatly smaller, borrow one (or some) monomers
                        if (target.vRadius < target.vRadiusThreshold )
                        {
                            // transfer first n monomers
                            int n = rand.Next(Globals.MONOMER_MOVE_RATE);
                            if (((Vesicle)target).polymers[0].chain.Count > n)
                            {
                                for (int count = 0; count < n; count++)
                                {
                                    // insert at end of chain
                                    ((Vesicle)f).polymers[0].chain.Add(/*rand.Next(((Vesicle)f).polymers[0].chain.Count),*/
                                        ((Vesicle)target).polymers[0].chain[0]);
                                    ((Vesicle)target).polymers[0].chain.RemoveAt(0);
                                    f.log.numberofMonomersEaten++;
                                }
                                target.vRadiusThreshold = target.vRadius;
                            }
                        }


                        // delete the object if it becomes too small to eat up
                        if (target.vRadius < 1.0f)
                            target.type = FType.ABSORBED;
                    }
        }
        

        public void Draw(SpriteBatch spriteBatch)
        {
            // for render debug text
            KeyboardState keyState = Keyboard.GetState();

            foreach (FloatingObject f in floatingObjects)
                f.Draw(spriteBatch);

            // show seperation grid
            if (keyState.IsKeyDown(Keys.D1))
                foreach (SpacePartition s in partitionManager.partitions)
                    spriteBatch.DrawString(sf, s.ID.ToString() + " " + s.objectList.Count.ToString() , 
                        new Vector2(s.bounds.Center.X, s.bounds.Center.Y) , 
                        Color.Red, 0.0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0.0f);
        }

    }
}
