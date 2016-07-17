using System;

namespace Soup
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PrimordialSoup game = new PrimordialSoup())
            {
                game.Run();
            }
        }
    }
#endif
}

