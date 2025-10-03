namespace MathGames.Games;

public static class GameOfLifePatterns
{
    public class Pattern
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool[,] Cells { get; set; } = new bool[0, 0];
        public int Width => Cells.GetLength(0);
        public int Height => Cells.GetLength(1);
    }

    public static List<Pattern> GetAllPatterns()
    {
        return new List<Pattern>
        {
            // Still Lifes
            CreateBlock(),
            CreateBeehive(),
            CreateLoaf(),
            CreateBoat(),
            CreateTub(),
            
            // Oscillators
            CreateBlinker(),
            CreateToad(),
            CreateBeacon(),
            CreatePulsar(),
            CreatePentadecathlon(),
            
            // Spaceships
            CreateGlider(),
            CreateLightweightSpaceship(),
            CreateMiddleweightSpaceship(),
            CreateHeavyweightSpaceship(),
            
            // Methuselahs
            CreateRPentomino(),
            CreateDiehard(),
            CreateAcorn(),
            
            // Guns
            CreateGosperGliderGun()
        };
    }

    public static List<string> GetCategories()
    {
        return new List<string> { "Still Lifes", "Oscillators", "Spaceships", "Methuselahs", "Guns" };
    }

    public static List<Pattern> GetPatternsByCategory(string category)
    {
        return GetAllPatterns().Where(p => p.Category == category).ToList();
    }

    #region Still Lifes

    private static Pattern CreateBlock()
    {
        return new Pattern
        {
            Name = "Block",
            Description = "The simplest still life pattern - 2x2 square",
            Category = "Still Lifes",
            Cells = new bool[,]
            {
                { true, true },
                { true, true }
            }
        };
    }

    private static Pattern CreateBeehive()
    {
        return new Pattern
        {
            Name = "Beehive",
            Description = "A common still life pattern",
            Category = "Still Lifes",
            Cells = new bool[,]
            {
                { false, true, true, false },
                { true, false, false, true },
                { false, true, true, false }
            }
        };
    }

    private static Pattern CreateLoaf()
    {
        return new Pattern
        {
            Name = "Loaf",
            Description = "A still life resembling a loaf of bread",
            Category = "Still Lifes",
            Cells = new bool[,]
            {
                { false, true, true, false },
                { true, false, false, true },
                { false, true, false, true },
                { false, false, true, false }
            }
        };
    }

    private static Pattern CreateBoat()
    {
        return new Pattern
        {
            Name = "Boat",
            Description = "A small still life pattern",
            Category = "Still Lifes",
            Cells = new bool[,]
            {
                { true, true, false },
                { true, false, true },
                { false, true, false }
            }
        };
    }

    private static Pattern CreateTub()
    {
        return new Pattern
        {
            Name = "Tub",
            Description = "A simple 5-cell still life",
            Category = "Still Lifes",
            Cells = new bool[,]
            {
                { false, true, false },
                { true, false, true },
                { false, true, false }
            }
        };
    }

    #endregion

    #region Oscillators

    private static Pattern CreateBlinker()
    {
        return new Pattern
        {
            Name = "Blinker",
            Description = "The simplest oscillator with period 2",
            Category = "Oscillators",
            Cells = new bool[,]
            {
                { true, true, true }
            }
        };
    }

    private static Pattern CreateToad()
    {
        return new Pattern
        {
            Name = "Toad",
            Description = "A period-2 oscillator",
            Category = "Oscillators",
            Cells = new bool[,]
            {
                { false, true, true, true },
                { true, true, true, false }
            }
        };
    }

    private static Pattern CreateBeacon()
    {
        return new Pattern
        {
            Name = "Beacon",
            Description = "A period-2 oscillator",
            Category = "Oscillators",
            Cells = new bool[,]
            {
                { true, true, false, false },
                { true, true, false, false },
                { false, false, true, true },
                { false, false, true, true }
            }
        };
    }

    private static Pattern CreatePulsar()
    {
        return new Pattern
        {
            Name = "Pulsar",
            Description = "A large period-3 oscillator",
            Category = "Oscillators",
            Cells = new bool[,]
            {
                { false, false, true, true, true, false, false, false, true, true, true, false, false },
                { false, false, false, false, false, false, false, false, false, false, false, false, false },
                { true, false, false, false, false, true, false, true, false, false, false, false, true },
                { true, false, false, false, false, true, false, true, false, false, false, false, true },
                { true, false, false, false, false, true, false, true, false, false, false, false, true },
                { false, false, true, true, true, false, false, false, true, true, true, false, false },
                { false, false, false, false, false, false, false, false, false, false, false, false, false },
                { false, false, true, true, true, false, false, false, true, true, true, false, false },
                { true, false, false, false, false, true, false, true, false, false, false, false, true },
                { true, false, false, false, false, true, false, true, false, false, false, false, true },
                { true, false, false, false, false, true, false, true, false, false, false, false, true },
                { false, false, false, false, false, false, false, false, false, false, false, false, false },
                { false, false, true, true, true, false, false, false, true, true, true, false, false }
            }
        };
    }

    private static Pattern CreatePentadecathlon()
    {
        return new Pattern
        {
            Name = "Pentadecathlon",
            Description = "A period-15 oscillator",
            Category = "Oscillators",
            Cells = new bool[,]
            {
                { false, false, true, false, false, false, false, true, false, false },
                { true, true, false, true, true, true, true, false, true, true },
                { false, false, true, false, false, false, false, true, false, false }
            }
        };
    }

    #endregion

    #region Spaceships

    private static Pattern CreateGlider()
    {
        return new Pattern
        {
            Name = "Glider",
            Description = "The smallest spaceship, moves diagonally",
            Category = "Spaceships",
            Cells = new bool[,]
            {
                { false, true, false },
                { false, false, true },
                { true, true, true }
            }
        };
    }

    private static Pattern CreateLightweightSpaceship()
    {
        return new Pattern
        {
            Name = "Lightweight Spaceship (LWSS)",
            Description = "A spaceship that moves horizontally",
            Category = "Spaceships",
            Cells = new bool[,]
            {
                { false, true, false, false, true },
                { true, false, false, false, false },
                { true, false, false, false, true },
                { true, true, true, true, false }
            }
        };
    }

    private static Pattern CreateMiddleweightSpaceship()
    {
        return new Pattern
        {
            Name = "Middleweight Spaceship (MWSS)",
            Description = "A medium-sized spaceship",
            Category = "Spaceships",
            Cells = new bool[,]
            {
                { false, false, true, false, false, false },
                { false, true, false, false, false, true },
                { true, false, false, false, false, false },
                { true, false, false, false, false, true },
                { true, true, true, true, true, false }
            }
        };
    }

    private static Pattern CreateHeavyweightSpaceship()
    {
        return new Pattern
        {
            Name = "Heavyweight Spaceship (HWSS)",
            Description = "A large spaceship",
            Category = "Spaceships",
            Cells = new bool[,]
            {
                { false, false, true, true, false, false, false },
                { false, true, false, false, false, false, true },
                { true, false, false, false, false, false, false },
                { true, false, false, false, false, false, true },
                { true, true, true, true, true, true, false }
            }
        };
    }

    #endregion

    #region Methuselahs

    private static Pattern CreateRPentomino()
    {
        return new Pattern
        {
            Name = "R-pentomino",
            Description = "A methuselah that stabilizes after 1103 generations",
            Category = "Methuselahs",
            Cells = new bool[,]
            {
                { false, true, true },
                { true, true, false },
                { false, true, false }
            }
        };
    }

    private static Pattern CreateDiehard()
    {
        return new Pattern
        {
            Name = "Diehard",
            Description = "Vanishes after 130 generations",
            Category = "Methuselahs",
            Cells = new bool[,]
            {
                { false, false, false, false, false, false, true, false },
                { true, true, false, false, false, false, false, false },
                { false, true, false, false, false, true, true, true }
            }
        };
    }

    private static Pattern CreateAcorn()
    {
        return new Pattern
        {
            Name = "Acorn",
            Description = "Stabilizes after 5206 generations",
            Category = "Methuselahs",
            Cells = new bool[,]
            {
                { false, true, false, false, false, false, false },
                { false, false, false, true, false, false, false },
                { true, true, false, false, true, true, true }
            }
        };
    }

    #endregion

    #region Guns

    private static Pattern CreateGosperGliderGun()
    {
        return new Pattern
        {
            Name = "Gosper Glider Gun",
            Description = "The first discovered gun - produces gliders infinitely",
            Category = "Guns",
            Cells = new bool[,]
            {
                { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, true, false, false, false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false, false, false, false, true, true, false, false, false, false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, false, false, true, true },
                { false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, true, false, false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, false, false, true, true },
                { true, true, false, false, false, false, false, false, false, false, true, false, false, false, false, false, true, false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
                { true, true, false, false, false, false, false, false, false, false, true, false, false, false, true, false, true, true, false, false, false, false, true, false, true, false, false, false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, true, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false, false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false }
            }
        };
    }

    #endregion
}
