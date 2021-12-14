extends Spatial


# Init function
func _ready():
	randomize()


func random_bool() -> bool:

	# Bigger range for better randomness
	var foo = randi() % 100

	if foo < 50:
		return false
	else:
		return true


# Translate directions to vectors
func direction_calc(vect) -> Vector3:
	
	if Globals.anim_direction == 0:
		vect.x += Globals.FULL_MOVE

	elif Globals.anim_direction == 1:
		vect.z += Globals.FULL_MOVE

	elif Globals.anim_direction == 2:
		vect.x += -Globals.FULL_MOVE

	elif Globals.anim_direction == 3:
		vect.z += -Globals.FULL_MOVE
		
	return vect


# Get position for the new platform
func get_future_pos(position) -> Vector3:
	
	var futurePos = position
	futurePos.y = -16

	return direction_calc(futurePos)


func is_move_legal() -> bool:

	if Globals.firstMove:

		Globals.firstMove = false
		return true
		
	else:
		# Checking for wrong moves

		if Globals.anim_direction == 0:
			return !_check_match("Corner0", "Corner1", "Long1", 2)
			
		elif Globals.anim_direction == 1:
			return !_check_match("Corner1", "Corner2", "Long0", 3)
			
		elif Globals.anim_direction == 2:
			return !_check_match("Corner2", "Corner3", "Long1", 0)
			
		elif Globals.anim_direction == 3:
			return !_check_match("Corner0", "Corner3", "Long0", 1)

		return true


func _check_match(c1, c2, l, dir) -> bool:

	var corners = [c1, c2, l]
		
	# Backtracking check
	for x in range(0, corners.size()):
		if Globals.prev_block == corners[x]:
			return true
		
	if Globals.prev_direction == dir:
		return true
	else:
		return false


func retranslate_direction(direction) -> int:

	# (Clockwise)
	if Globals.camera_rotation_index != 3:
		direction -= (Globals.camera_rotation_index + 1)

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