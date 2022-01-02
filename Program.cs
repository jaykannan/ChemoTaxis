// Wikipedia Definition
//
//
// The aim of this project is to analyze and simulate the formation / evolution of Protobionts from Organic Vesicles
// given the necessary conditions with as little intelligence programming (cheating) as possible.
//
// There is no fitness function and the fitter ones just eat the less fit, vesicles with radii below 5 units will 
// be allowed to die.
//
// Simulation inspired by ALife experiments referring to Alexander Oparin's The Origin of Life
//
//
// the scope of this project is to stay in the artificial domain, to help with balancing and problemsolving
// rather than simulate life.

using System;
using ChemoTaxis.Soup;

namespace ChemoTaxis
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new PrimordialSoup();
            game.Run();
        }
    }
}
