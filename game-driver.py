import random
from math import sin, cos, pi

import pygame
from pygame.locals import *

# Parameters
N = 4
n = 2
k = 2

screen_dims = (512, 512)

assert 2 <= N <= 12
assert 2 <= n < N
assert 2 <= k <= 8

colors = [(0, 0, 0) for i in range(9)]

colors[0] = BLACK = (0, 0, 0)
colors[1] = WHITE = (255, 255, 255)
colors[2] = RED = (255, 0, 0)
colors[3] = GREEN = (0, 255, 0)
colors[4] = BLUE = (0, 0, 255)
colors[5] = YELLOW = (255, 255, 0)
colors[6] = ORANGE = (255, 128, 0)
colors[7] = PURPLE = (127, 0, 255)
GREY = (128, 128, 128)  # Default color for hidden colors

holes = list()
hole_colors = [random.randrange(k) for _ in range(N)]

ADVERSARY = 0
SELECT = 1
MODIFY = 2
VICTORY = 3


def rotate(a):
    """Performs a cyclic permutation of a units on the holes."""
    for i in range(N):
        hole_colors[i], hole_colors[(i - a) % N] = hole_colors[(i - a) % N], hole_colors[i]


def center(x, y):
    """Transforms the point given by (x,y) so that (0,0) is the center of the screen. Automatically converts floats
    to ints. """
    return tuple((int(x + screen_dims[0] / 2), int(y + screen_dims[1] / 2)))


def isWon():
    """Checks for a monochromatic coloring."""
    victory = True
    for i in range(N - 1):
        if hole_colors[i] != hole_colors[i + 1]:
            victory = False
    return victory


def main():
    """Main game loop."""
    pygame.init()

    logo = pygame.image.load("lock.png")
    pygame.display.set_icon(logo)
    pygame.display.set_caption("Lock Game")

    # Init Screen

    screen = pygame.display.set_mode(screen_dims)
    screen.fill(WHITE)

    for i in range(N):
        pygame.draw.circle(screen, GREY, center(200 * sin(2 * i * pi / N), 200 * cos(2 * i * pi / N)), 50)
        holes.insert(i,
                     pygame.draw.circle(screen, BLACK, center(200 * sin(2 * i * pi / N), 200 * cos(2 * i * pi / N)), 50,
                                        1))

    # Game Loop
    running = True
    phase = SELECT
    selection = [-1 for _ in range(n)]
    selection_count = 0

    while running:
        if phase == ADVERSARY:
            rotate(random.randrange(N))
            phase = SELECT

        for event in pygame.event.get():
            if event.type == KEYDOWN:
                if phase == MODIFY and event.key == K_SPACE:
                    for i in range(N):
                        pygame.draw.circle(screen, GREY, center(200 * sin(2 * i * pi / N), 200 * cos(2 * i * pi / N)),
                                           50)
                        pygame.draw.circle(screen, BLACK, center(200 * sin(2 * i * pi / N), 200 * cos(2 * i * pi / N)),
                                           50, 1)
                    selection = [-1 for _ in range(n)]
                    phase = ADVERSARY

            if event.type == MOUSEBUTTONDOWN:
                for i in range(N):
                    if holes[i].collidepoint(pygame.mouse.get_pos()):
                        if phase == SELECT and i not in selection:
                            selection[selection_count] = i
                            selection_count += 1
                        elif phase == MODIFY:
                            if i in selection:
                                hole_colors[i] = (hole_colors[i] + 1) % k

            if event.type == pygame.QUIT:
                running = False

        if phase == SELECT:
            for hole in range(selection_count):
                pygame.draw.circle(screen, colors[hole_colors[selection[hole]]],
                                   center(200 * sin(2 * selection[hole] * pi / N),
                                          200 * cos(2 * selection[hole] * pi / N)), 50)
                pygame.draw.circle(screen, BLACK, center(200 * sin(2 * selection[hole] * pi / N),
                                                         200 * cos(2 * selection[hole] * pi / N)), 50, 1)

        if selection_count >= len(selection):
            selection_count = 0
            phase = MODIFY

        if phase == MODIFY:
            for i in selection:
                pygame.draw.circle(screen, colors[hole_colors[i]],
                                   center(200 * sin(2 * i * pi / N), 200 * cos(2 * i * pi / N)), 50)
                pygame.draw.circle(screen, BLACK, center(200 * sin(2 * i * pi / N), 200 * cos(2 * i * pi / N)), 50, 1)

        if phase == VICTORY:
            screen.fill((51, 255, 255))
            for i in range(N):
                pygame.draw.circle(screen, colors[hole_colors[i]],
                                   center(200 * sin(2 * i * pi / N), 200 * cos(2 * i * pi / N)), 50)
                pygame.draw.circle(screen, BLACK, center(200 * sin(2 * i * pi / N), 200 * cos(2 * i * pi / N)), 50, 1)

        if isWon():
            phase = VICTORY

        pygame.display.update()
        pygame.time.Clock().tick(30)


# run the main function only if this module is executed as the main script
# (if you import this as a module then nothing is executed)
if __name__ == "__main__":
    # call the main function
    main()
