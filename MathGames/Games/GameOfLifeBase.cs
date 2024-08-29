namespace MathGames.Games;

public class GameOfLifeBase
{
    private void NotifyStateChanged() => OnChangeAsync?.Invoke();
    public bool Paused { get; set; }
    private readonly int _sizeX;
    private readonly int _sizeY;
    private int _refreshRate = 500;
    private bool[,] _cells;

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
            while (true)
            {
                if (!Paused)
                {
                    Step();
                    NotifyStateChanged();
                }
                await Task.Delay(_refreshRate);
            }
        });
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
}