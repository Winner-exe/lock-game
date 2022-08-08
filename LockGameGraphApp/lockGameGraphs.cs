// ReSharper disable once CheckNamespace

using System.Collections.Concurrent;
using System.Collections.Immutable;

// ReSharper disable MemberCanBePrivate.Global

namespace LockGameGraphs
{
    public static class Combinatorics
    {
        private static ImmutableList<ImmutableList<ushort>> _ans = ImmutableList<ImmutableList<ushort>>.Empty;
        private static ImmutableList<ushort> _tmp = ImmutableList<ushort>.Empty;

        private static void MakeCombiUtil(ushort n, ushort left, ushort k)
        {
            // Pushing this vector to a vector of vector
            if (k == 0)
            {
                _ans = _ans.Add(_tmp);
                return;
            }

            // i iterates from left to n. First time
            // left will be 1
            for (ushort i = left; i <= n; ++i)
            {
                _tmp = _tmp.Add(i);
                MakeCombiUtil(n, (ushort)(i + 1), (ushort)(k - 1));

                // Popping out last inserted element
                // from the vector
                _tmp = _tmp.RemoveAt(_tmp.Count - 1);
            }
        }

        // Prints all combinations of size k of numbers
        // from 1 to n.
        public static ImmutableList<ImmutableList<ushort>> DifferentCombinations(ushort n, ushort k)
        {
            MakeCombiUtil(n, 1, k);
            return _ans;
        }
    }

    // ReSharper disable once InconsistentNaming
    class ABCLockGameGraph
    {
        private readonly ushort _numHoles;
        private readonly ushort _numHands;
        private LinkedList<ushort> _colors;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ImmutableList<ImmutableList<ushort>> _selections;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ImmutableList<ushort> _recolorings;
        public readonly ImmutableList<(ushort coloring, ImmutableList<ushort> selection)> Locations;

        // ReSharper disable once NotAccessedField.Local
        public (ushort coloring, ImmutableList<ushort> selection) InitialState;
        public readonly ImmutableList<ushort> Actions;
        public readonly ImmutableHashSet<(ushort coloring, ImmutableList<ushort> selection)> Target;

        public readonly ConcurrentDictionary<((ushort coloring, ImmutableList<ushort> selection) location1, ushort
            action
            , (ushort coloring,
            ImmutableList<ushort> selection) location2), byte> Transitions;

        public ABCLockGameGraph(ushort numHoles, ushort numHands)
        {
            var rand = new Random();
            if (numHands > numHoles)
            {
                throw new Exception("Number of hands cannot exceed number of holes!");
            }

            this._numHoles = numHoles;
            this._numHands = numHands;
            this._colors = new LinkedList<ushort>(new[] { (ushort)0, (ushort)1 });
            var colorings = Enumerable.Range(0, (ushort)Math.Pow(2, _numHoles)).Select(i => (ushort)i).ToList();
            this._selections = Combinatorics.DifferentCombinations(_numHoles, _numHands)
                .Append(ImmutableList<ushort>.Empty).ToImmutableList();
            this._recolorings = Enumerable.Range(0, (ushort)Math.Pow(2, _numHands)).Select(i => (ushort)i)
                .ToImmutableList();
            this.Locations = (from coloring in colorings
                from selection in _selections
                select (coloring, selection)).ToImmutableList();
            this.InitialState = ((ushort)rand.NextInt64(0, (ushort)Math.Pow(2, _numHoles)),
                ImmutableList<ushort>.Empty);
            this.Actions = Enumerable.Range(0, _numHoles).Select(i => (ushort)i).ToImmutableList();
            var monoChromaticLocations =
                (from ushort monoColoring in new[] { (ushort)0, (ushort)(Math.Pow(2, _numHoles) - 1) }
                    from selection in _selections
                    select (monoColoring, selection)).ToImmutableList();
            this.Target = Locations.Except(monoChromaticLocations).ToImmutableHashSet();
            this.Transitions =
                new ConcurrentDictionary<((ushort coloring, ImmutableList<ushort> selection) location1, ushort action, (
                    ushort
                    coloring,
                    ImmutableList<ushort> selection) location2), byte>(8,
                    (int)Math.Pow(10, 9));
            Parallel.ForEach(Locations, location =>
            {
                Parallel.ForEach(Actions, action =>
                {
                    if (location.coloring == 0 || location.coloring == (ushort)Math.Pow(2, _numHoles) - 1)
                    {
                        Transitions.TryAdd((location, action,
                            (Rotate(location.coloring, action), location.selection)), 0);
                    }
                    else
                    {
                        Parallel.ForEach(_selections, newSelection =>
                        {
                            if (!newSelection.IsEmpty)
                            {
                                Parallel.ForEach(_recolorings, recoloring =>
                                {
                                    Transitions.TryAdd((location, action,
                                        (Recolor(Rotate(location.coloring, action), location.selection, recoloring),
                                            newSelection)), 0);
                                });
                            }
                        });
                    }
                });
            });
        }

        public ImmutableHashSet<(ushort coloring, ImmutableList<ushort> selection)> Cpre(
            ImmutableHashSet<(ushort coloring, ImmutableList<ushort> selection)> target)
        {
            var result = ImmutableHashSet<(ushort coloring, ImmutableList<ushort> selection)>.Empty;
            foreach (var location in Locations)
            {
                foreach (var action in Actions)
                {
                    var addLocation = true;
                    foreach (var newLocation in Locations)
                    {
                        var condition = Transitions.ContainsKey((location, action, newLocation)) &&
                                        !target.Contains(newLocation);
                        if (!condition) continue;
                        addLocation = false;
                        break;
                    }

                    if (!addLocation) continue;
                    result = result.Add(location);
                    break;
                }
            }

            return result;
        }

        public ImmutableHashSet<(ushort coloring, ImmutableList<ushort> selection)> ComputeWinningSpace(
            ImmutableHashSet<(ushort coloring, ImmutableList<ushort> selection)> target)
        {
            var w = target;
            var wNext = target.Intersect(Cpre(w));
            while (!w.SetEquals(wNext))
            {
                w = wNext;
                wNext = target.Intersect(Cpre(w));
            }

            return w;
        }

        public bool CanPlayerWin()
        {
            return !ComputeWinningSpace(Target).Any();
        }

        private ushort Rotate(ushort coloring, ushort rotation)
        {
            return Convert.ToUInt16(((ushort)Math.Pow(2, _numHoles) - 1) & (coloring << rotation) |
                                    (coloring >> (_numHoles - rotation)));
        }

        private ushort Recolor(ushort coloring, ImmutableList<ushort> selection, ushort recoloring)
        {
            if (!selection.IsEmpty)
            {
                var newColoring = coloring;
                for (ushort i = 0; i < _numHands; i++)
                {
                    newColoring = (recoloring & (1 << i)) != 0
                        ? Convert.ToUInt16(newColoring | (1 << selection[i]))
                        : Convert.ToUInt16(newColoring & ~(1 << selection[i]));
                }

                return newColoring;
            }
            else
            {
                return coloring;
            }
        }
    }

    internal static class Driver
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            var lockGame = new ABCLockGameGraph(8, 4);
            Console.WriteLine("Game Graph Initialized. Proceeding to calculate existence of winning strategy...");
            var canPlayerWin = lockGame.CanPlayerWin();
            Console.WriteLine(
                canPlayerWin
                    ? "Winning space for the Adversary is empty. Surely winning strategy may exist for the Player."
                    : "Winning space for the Adversary is non-empty. Player cannot surely win.");
        }
    }
}