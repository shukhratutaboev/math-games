namespace MathGames.Games;

public class LangtonsAntBase
{
    private void NotifyStateChanged() => OnChangeAsync?.Invoke();
    public bool Paused { get; set; }
    private readonly int _sizeX;
    private readonly int _sizeY;
    private int _refreshRate = 100;
    private bool[,] _cells;
    private int _antX;
    private int _antY;
    private int _antDirection; // 0 = up, 1 = right, 2 = down, 3 = left
    private int _steps;

    public event Func<Task>? OnChangeAsync;
    public int SizeX => _sizeX;
    public int SizeY => _sizeY;
    public int AntX => _antX;
    public int AntY => _antY;
    public int AntDirection => _antDirection;
    public int Steps => _steps;

    public LangtonsAntBase(int sizeX, int sizeY)
    {
        _sizeX = sizeX;
        _sizeY = sizeY;
        _cells = new bool[sizeX, sizeY];
        Paused = true;
        _antX = sizeX / 2;
        _antY = sizeY / 2;
        _antDirection = 0;
        _steps = 0;

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
        _antX = _sizeX / 2;
        _antY = _sizeY / 2;
        _antDirection = 0;
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

    private void Step()
    {
        _steps++;
        if (_antX < 0 || _antX >= _sizeX || _antY < 0 || _antY >= _sizeY)
        {
            Paused = true;
            return;
        }

        if (!_cells[_antX, _antY])
        {
            _antDirection = (_antDirection + 1) % 4;
        }
        else
        {
            _antDirection = (_antDirection + 3) % 4;
        }

        _cells[_antX, _antY] = !_cells[_antX, _antY];

        switch (_antDirection)
        {
            case 0:
                _antX--;
                break;
            case 1:
                _antY++;
                break;
            case 2:
                _antX++;
                break;
            case 3:
                _antY--;
                break;
        }
    }
}