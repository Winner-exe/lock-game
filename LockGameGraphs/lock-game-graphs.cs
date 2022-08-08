// ReSharper disable once CheckNamespace

namespace LockGameGraphs
{
    public static class Combinatorics
    {
        public static IEnumerable<IEnumerable<T>> DifferentCombinations<T>(this IEnumerable<T> elements, int k)
        {
            var enumerable = elements as T[] ?? elements.ToArray();
            return k == 0
                ? new[] { Array.Empty<T>() }
                : enumerable.SelectMany((e, i) =>
                    enumerable.Skip(i + 1).DifferentCombinations(k - 1).Select(c => (new[] { e }).Concat(c)));
        }
    }

    // ReSharper disable once InconsistentNaming
    class ABCLockGameGraph
    {
        private readonly int _numHoles;
        private readonly int _numHands;
        private IEnumerable<int> _colors;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IEnumerable<IEnumerable<int>> _selections;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IEnumerable<int> _recolorings;
        private readonly IEnumerable<(int coloring, IEnumerable<int> selection)> _locations;
        private (int coloring, IEnumerable<int> selection) _initialState;
        private readonly IEnumerable<int> _actions;
        private readonly HashSet<(int coloring, IEnumerable<int> selection)> _target;

        private readonly HashSet<((int coloring, IEnumerable<int> selection) location1, int action
            , (int coloring,
            IEnumerable<int> selection) location2)> _transitions;

        public ABCLockGameGraph(int numHoles, int numHands)
        {
            var rand = new Random();
            if (numHands > numHoles)
            {
                throw new Exception("Number of hands cannot exceed number of holes!");
            }

            this._numHoles = numHoles;
            this._numHands = numHands;
            this._colors = Enumerable.Range(0, 1);
            var colorings = Enumerable.Range(0, (int)Math.Pow(2, _numHoles));
            this._selections = Enumerable.Range(0, _numHoles).DifferentCombinations(_numHands)
                .Append(Enumerable.Empty<int>());
            this._recolorings = Enumerable.Range(0, (int)Math.Pow(2, _numHands));
            this._locations = from coloring in colorings
                from selection in _selections
                select (coloring, selection);
            this._initialState = ((int)rand.NextInt64(0, (int)Math.Pow(2, _numHoles)), Enumerable.Empty<int>());
            this._actions = Enumerable.Range(0, _numHoles);
            var locationList = _locations.ToList();
            this._transitions = (from location in locationList
                from action in _actions
                from newSelection in _selections
                from recoloring in _recolorings
                where newSelection.Any()
                select (location, action,
                    (Recolor(Rotate(location.coloring, action), location.selection, recoloring),
                        newSelection))).ToHashSet();

            var playerTarget = from monoColoring in new int[] { 0, (int)Math.Pow(2, _numHoles) - 1 }
                from selection in _selections
                select (monoColoring, selection);
            this._target = locationList.Except(playerTarget).ToHashSet();
        }

        public HashSet<(int coloring, IEnumerable<int> selection)> Cpre(
            HashSet<(int coloring, IEnumerable<int> selection)> target)
        {
            return (from location in _locations
                from action in _actions
                where _locations.All(newLocation =>
                    _transitions.Contains((location, action, newLocation)) && target.Contains(newLocation))
                select location).ToHashSet();
        }

        public HashSet<(int coloring, IEnumerable<int> selection)> ComputeWinningSpace(
            HashSet<(int coloring, IEnumerable<int> selection)> target)
        {
            var w = target;
            var wNext = target.Intersect(Cpre(target)).ToHashSet();
            while (!w.Equals(wNext))
            {
                w = wNext;
                wNext = w.Intersect(Cpre(w)).ToHashSet();
            }

            return w;
        }

        public bool CanPlayerWin()
        {
            return !ComputeWinningSpace(_target).Any();
        }

        private int Rotate(int coloring, int rotation)
        {
            return ((int)Math.Pow(2, _numHoles) - 1) & ((coloring << rotation) | (coloring >> (_numHoles - rotation)));
        }

        private int Recolor(int coloring, IEnumerable<int> selection, int recoloring)
        {
            var newColoring = coloring;
            using var selectionIter = selection.GetEnumerator();
            for (var i = 0; i < _numHands; i++)
            {
                selectionIter.MoveNext();
                if ((recoloring & (1 << i)) == 1)
                    newColoring = newColoring | (1 << selectionIter.Current);
                else
                    newColoring = newColoring & ~(1 << selectionIter.Current);
            }

            return newColoring;
        }
    }

    class Driver
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            var lockGame = new ABCLockGameGraph(4, 2);
            Console.WriteLine("Game Graph Initialized. Proceeding to calculate existence of winning strategy...");
            var canPlayerWin = lockGame.CanPlayerWin();
            if (canPlayerWin)
                Console.WriteLine(
                    "Winning space for the Adversary is empty. Surely winning strategy may exist for the Player.");
            else
                Console.WriteLine("Winning space for the Adversary is non-empty. Player cannot surely win.");
        }
    }
}