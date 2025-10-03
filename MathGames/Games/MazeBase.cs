using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MathGames.Games
{
    public class MazeBase : IDisposable
    {
        private readonly object _lock = new object();
        private void NotifyStateChanged() => OnChangeAsync?.Invoke();
        private bool _initialized = false;
        public bool Paused { get; set; } = true;
        private readonly int _sizeX;
        private readonly int _sizeY;
        private int _refreshRate = 100;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _disposed = false;
        
        // 0 = wall, 1 = path, 2 = visited (for generation), 
        // 3 = visited (for solving), 4 = solution path, 5 = start, 6 = end
        private int[,] _grid;
        
        // Algorithm types
        public enum GenerationAlgorithm { RecursiveBacktracking, Kruskal, Eller }
        public enum SolvingAlgorithm { BreadthFirstSearch, DepthFirstSearch, AStar }
        
        // Current settings
        public GenerationAlgorithm CurrentGenerationAlgorithm { get; private set; } = GenerationAlgorithm.RecursiveBacktracking;
        public SolvingAlgorithm CurrentSolvingAlgorithm { get; private set; } = SolvingAlgorithm.BreadthFirstSearch;
        
        // State tracking
        private bool _isGenerating = false;
        private bool _isSolving = false;
        
        // Start and end points
        private (int x, int y) _start = (1, 1);
        private (int x, int y) _end = (0, 0);
        
        // Statistics
        private int _stepsToGenerate = 0;
        private int _stepsToSolve = 0;
        private int _solutionLength = 0;
        
        // For algorithms
        private Random _random = new Random();
        private Stack<(int x, int y)> _generationStack = new Stack<(int x, int y)>();
        private List<HashSet<(int x, int y)>> _kruskalSets = new List<HashSet<(int x, int y)>>();
        private List<(int x, int y, int x2, int y2)> _kruskalWalls = new List<(int x, int y, int x2, int y2)>();
        
        private Queue<(int x, int y, List<(int x, int y)> path)>? _bfsQueue;
        private Stack<(int x, int y, List<(int x, int y)> path)>? _dfsStack;
        private PriorityQueue<(int x, int y, List<(int x, int y)> path), int>? _aStarQueue;
        private HashSet<(int x, int y)>? _visitedCells;

        public event Func<Task>? OnChangeAsync;
        public int SizeX => _sizeX;
        public int SizeY => _sizeY;
        
        // Public properties for statistics
        public int StepsToGenerate => _stepsToGenerate;
        public int StepsToSolve => _stepsToSolve;
        public int SolutionLength => _solutionLength;
        
        public MazeBase(int sizeX, int sizeY)
        {
            // Ensure odd dimensions for proper maze walls
            _sizeX = sizeX % 2 == 0 ? sizeX + 1 : sizeX;
            _sizeY = sizeY % 2 == 0 ? sizeY + 1 : sizeY;
            _grid = new int[_sizeX, _sizeY];
            _end = (_sizeX - 2, _sizeY - 2);
            
            // Initialize collections to avoid null references
            _bfsQueue = new Queue<(int x, int y, List<(int x, int y)> path)>();
            _dfsStack = new Stack<(int x, int y, List<(int x, int y)> path)>();
            _aStarQueue = new PriorityQueue<(int x, int y, List<(int x, int y)> path), int>();
            _visitedCells = new HashSet<(int x, int y)>();

            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (!Paused)
                    {
                        if (_isGenerating)
                        {
                            GenerationStep();
                        }
                        else if (_isSolving)
                        {
                            SolvingStep();
                        }
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
        
        public void SetRefreshRate(int rate)
        {
            _refreshRate = rate;
        }
        
        public int GetCell(int x, int y)
        {
            return _grid[x, y];
        }
        
        public void SetGenerationAlgorithm(GenerationAlgorithm algorithm)
        {
            if (_isGenerating || _isSolving) return;
            CurrentGenerationAlgorithm = algorithm;
            NotifyStateChanged();
        }
        
        public void SetSolvingAlgorithm(SolvingAlgorithm algorithm)
        {
            if (_isGenerating || _isSolving) return;
            CurrentSolvingAlgorithm = algorithm;
            NotifyStateChanged();
        }
        
        public void InitializeMaze()
        {
            if (_isGenerating || _isSolving) return;
            
            lock (_lock)
            {
                // Initialize the grid with walls
                for (int x = 0; x < _sizeX; x++)
                {
                    for (int y = 0; y < _sizeY; y++)
                    {
                        // Make every cell a wall initially
                        _grid[x, y] = 0;
                    }
                }
                
                _initialized = true;
                _isGenerating = false;
                _isSolving = false;
                _stepsToGenerate = 0;
                _stepsToSolve = 0;
                _solutionLength = 0;
                
                NotifyStateChanged();
            }
        }
        
        public void Play()
        {
            if (!_initialized)
                InitializeMaze();
                
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
            if (!_initialized)
                InitializeMaze();
                
            if (_isGenerating)
            {
                GenerationStep();
            }
            else if (_isSolving)
            {
                SolvingStep();
            }
            NotifyStateChanged();
        }
        
        public void StartGeneration()
        {
            if (_isGenerating || _isSolving) return;
            
            InitializeMaze();
            
            _isGenerating = true;
            _stepsToGenerate = 0;
            
            switch (CurrentGenerationAlgorithm)
            {
                case GenerationAlgorithm.RecursiveBacktracking:
                    InitializeRecursiveBacktracking();
                    break;
                case GenerationAlgorithm.Kruskal:
                    InitializeKruskal();
                    break;
                case GenerationAlgorithm.Eller:
                    InitializeEller();
                    break;
            }
            
            NotifyStateChanged();
        }
        
        public void StartSolving()
        {
            if (_isGenerating || _isSolving || !MazeIsGenerated()) return;
            
            // Reset maze for solving (clear previous solution)
            for (int x = 0; x < _sizeX; x++)
            {
                for (int y = 0; y < _sizeY; y++)
                {
                    if (_grid[x, y] == 3 || _grid[x, y] == 4)
                    {
                        _grid[x, y] = 1;
                    }
                }
            }
            
            _isSolving = true;
            _stepsToSolve = 0;
            _solutionLength = 0;
            
            // Set start and end points
            _grid[_start.x, _start.y] = 5;
            _grid[_end.x, _end.y] = 6;
            
            // Initialize the appropriate solving algorithm (ensures collections are created)
            switch (CurrentSolvingAlgorithm)
            {
                case SolvingAlgorithm.BreadthFirstSearch:
                    InitializeBFS();
                    break;
                case SolvingAlgorithm.DepthFirstSearch:
                    InitializeDFS();
                    break;
                case SolvingAlgorithm.AStar:
                    InitializeAStar();
                    break;
            }
            
            NotifyStateChanged();
        }
        
        public void SetStartPoint(int x, int y)
        {
            if (_isGenerating || _isSolving || _grid[x, y] == 0) return;
            
            // Remove previous start
            _grid[_start.x, _start.y] = 1;
            
            // Set new start
            _start = (x, y);
            _grid[x, y] = 5;
            
            NotifyStateChanged();
        }
        
        public void SetEndPoint(int x, int y)
        {
            if (_isGenerating || _isSolving || _grid[x, y] == 0) return;
            
            // Remove previous end
            _grid[_end.x, _end.y] = 1;
            
            // Set new end
            _end = (x, y);
            _grid[x, y] = 6;
            
            NotifyStateChanged();
        }
        
        private bool MazeIsGenerated()
        {
            // Check if there's at least one path cell
            for (int x = 0; x < _sizeX; x++)
            {
                for (int y = 0; y < _sizeY; y++)
                {
                    if (_grid[x, y] == 1 || _grid[x, y] == 5 || _grid[x, y] == 6)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        #region Generation Algorithms
        
        private void InitializeRecursiveBacktracking()
        {
            // Start at a random odd position
            int startX = 1 + 2 * _random.Next((_sizeX - 1) / 2);
            int startY = 1 + 2 * _random.Next((_sizeY - 1) / 2);
            
            if (startX >= _sizeX) startX = _sizeX - 2;
            if (startY >= _sizeY) startY = _sizeY - 2;
            
            _grid[startX, startY] = 1; // Mark as path
            _generationStack.Clear();
            _generationStack.Push((startX, startY));
        }
        
        private void InitializeKruskal()
        {
            _kruskalSets.Clear();
            _kruskalWalls.Clear();
            
            // Initialize all path cells (odd coordinates)
            for (int x = 1; x < _sizeX; x += 2)
            {
                for (int y = 1; y < _sizeY; y += 2)
                {
                    _grid[x, y] = 1; // Path
                    
                    // Each cell starts in its own set
                    var cellSet = new HashSet<(int x, int y)>();
                    cellSet.Add((x, y));
                    _kruskalSets.Add(cellSet);
                }
            }
            
            // Collect all internal walls
            for (int x = 1; x < _sizeX - 1; x += 2)
            {
                for (int y = 1; y < _sizeY - 1; y += 2)
                {
                    // Add horizontal wall
                    if (x + 2 < _sizeX)
                        _kruskalWalls.Add((x, y, x + 2, y));
                    
                    // Add vertical wall
                    if (y + 2 < _sizeY)
                        _kruskalWalls.Add((x, y, x, y + 2));
                }
            }
            
            // Shuffle walls
            ShuffleWalls();
        }
        
        private void ShuffleWalls()
        {
            int n = _kruskalWalls.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                var value = _kruskalWalls[k];
                _kruskalWalls[k] = _kruskalWalls[n];
                _kruskalWalls[n] = value;
            }
        }
        
        private void InitializeEller()
        {
            // For simplicity, we'll implement Eller's algorithm as a series
            // of steps rather than a complete initialization. It works row by row.
            
            // Start with cells in the first row
            for (int x = 1; x < _sizeX; x += 2)
            {
                _grid[x, 1] = 1; // Path
            }
        }
        
        private void GenerationStep()
        {
            _stepsToGenerate++;
            
            switch (CurrentGenerationAlgorithm)
            {
                case GenerationAlgorithm.RecursiveBacktracking:
                    RecursiveBacktrackingStep();
                    break;
                case GenerationAlgorithm.Kruskal:
                    KruskalStep();
                    break;
                case GenerationAlgorithm.Eller:
                    EllerStep();
                    break;
            }
        }
        
        private void RecursiveBacktrackingStep()
        {
            if (_generationStack.Count == 0)
            {
                _isGenerating = false;
                FinalizeGeneration();
                return;
            }
            
            var current = _generationStack.Peek();
            
            // Find unvisited neighbors
            List<(int x, int y)> neighbors = new List<(int x, int y)>();
            
            // Check in all four directions
            int[][] directions = new int[][] { new int[] { 0, -2 }, new int[] { 2, 0 }, new int[] { 0, 2 }, new int[] { -2, 0 } };
            
            foreach (var dir in directions)
            {
                int newX = current.x + dir[0];
                int newY = current.y + dir[1];
                
                // Check if in bounds and unvisited
                if (newX > 0 && newX < _sizeX - 1 && newY > 0 && newY < _sizeY - 1 &&
                    _grid[newX, newY] == 0)
                {
                    neighbors.Add((newX, newY));
                }
            }
            
            if (neighbors.Count > 0)
            {
                // Choose a random neighbor
                var next = neighbors[_random.Next(neighbors.Count)];
                
                // Create a path between current and next
                _grid[current.x + (next.x - current.x) / 2, current.y + (next.y - current.y) / 2] = 1;
                
                // Mark the neighbor as visited
                _grid[next.x, next.y] = 1;
                
                // Push the neighbor to the stack
                _generationStack.Push(next);
            }
            else
            {
                // Mark current as visited but backtrack
                _generationStack.Pop();
            }
        }
        
        private void KruskalStep()
        {
            if (_kruskalWalls.Count == 0 || _kruskalSets.Count <= 1)
            {
                _isGenerating = false;
                FinalizeGeneration();
                return;
            }
            
            // Take a wall
            var wall = _kruskalWalls[_kruskalWalls.Count - 1];
            _kruskalWalls.RemoveAt(_kruskalWalls.Count - 1);
            
            // Find the sets containing the cells on either side of the wall
            HashSet<(int x, int y)> set1 = null;
            HashSet<(int x, int y)> set2 = null;
            
            foreach (var set in _kruskalSets)
            {
                if (set.Contains((wall.x, wall.y)))
                    set1 = set;
                if (set.Contains((wall.x2, wall.y2)))
                    set2 = set;
                
                if (set1 != null && set2 != null)
                    break;
            }
            
            // If the cells are in different sets, remove the wall and merge the sets
            if (set1 != null && set2 != null && set1 != set2)
            {
                // Remove the wall
                int wallX = wall.x + (wall.x2 - wall.x) / 2;
                int wallY = wall.y + (wall.y2 - wall.y) / 2;
                _grid[wallX, wallY] = 1;
                
                // Merge the sets
                _kruskalSets.Remove(set2);
                set1.UnionWith(set2);
            }
        }
        
        private void EllerStep()
        {
            // A simplified version of Eller's algorithm
            // For each step, we'll process one row
            
            // Find the current row we're working on
            int currentRow = -1;
            
            for (int y = 1; y < _sizeY; y += 2)
            {
                bool rowInProgress = false;
                for (int x = 1; x < _sizeX; x += 2)
                {
                    if (_grid[x, y] == 1 && (y + 2 >= _sizeY || _grid[x, y + 2] == 0))
                    {
                        rowInProgress = true;
                        break;
                    }
                }
                
                if (rowInProgress)
                {
                    currentRow = y;
                    break;
                }
            }
            
            if (currentRow == -1 || currentRow + 2 >= _sizeY)
            {
                _isGenerating = false;
                FinalizeGeneration();
                return;
            }
            
            // Randomly connect cells horizontally
            List<int> sets = new List<int>();
            Dictionary<int, List<int>> setMembers = new Dictionary<int, List<int>>();
            
            // Assign each cell in the row to a set
            int nextSet = 1;
            for (int x = 1; x < _sizeX; x += 2)
            {
                if (_grid[x, currentRow] != 1) continue;
                
                if (x > 1 && _grid[x - 1, currentRow] == 1)
                {
                    // Join the previous set
                    sets.Add(sets[sets.Count - 1]);
                }
                else
                {
                    // Create a new set
                    sets.Add(nextSet++);
                }
                
                // Add this cell to its set
                if (!setMembers.ContainsKey(sets[sets.Count - 1]))
                {
                    setMembers[sets[sets.Count - 1]] = new List<int>();
                }
                setMembers[sets[sets.Count - 1]].Add(x);
            }
            
            // Randomly merge adjacent sets
            for (int i = 0; i < sets.Count - 1; i++)
            {
                if (sets[i] != sets[i + 1] && _random.Next(2) == 0)
                {
                    // Merge by knocking down a wall
                    int x1 = setMembers[sets[i]].Last();
                    int x2 = setMembers[sets[i + 1]].First();
                    _grid[(x1 + x2) / 2, currentRow] = 1;
                    
                    // Update set info
                    int oldSet = sets[i + 1];
                    foreach (var x in setMembers[oldSet])
                    {
                        setMembers[sets[i]].Add(x);
                    }
                    setMembers.Remove(oldSet);
                    
                    for (int j = i + 1; j < sets.Count; j++)
                    {
                        if (sets[j] == oldSet)
                        {
                            sets[j] = sets[i];
                        }
                    }
                }
            }
            
            // For each set, create at least one vertical connection to the next row
            foreach (var setId in setMembers.Keys)
            {
                if (currentRow + 2 < _sizeY)
                {
                    // Guarantee at least one connection
                    int rand = _random.Next(setMembers[setId].Count);
                    int x = setMembers[setId][rand];
                    
                    // Create a path to the next row
                    _grid[x, currentRow + 1] = 1;
                    _grid[x, currentRow + 2] = 1;
                    
                    // Make additional random connections
                    foreach (var cellX in setMembers[setId])
                    {
                        if (cellX != x && _random.Next(3) == 0)
                        {
                            _grid[cellX, currentRow + 1] = 1;
                            _grid[cellX, currentRow + 2] = 1;
                        }
                    }
                }
            }
        }
        
        private void FinalizeGeneration()
        {
            // Set start and end positions
            _start = (1, 1);
            _end = (_sizeX - 2, _sizeY - 2);
            
            // Ensure path cells are all set to 1
            for (int x = 0; x < _sizeX; x++)
            {
                for (int y = 0; y < _sizeY; y++)
                {
                    if (_grid[x, y] != 0)
                    {
                        _grid[x, y] = 1;
                    }
                }
            }
            
            // Set start and end
            _grid[_start.x, _start.y] = 5;
            _grid[_end.x, _end.y] = 6;
            
            NotifyStateChanged();
        }
        
        #endregion
        
        #region Solving Algorithms
        
        private void InitializeBFS()
        {
            _bfsQueue = new Queue<(int x, int y, List<(int x, int y)> path)>();
            _visitedCells = new HashSet<(int x, int y)>();
            
            var initialPath = new List<(int x, int y)> { _start };
            _bfsQueue.Enqueue((_start.x, _start.y, initialPath));
            _visitedCells.Add(_start);
        }
        
        private void InitializeDFS()
        {
            _dfsStack = new Stack<(int x, int y, List<(int x, int y)> path)>();
            _visitedCells = new HashSet<(int x, int y)>();
            
            var initialPath = new List<(int x, int y)> { _start };
            _dfsStack.Push((_start.x, _start.y, initialPath));
            _visitedCells.Add(_start);
        }
        
        private void InitializeAStar()
        {
            _aStarQueue = new PriorityQueue<(int x, int y, List<(int x, int y)> path), int>();
            _visitedCells = new HashSet<(int x, int y)>();
            
            var initialPath = new List<(int x, int y)> { _start };
            int priority = ManhattanDistance(_start.x, _start.y, _end.x, _end.y);
            _aStarQueue.Enqueue((_start.x, _start.y, initialPath), priority);
            _visitedCells.Add(_start);
        }
        
        private int ManhattanDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }
        
        private void SolvingStep()
        {
            _stepsToSolve++;
            
            switch (CurrentSolvingAlgorithm)
            {
                case SolvingAlgorithm.BreadthFirstSearch:
                    BFSStep();
                    break;
                case SolvingAlgorithm.DepthFirstSearch:
                    DFSStep();
                    break;
                case SolvingAlgorithm.AStar:
                    AStarStep();
                    break;
            }
        }
        
        private void BFSStep()
        {
            if (_bfsQueue == null || _bfsQueue.Count == 0 || _visitedCells == null)
            {
                _isSolving = false;
                return;
            }
            
            var current = _bfsQueue.Dequeue();
            
            // Check if we reached the end
            if (current.x == _end.x && current.y == _end.y)
            {
                TraceFoundPath(current.path);
                return;
            }
            
            // Mark as visited
            if (_grid[current.x, current.y] != 5 && _grid[current.x, current.y] != 6)
            {
                _grid[current.x, current.y] = 3;
            }
            
            // Explore neighbors
            int[][] directions = new int[][] { new int[] { 0, -1 }, new int[] { 1, 0 }, new int[] { 0, 1 }, new int[] { -1, 0 } };
            
            foreach (var dir in directions)
            {
                int newX = current.x + dir[0];
                int newY = current.y + dir[1];
                
                if (newX > 0 && newX < _sizeX && newY > 0 && newY < _sizeY && 
                    (_grid[newX, newY] == 1 || _grid[newX, newY] == 6) &&
                    !_visitedCells.Contains((newX, newY)))
                {
                    // Create new path
                    var newPath = new List<(int x, int y)>(current.path);
                    newPath.Add((newX, newY));
                    
                    _bfsQueue.Enqueue((newX, newY, newPath));
                    _visitedCells.Add((newX, newY));
                }
            }
        }
        
        private void DFSStep()
        {
            if (_dfsStack == null || _dfsStack.Count == 0 || _visitedCells == null)
            {
                _isSolving = false;
                return;
            }
            
            var current = _dfsStack.Pop();
            
            // Check if we reached the end
            if (current.x == _end.x && current.y == _end.y)
            {
                TraceFoundPath(current.path);
                return;
            }
            
            // Mark as visited
            if (_grid[current.x, current.y] != 5 && _grid[current.x, current.y] != 6)
            {
                _grid[current.x, current.y] = 3;
            }
            
            // Explore neighbors
            int[][] directions = new int[][] { new int[] { 0, -1 }, new int[] { 1, 0 }, new int[] { 0, 1 }, new int[] { -1, 0 } };
            
            foreach (var dir in directions)
            {
                int newX = current.x + dir[0];
                int newY = current.y + dir[1];
                
                if (newX > 0 && newX < _sizeX && newY > 0 && newY < _sizeY && 
                    (_grid[newX, newY] == 1 || _grid[newX, newY] == 6) &&
                    !_visitedCells.Contains((newX, newY)))
                {
                    // Create new path
                    var newPath = new List<(int x, int y)>(current.path);
                    newPath.Add((newX, newY));
                    
                    _dfsStack.Push((newX, newY, newPath));
                    _visitedCells.Add((newX, newY));
                }
            }
        }
        
        private void AStarStep()
        {
            if (_aStarQueue == null || _aStarQueue.Count == 0 || _visitedCells == null)
            {
                _isSolving = false;
                return;
            }
            
            var current = _aStarQueue.Dequeue();
            
            // Check if we reached the end
            if (current.x == _end.x && current.y == _end.y)
            {
                TraceFoundPath(current.path);
                return;
            }
            
            // Mark as visited
            if (_grid[current.x, current.y] != 5 && _grid[current.x, current.y] != 6)
            {
                _grid[current.x, current.y] = 3;
            }
            
            // Explore neighbors
            int[][] directions = new int[][] { new int[] { 0, -1 }, new int[] { 1, 0 }, new int[] { 0, 1 }, new int[] { -1, 0 } };
            
            foreach (var dir in directions)
            {
                int newX = current.x + dir[0];
                int newY = current.y + dir[1];
                
                if (newX > 0 && newX < _sizeX && newY > 0 && newY < _sizeY && 
                    (_grid[newX, newY] == 1 || _grid[newX, newY] == 6) &&
                    !_visitedCells.Contains((newX, newY)))
                {
                    // Create new path
                    var newPath = new List<(int x, int y)>(current.path);
                    newPath.Add((newX, newY));
                    
                    // Calculate priority using path length + Manhattan distance to end
                    int priority = newPath.Count + ManhattanDistance(newX, newY, _end.x, _end.y);
                    
                    _aStarQueue.Enqueue((newX, newY, newPath), priority);
                    _visitedCells.Add((newX, newY));
                }
            }
        }
        
        private void TraceFoundPath(List<(int x, int y)> path)
        {
            _isSolving = false;
            
            if (path == null)
            {
                NotifyStateChanged();
                return;
            }
            
            _solutionLength = path.Count - 1; // Don't count the start point
            
            // Mark the final path
            foreach (var point in path)
            {
                if (_grid[point.x, point.y] != 5 && _grid[point.x, point.y] != 6)
                {
                    _grid[point.x, point.y] = 4;
                }
            }
            
            NotifyStateChanged();
        }
        
        #endregion

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
