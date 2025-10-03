namespace MathGames.Games;

public class GameOfLifeBase : IDisposable
{
    private void NotifyStateChanged() => OnChangeAsync?.Invoke();
    public bool Paused { get; set; }
    private readonly int _sizeX;
    private readonly int _sizeY;
    private int _refreshRate = 500;
    private bool[,] _cells;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _disposed = false;

    public event Func<Task>? OnChangeAsync;
    public int SizeX => _sizeX;
    public int SizeY => _sizeY;

    public GameOfLifeBase(int sizeX, int sizeY)
    {
        _sizeX = sizeX;
        _sizeY = sizeY;
        _cells = new bool[sizeX, sizeY];
        Paused = true;

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

    public void Randomize()
    {
        Paused = true;
        var random = new Random();
        for (var y = 0; y != _sizeY; ++y)
        {
            for (var x = 0; x != _sizeX; ++x)
            {
                _cells[x, y] = random.Next(0, 2) == 1;
            }
        }
        NotifyStateChanged();
    }

    public void Reset()
    {
        Paused = true;
        _cells = new bool[_sizeX, _sizeY];
        NotifyStateChanged();
    }

    public void Pause()
    {
        Paused = true;
        NotifyStateChanged();
    }

    public void Next()
    {
        Paused = true;
        Step();
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

    public void ToggleCell(int x, int y)
    {
        _cells[x, y] = !_cells[x, y];
        NotifyStateChanged();
    }

    public void LoadPattern(GameOfLifePatterns.Pattern pattern, int startX, int startY)
    {
        Paused = true;
        
        for (int x = 0; x < pattern.Width && startX + x < _sizeX; x++)
        {
            for (int y = 0; y < pattern.Height && startY + y < _sizeY; y++)
            {
                _cells[startX + x, startY + y] = pattern.Cells[x, y];
            }
        }
        
        NotifyStateChanged();
    }

    public void LoadPatternCentered(GameOfLifePatterns.Pattern pattern)
    {
        int startX = (_sizeX - pattern.Width) / 2;
        int startY = (_sizeY - pattern.Height) / 2;
        LoadPattern(pattern, startX, startY);
    }

    private void Step()
    {
        var newGeneration = new bool[_sizeX, _sizeY];

        for (var y = 0; y != _sizeY; ++y)
        {
            for (var x = 0; x != _sizeX; ++x)
            {
                var neighborOffsets = new (int, int)[]
                {
                    (-1, 0), (-1, 1), (0, 1), (1, 1),
                    (1, 0), (1, -1), (0, -1), (-1, -1)
                };
                var aliveNeighbor = neighborOffsets.Sum(offset =>IsNeighborAlive(x + offset.Item1, y + offset.Item2));

                var isAlive = _cells[x, y];

                var survives = (!isAlive && aliveNeighbor == 3) || (isAlive && (aliveNeighbor == 2 || aliveNeighbor == 3));

                newGeneration[x, y] = survives;
            }
        }

        _cells = newGeneration;
    }

    private int IsNeighborAlive(int x, int y)
    {
        var outOfBounds = x < 0 || x >= _sizeX || y < 0 || y >= _sizeY;
        return (!outOfBounds) && _cells[x, y] ? 1 : 0;
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