from itertools import product


class GameGraph:
    def __init__(self, locations, initial_state, actions, transitions):
        self.locations = locations

        if initial_state not in locations:
            raise Exception("Initial state is not in the location space!")

        self.initial_state = initial_state
        self.state = initial_state
        self.actions = actions

        if not (set(transitions) <= set(product(locations, actions, locations))):
            raise Exception("Transition set is not valid!")

        self.transitions = transitions


class ImperfectInfoGameGraph(GameGraph):
    def __init__(self, locations, initial_state, actions, transitions, observations):
        super().__init__(locations, initial_state, actions, transitions)
        self.observations = observations
