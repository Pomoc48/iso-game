# Game project using the Godot Engine
Simple mobile game with orthogonal camera projection written in C#<br>
*(Name yet to be decided)*

## Gameplay
Each move generates a platform with one or more valid moves. (backtracking moves count as wrong moves)
Once the game has been started, white bar at the top will slowly start to deplete.
Every wrong move lowers the health by a significant amount. Correct moves restore little bit of health.
Every once in a while the in-game camera rotates 90°, 180° or 270° just to confuse the player.

Each correct move has a 1% chance to enter a special mode in which the camera changes it's projection type,
life fully recovers and stops depleting, the game colors turn red and points are counted by 2.

As the game goes on the difficulty will increase:
* Health will deplete faster
* Camera rotations will be more common
* Newly generated platforms with have less exits

**Full depletion of the health bar results in a game over**

## Controls
Player has four possible moves accessible as buttons visible on the screen.

Alternatively, the arrow keys on the keyboard and the gamepad are supported for testing purposes.
Since the game uses diagonal directions to move, two buttons on the keyboard must be pressed simultaneously to indicate the diagonal direction.
For gamepad controls just point the left stick in the diagonal directions.

## Screenshots
<img src="https://github.com/Pomoc48/godot/blob/main/assets/screenshots/game4.png">
<div style="display: flex;">
  <img width="49%" src="https://github.com/Pomoc48/godot/blob/main/assets/screenshots/game6.png">
  <img width="49%" src="https://github.com/Pomoc48/godot/blob/main/assets/screenshots/game2.png">
</div>
<div style="display: flex;">
  <img width="49%" src="https://github.com/Pomoc48/godot/blob/main/assets/screenshots/game3.png">
  <img width="49%" src="https://github.com/Pomoc48/godot/blob/main/assets/screenshots/game5.png">
</div>
