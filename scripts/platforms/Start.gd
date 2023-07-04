extends Node3D


func _ready():
	_rotate()
	_update_possible_moves()


func _rotate():
	var rotationY: int = _generate_starting_pos()
	rotationY *= -90

	var vector = Vector3(0, rotationY, 0)
	self.rotation_degrees = vector


func _generate_starting_pos() -> int:
	Globals.starting_direction = randi() % 4
	return Globals.starting_direction


func _update_possible_moves():
	Globals.possible_moves = [Globals.starting_direction]
