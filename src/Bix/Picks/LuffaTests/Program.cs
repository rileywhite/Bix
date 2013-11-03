using NUnit.ConsoleRunner;
using NUnit.Framework;
using System;
using System.Reflection;

namespace Bix.Picks.LuffaTests
{
    [TestFixture]
    class Program
    {
        static void Main(string[] args)
        {
            Runner.Main(new string[] { Assembly.GetExecutingAssembly().Location });
        }
    }
}
