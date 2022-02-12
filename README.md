# Game project using the Godot Engine
Simple mobile game with orthogonal camera projection written in C#

## Gameplay
Each move generates a platform with one or more valid moves.
Once the game has been started, the bar at the top will slowly start to deplete.
Every wrong move lowers the health by a significant amount. Correct moves restore little bit of health.

Full depletion of the health bar results in a game over.

As the game goes on the difficulty will increase:
* Faster depleting health
* More common camera rotations
* Less valid exits on newly generated platforms

## Controls
Player has four possible moves accessible as buttons visible on the screen.

Alternatively, the arrow keys on the keyboard and the gamepad are supported for testing purposes.
Since the game uses diagonal directions to move, two buttons on the keyboard must be pressed simultaneously to indicate the diagonal direction.
For gamepad controls just point the left stick in the diagonal directions.

## Licence
This work is licensed under [Attribution-NonCommercial 4.0 International](https://creativecommons.org/licenses/by-nc/4.0/legalcode). Please see [the licence file](LICENCE.md) for more information.
