extends Node


const FULL_HEALTH = 24
const FULL_MOVE = 20


var player_health: float = FULL_HEALTH


var firstMove: bool = true


var camera_rotation_index: int = 3
var session_score: int = 0
var anim_direction: int
var prev_direction: int


var prev_block: String
var playerPosition: Vector3


# Init function
func _ready():
	randomize()


func reset():

	player_health = FULL_HEALTH
	firstMove = true
	camera_rotation_index = 3
	session_score = 0


# Globals.session_score


func random_bool() -> bool:

	# Bigger range for better randomness
	var foo = randi() % 100

	if foo < 50:
		return false
	else:
		return true


# Translate directions to vectors
func direction_calc() -> Vector3:

	var vect = Globals.playerPosition
	
	if anim_direction == 0:
		vect.x += FULL_MOVE

	elif anim_direction == 1:
		vect.z += FULL_MOVE

	elif anim_direction == 2:
		vect.x += -FULL_MOVE

	elif anim_direction == 3:
		vect.z += -FULL_MOVE
		
	return vect


func is_move_legal() -> bool:

	if firstMove:

		firstMove = false
		return true
		
	else:
		# Checking for wrong moves

		if anim_direction == 0:
			return !_check_match("Corner0", "Corner1", "Long1", 2)
			
		elif anim_direction == 1:
			return !_check_match("Corner1", "Corner2", "Long0", 3)
			
		elif anim_direction == 2:
			return !_check_match("Corner2", "Corner3", "Long1", 0)
			
		elif anim_direction == 3:
			return !_check_match("Corner0", "Corner3", "Long0", 1)

		return true


func _check_match(c1, c2, l, dir) -> bool:

	var corners = [c1, c2, l]
		
	# Backtracking check
	for x in range(0, corners.size()):
		if prev_block == corners[x]:
			return true
		
	if prev_direction == dir:
		return true
	else:
		return false


func retranslate_direction(direction) -> int:

	# (Clockwise)
	if camera_rotation_index != 3:
		direction -= (camera_rotation_index + 1)

		# Reverse overflow check
		if direction < 0:
			direction += 4

	return direction


func decorations_calc() -> int:

	var pos: bool = random_bool()
	
	# Add safe margin around the player
	var temp = randi() % 13 + 7
	if !pos:
		temp *= -1

	return temp