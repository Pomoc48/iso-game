extends Node3D


var _reverse: bool
var _moves = [[1, 3], [2, 0], [3, 1], [0, 2]]


func _ready():
	_reverse = Globals.get_random_bool()

	_rotate()
	_update_possible_moves()


func _rotate():
	self.rotation_degrees = _get_rotation()


func _update_possible_moves():
	var valid = _moves[Globals.animation_direction][_reverse as int]
	Globals.possible_moves = [valid]


func _get_rotation() -> Vector3:
	var rotationVector = Vector3()

	if Globals.animation_direction == 1:
		rotationVector.y = -90
	
	if Globals.animation_direction == 2:
		rotationVector.y = 180

	if Globals.animation_direction == 3:
		rotationVector.y = 90

	if _reverse:
		rotationVector.y -= 90

	return rotationVector
