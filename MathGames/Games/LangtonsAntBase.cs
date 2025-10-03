namespace MathGames.Games;

public class LangtonsAntBase : IDisposable
{
    private void NotifyStateChanged() => OnChangeAsync?.Invoke();
    public bool Paused { get; set; }
    private readonly int _sizeX;
    private readonly int _sizeY;
    private int _refreshRate = 100;
    private bool[,] _cells;
    private int _steps;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _disposed = false;

    // List to store multiple ants
    private List<Ant> _ants = new List<Ant>();

    // Inner class to represent an individual ant
    public class Ant
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Direction { get; set; } // 0 = up, 1 = right, 2 = down, 3 = left
        public bool IsActive { get; set; } = true;

        public Ant(int x, int y, int direction)
        {
            X = x;
            Y = y;
            Direction = direction;
        }
    }

    public event Func<Task>? OnChangeAsync;
    public int SizeX => _sizeX;
    public int SizeY => _sizeY;

    // Update properties to get values from the most recently added active ant
    public int AntX => _ants.LastOrDefault(a => a.IsActive)?.X ?? _sizeX / 2;
    public int AntY => _ants.LastOrDefault(a => a.IsActive)?.Y ?? _sizeY / 2;
    public int AntDirection => _ants.LastOrDefault(a => a.IsActive)?.Direction ?? 0;

    public int Steps => _steps;

    // Add a public accessor for the ants collection
    public IReadOnlyList<Ant> Ants => _ants.AsReadOnly();

    public LangtonsAntBase(int sizeX, int sizeY)
    {
        _sizeX = sizeX;
        _sizeY = sizeY;
        _cells = new bool[sizeX, sizeY];
        Paused = true;

        // Initialize with one ant in the center
        _ants.Add(new Ant(sizeX / 2, sizeY / 2, 0));
        _steps = 0;

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
        _cells = new bool[_sizeX, _sizeY];

        // Reset to just one ant in the center
        _ants.Clear();
        _ants.Add(new Ant(_sizeX / 2, _sizeY / 2, 0));

        _steps = 0;
        NotifyStateChanged();
    }

    public void SetRefreshRate(int rate)
    {
        _refreshRate = rate;
    }

    public bool GetCell(int x, int y)
    {
        return _cells[x, y];
    }

    public void CreateAntAt(int x, int y)
    {
        // Make sure the coordinates are within bounds
        if (x >= 0 && x < _sizeX && y >= 0 && y < _sizeY)
        {
            // Create a new ant at the clicked position
            // Direction is randomly chosen
            int direction = new Random().Next(4);
            _ants.Add(new Ant(x, y, direction));

            // Notify that the state has changed
            NotifyStateChanged();
        }
    }

    private void Step()
    {
        _steps++;

        // Process each ant in the list
        for (int i = 0; i < _ants.Count; i++)
        {
            Ant ant = _ants[i];

            // Skip inactive ants
            if (!ant.IsActive) continue;

            // Check bounds
            if (ant.X < 0 || ant.X >= _sizeX || ant.Y < 0 || ant.Y >= _sizeY)
            {
                ant.IsActive = false;
                continue;
            }

            // Apply Langton's Ant rules
            if (!_cells[ant.X, ant.Y])
            {
                ant.Direction = (ant.Direction + 1) % 4;
            }
            else
            {
                ant.Direction = (ant.Direction + 3) % 4;
            }

            // Flip the cell color
            _cells[ant.X, ant.Y] = !_cells[ant.X, ant.Y];

            // Move the ant
            switch (ant.Direction)
            {
                case 0:
                    ant.X--;
                    break;
                case 1:
                    ant.Y++;
                    break;
                case 2:
                    ant.X++;
                    break;
                case 3:
                    ant.Y--;
                    break;
            }
        }

        // Remove inactive ants
        _ants.RemoveAll(a => !a.IsActive);

        // If all ants are gone, pause the simulation
        if (!_ants.Any())
        {
            Paused = true;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}