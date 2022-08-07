import random
from itertools import combinations

from gamegraphs import *


class AdversaryBinaryLockGameGraph:
    def __init__(self, num_holes, num_hands):
        if num_hands > num_holes:
            raise Exception("Number of hands exceeds number of holes!")

        self.num_holes = num_holes
        self.num_hands = num_hands
        self.colors = [0, 1]
        self.colorings = list(range(2 ** num_holes))
        self.selections = list(combinations(range(num_holes), num_hands))
        self.selections.append(tuple())
        self.recolorings = list(range(2 ** num_hands))

        self.locations = list(product(self.colorings, self.selections))
        self.initial_state = (random.choice(self.colorings), tuple())

        if self.initial_state not in set(self.locations):
            raise Exception("Initial state not in the location space!")

        self.actions = list(range(num_holes))
        self.transitions = set()

        for coloring, selection in self.locations:
            if coloring == 0 or coloring == 2 ** num_holes - 1:
                self.transitions.add(((coloring, selection), 0, (coloring, selection)))
                continue
            for new_selection in self.selections:
                if not new_selection:
                    continue
                else:
                    for action in self.actions:
                        for recoloring in self.recolorings:
                            new_coloring = self.recolor(
                                (2 ** num_holes - 1) & ((coloring << action) | (coloring >> (num_holes - action))),
                                selection, recoloring)
                            self.transitions.add(((coloring, selection), action, (new_coloring, new_selection)))

        self.player_target = set()
        for coloring in [0, 2 ** num_holes - 1]:
            for selection in self.selections:
                self.player_target.add((coloring, selection))

        self.target = set(self.locations) - self.player_target

    def cpre(self, target):
        result = set()
        for location in self.locations:
            for action in self.actions:
                add_location = True
                for new_location in self.locations:
                    if (location, action, new_location) in self.transitions and new_location not in target:
                        add_location = False
                        break
                if add_location:
                    result.add(location)
                    break
        return result

    def compute_winning_set(self):
        W = self.target
        W_next = W & self.cpre(W)
        while W != W_next:
            W = W_next
            W_next = W & self.cpre(W)
        return W

    def can_player_win(self):
        return not self.compute_winning_set()

    @staticmethod
    def recolor(coloring, selection, recoloring):
        new_coloring = coloring
        for i in range(len(selection)):
            if recoloring & (1 << i):
                new_coloring = new_coloring | (1 << selection[i])
            else:
                new_coloring = new_coloring & ~(1 << selection[i])
        return new_coloring


def main():
    print("Starting...")
    lock_game = AdversaryBinaryLockGameGraph(4, 2)
    print(lock_game.can_player_win())


# run the main function only if this module is executed as the main script
# (if you import this as a module then nothing is executed)
if __name__ == "__main__":
    # call the main function
    main()
