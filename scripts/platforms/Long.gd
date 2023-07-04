extends Node3D


func _ready():
	_rotate()
	_update_possible_moves()


func _rotate():
	if Globals.animation_direction % 2 != 0:
		var rotation = Vector3(0, 90, 0)
		self.rotation_degrees = rotation


func _update_possible_moves():
	Globals.possible_moves = [Globals.animation_direction]
