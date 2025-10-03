using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MathGames.Games
{
    public class FoxAndRabbitBase : IDisposable
    {
        private void NotifyStateChanged() => OnChangeAsync?.Invoke();
        public bool Paused { get; set; }
        private readonly int _sizeX;
        private readonly int _sizeY;
        private int _refreshRate = 500;
        private int _steps;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _disposed = false;
        
        // Cell states: 0 = empty, 1 = grass, 2 = rabbit, 3 = fox
        private int[,] _grid;
        
        // Configuration parameters
        private readonly Random _random = new Random();
        private double _grassGrowthRate = 0.05;
        private double _rabbitBreedRate = 0.08;
        private double _foxBreedRate = 0.05;
        private int _rabbitFeedEnergy = 5;
        private int _foxFeedEnergy = 8;
        private int _rabbitMaxEnergy = 15;
        private int _foxMaxEnergy = 20;
        
        // Animal tracking
        private List<Animal> _rabbits = new List<Animal>();
        private List<Animal> _foxes = new List<Animal>();
        
        // Statistics history for charting
        private const int MaxHistorySize = 100;
        public List<int> GrassHistory { get; private set; } = new List<int>();
        public List<int> RabbitHistory { get; private set; } = new List<int>();
        public List<int> FoxHistory { get; private set; } = new List<int>();

        public event Func<Task>? OnChangeAsync;
        public int SizeX => _sizeX;
        public int SizeY => _sizeY;
        public int Steps => _steps;
        
        // Statistics properties
        public int GrassCount { get; private set; }
        public int RabbitCount => _rabbits.Count;
        public int FoxCount => _foxes.Count;

        public class Animal
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Energy { get; set; }
            public bool HasMoved { get; set; }

            public Animal(int x, int y, int energy)
            {
                X = x;
                Y = y;
                Energy = energy;
                HasMoved = false;
            }
        }

        public FoxAndRabbitBase(int sizeX, int sizeY)
        {
            _sizeX = sizeX;
            _sizeY = sizeY;
            _grid = new int[sizeX, sizeY];
            Paused = true;
            
            InitializeGrid();

            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (!Paused)
                    {
                        Step();
                        NotifyStateChanged();
                    }
                    try
                    {
                        await Task.Delay(_refreshRate, _cancellationTokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        private void InitializeGrid()
        {
            // Start with an empty grid
            _grid = new int[_sizeX, _sizeY];
            _rabbits.Clear();
            _foxes.Clear();
            
            // Place grass randomly
            for (int x = 0; x < _sizeX; x++)
            {
                for (int y = 0; y < _sizeY; y++)
                {
                    if (_random.NextDouble() < 0.3)
                    {
                        _grid[x, y] = 1; // Grass
                        GrassCount++;
                    }
                }
            }
            
            // Place rabbits randomly
            int rabbitCount = (_sizeX * _sizeY) / 40; // About 2.5% rabbits
            for (int i = 0; i < rabbitCount; i++)
            {
                int x = _random.Next(_sizeX);
                int y = _random.Next(_sizeY);
                if (_grid[x, y] == 0)
                {
                    _grid[x, y] = 2; // Rabbit
                    _rabbits.Add(new Animal(x, y, _rabbitMaxEnergy / 2));
                }
            }
            
            // Place foxes randomly
            int foxCount = (_sizeX * _sizeY) / 100; // About 1% foxes
            for (int i = 0; i < foxCount; i++)
            {
                int x = _random.Next(_sizeX);
                int y = _random.Next(_sizeY);
                if (_grid[x, y] == 0)
                {
                    _grid[x, y] = 3; // Fox
                    _foxes.Add(new Animal(x, y, _foxMaxEnergy / 2));
                }
            }
        }

        public void Play()
        {
            Paused = false;
            NotifyStateChanged();
        }

        public void Pause()
        {
            Paused = true;
            NotifyStateChanged();
        }

        public void Next()
        {
            Step();
            NotifyStateChanged();
        }

        public void Reset()
        {
            Paused = true;
            _steps = 0;
            GrassCount = 0;
            InitializeGrid();
            NotifyStateChanged();
        }

        public void SetRefreshRate(int rate)
        {
            _refreshRate = rate;
        }

        public int GetCell(int x, int y)
        {
            return _grid[x, y];
        }
        
        private void Step()
        {
            _steps++;
            
            // Grow grass
            GrowGrass();
            
            // Reset movement flags
            foreach (var rabbit in _rabbits)
            {
                rabbit.HasMoved = false;
            }
            
            foreach (var fox in _foxes)
            {
                fox.HasMoved = false;
            }
            
            // Move and process rabbits
            MoveRabbits();
            
            // Move and process foxes
            MoveFoxes();
            
            // Update statistics history
            UpdateStatisticsHistory();
            
            // If all animals die, pause the simulation
            if (_rabbits.Count == 0 && _foxes.Count == 0)
            {
                Paused = true;
            }
        }
        
        private void UpdateStatisticsHistory()
        {
            GrassHistory.Add(GrassCount);
            RabbitHistory.Add(_rabbits.Count);
            FoxHistory.Add(_foxes.Count);
            
            // Keep history size limited
            if (GrassHistory.Count > MaxHistorySize)
            {
                GrassHistory.RemoveAt(0);
                RabbitHistory.RemoveAt(0);
                FoxHistory.RemoveAt(0);
            }
        }
        
        private void GrowGrass()
        {
            for (int x = 0; x < _sizeX; x++)
            {
                for (int y = 0; y < _sizeY; y++)
                {
                    if (_grid[x, y] == 0 && _random.NextDouble() < _grassGrowthRate)
                    {
                        _grid[x, y] = 1; // Grass
                        GrassCount++;
                    }
                }
            }
        }
        
        private void MoveRabbits()
        {
            List<Animal> newRabbits = new List<Animal>();
            
            for (int i = _rabbits.Count - 1; i >= 0; i--)
            {
                Animal rabbit = _rabbits[i];
                
                if (rabbit.HasMoved)
                    continue;
                
                // Decrease energy
                rabbit.Energy--;
                
                // Check if rabbit dies from starvation
                if (rabbit.Energy <= 0)
                {
                    _grid[rabbit.X, rabbit.Y] = 0; // Empty the cell
                    _rabbits.RemoveAt(i);
                    continue;
                }
                
                // Find possible moves
                List<(int, int)> possibleMoves = new List<(int, int)>();
                List<(int, int)> grassCells = new List<(int, int)>();
                
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        
                        int nx = (rabbit.X + dx + _sizeX) % _sizeX;
                        int ny = (rabbit.Y + dy + _sizeY) % _sizeY;
                        
                        if (_grid[nx, ny] == 0) // Empty cell
                        {
                            possibleMoves.Add((nx, ny));
                        }
                        else if (_grid[nx, ny] == 1) // Grass
                        {
                            grassCells.Add((nx, ny));
                        }
                    }
                }
                
                int newX = rabbit.X;
                int newY = rabbit.Y;
                bool moved = false;
                
                // Prefer eating grass if available
                if (grassCells.Count > 0)
                {
                    int index = _random.Next(grassCells.Count);
                    (newX, newY) = grassCells[index];
                    rabbit.Energy += _rabbitFeedEnergy;
                    if (rabbit.Energy > _rabbitMaxEnergy)
                        rabbit.Energy = _rabbitMaxEnergy;
                    
                    GrassCount--;
                    moved = true;
                }
                // Otherwise move to an empty cell if possible
                else if (possibleMoves.Count > 0)
                {
                    int index = _random.Next(possibleMoves.Count);
                    (newX, newY) = possibleMoves[index];
                    moved = true;
                }
                
                if (moved)
                {
                    // Update grid
                    _grid[rabbit.X, rabbit.Y] = 0; // Empty old cell
                    _grid[newX, newY] = 2; // Rabbit in new cell
                    
                    // Update rabbit position
                    rabbit.X = newX;
                    rabbit.Y = newY;
                    rabbit.HasMoved = true;
                    
                    // Chance to breed
                    if (_random.NextDouble() < _rabbitBreedRate && rabbit.Energy > _rabbitMaxEnergy / 2 && possibleMoves.Count > 0)
                    {
                        int index = _random.Next(possibleMoves.Count);
                        (int bx, int by) = possibleMoves[index];
                        
                        // Create new rabbit
                        _grid[bx, by] = 2;
                        newRabbits.Add(new Animal(bx, by, rabbit.Energy / 2));
                        rabbit.Energy /= 2;
                    }
                }
            }
            
            // Add new rabbits
            _rabbits.AddRange(newRabbits);
        }
        
        private void MoveFoxes()
        {
            List<Animal> newFoxes = new List<Animal>();
            
            for (int i = _foxes.Count - 1; i >= 0; i--)
            {
                Animal fox = _foxes[i];
                
                if (fox.HasMoved)
                    continue;
                
                // Decrease energy
                fox.Energy--;
                
                // Check if fox dies from starvation
                if (fox.Energy <= 0)
                {
                    _grid[fox.X, fox.Y] = 0; // Empty the cell
                    _foxes.RemoveAt(i);
                    continue;
                }
                
                // Find possible moves
                List<(int, int)> possibleMoves = new List<(int, int)>();
                List<(int, int)> rabbitCells = new List<(int, int)>();
                
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        
                        int nx = (fox.X + dx + _sizeX) % _sizeX;
                        int ny = (fox.Y + dy + _sizeY) % _sizeY;
                        
                        if (_grid[nx, ny] == 0 || _grid[nx, ny] == 1) // Empty or grass
                        {
                            possibleMoves.Add((nx, ny));
                        }
                        else if (_grid[nx, ny] == 2) // Rabbit
                        {
                            rabbitCells.Add((nx, ny));
                        }
                    }
                }
                
                int newX = fox.X;
                int newY = fox.Y;
                bool moved = false;
                
                // Prefer hunting rabbits if available
                if (rabbitCells.Count > 0)
                {
                    int index = _random.Next(rabbitCells.Count);
                    (newX, newY) = rabbitCells[index];
                    
                    // Find and remove the rabbit
                    for (int j = _rabbits.Count - 1; j >= 0; j--)
                    {
                        if (_rabbits[j].X == newX && _rabbits[j].Y == newY)
                        {
                            _rabbits.RemoveAt(j);
                            break;
                        }
                    }
                    
                    fox.Energy += _foxFeedEnergy;
                    if (fox.Energy > _foxMaxEnergy)
                        fox.Energy = _foxMaxEnergy;
                    
                    moved = true;
                }
                // Otherwise move to an empty cell or grass if possible
                else if (possibleMoves.Count > 0)
                {
                    int index = _random.Next(possibleMoves.Count);
                    (newX, newY) = possibleMoves[index];
                    
                    // If moving to grass, reduce grass count
                    if (_grid[newX, newY] == 1)
                        GrassCount--;
                        
                    moved = true;
                }
                
                if (moved)
                {
                    // Update grid
                    _grid[fox.X, fox.Y] = 0; // Empty old cell
                    _grid[newX, newY] = 3; // Fox in new cell
                    
                    // Update fox position
                    fox.X = newX;
                    fox.Y = newY;
                    fox.HasMoved = true;
                    
                    // Chance to breed
                    if (_random.NextDouble() < _foxBreedRate && fox.Energy > _foxMaxEnergy / 2 && possibleMoves.Count > 0)
                    {
                        int index = _random.Next(possibleMoves.Count);
                        (int bx, int by) = possibleMoves[index];
                        
                        // If moving to grass, reduce grass count
                        if (_grid[bx, by] == 1)
                            GrassCount--;
                            
                        // Create new fox
                        _grid[bx, by] = 3;
                        newFoxes.Add(new Animal(bx, by, fox.Energy / 2));
                        fox.Energy /= 2;
                    }
                }
            }
            
            // Add new foxes
            _foxes.AddRange(newFoxes);
        }
        
        // Simulation parameters adjustments
        public void SetGrassGrowthRate(double rate)
        {
            _grassGrowthRate = Math.Clamp(rate, 0.01, 0.2);
        }
        
        public void SetRabbitBreedRate(double rate)
        {
            _rabbitBreedRate = Math.Clamp(rate, 0.01, 0.2);
        }
        
        public void SetFoxBreedRate(double rate)
        {
            _foxBreedRate = Math.Clamp(rate, 0.01, 0.2);
        }
        
        public double GetGrassGrowthRate() => _grassGrowthRate;
        public double GetRabbitBreedRate() => _rabbitBreedRate;
        public double GetFoxBreedRate() => _foxBreedRate;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}
