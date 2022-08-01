import random
from itertools import cycle, combinations

from gamegraphs import *


class AdversaryLockGameGraph(GameGraph):
    def __init__(self, num_holes, num_hands, num_colors):
        if num_hands > num_holes:
            raise Exception("Number of hands exceeds number of holes!")

        self.num_holes = num_holes
        self.num_hands = num_hands
        self.num_colors = num_colors
        self.colors = list(range(num_colors))
        self.colorings = list(product(self.colors, repeat=num_holes))
        self.selections = list(combinations(range(num_holes), num_hands))
        self.selections.append(tuple())
        self.recolorings = list(product(self.colors, repeat=num_hands))

        locations = list(product(self.colorings, self.selections))
        initial_state = (random.choice(self.colorings), tuple())
        actions = list(range(num_holes))
        transitions = list()

        for (coloring, selection) in locations:
            for action in actions:
                for new_selection in self.selections:
                    if not new_selection:
                        continue
                    if AdversaryLockGameGraph.ismonochromatic(coloring):
                        transitions.append(((coloring, selection), action, (coloring, new_selection)))
                    else:
                        rotated_coloring = AdversaryLockGameGraph.rotate(coloring, action)
                        for recoloring in self.recolorings:
                            new_coloring = AdversaryLockGameGraph.recolor(rotated_coloring, selection, recoloring)
                            transitions.append(((coloring, selection), action, (new_coloring, new_selection)))

        super().__init__(locations, initial_state, actions, transitions)

        self.player_target = set()
        for i in self.colors:
            for selection in self.selections:
                self.player_target.add(((i for _ in range(num_holes)), selection))

        self.target = set(locations) - self.player_target

    @staticmethod
    def ismonochromatic(coloring):
        for i in range(len(coloring) - 1):
            if coloring[i] != coloring[i + 1]:
                return False
        return True

    @staticmethod
    def rotate(coloring, units):
        iterator = cycle(coloring)
        for _ in range(units):
            next(iterator)
        return tuple(next(iterator) for _ in range(len(coloring)))

    @staticmethod
    def recolor(coloring, selection, recoloring):
        new_coloring = list(coloring)
        for i in range(len(selection)):
            new_coloring[selection[i]] = recoloring[i]
        return tuple(new_coloring)


def main():
    lock_game = AdversaryLockGameGraph(4, 2, 2)
    print("DONE")


# run the main function only if this module is executed as the main script
# (if you import this as a module then nothing is executed)
if __name__ == "__main__":
    # call the main function
    main()
