using Soup;
using System;

namespace ChemoTaxis
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new PrimordialSoup())
                game.Run();
        }
    }
}
