extends Node3D


var _frame_count = 0


func _physics_process(_delta):
	_frame_count += 1

	if _frame_count >= 60:
		_frame_count = 0
		create()


func create():
	_load(_get_position())


func _load(my_position):
	var block = load("res://scenes/Block.tscn") as PackedScene
	var block_instance = block.instantiate() as Node3D
	
	block_instance.position = my_position
	_recolor(block_instance)
	add_child(block_instance)


func _recolor(instance):
	var hue = Globals.get_emission_material(0.05)

	var mesh_body = instance.get_node("MeshInstance3D") as MeshInstance3D
	var particles = instance.get_node("CPUParticles3D") as CPUParticles3D

	particles.mesh.surface_set_material(0, hue)
	mesh_body.set_surface_override_material(0, hue)


func _get_position() -> Vector3:
	var block_position = Vector3()

	if Globals.first_move:
		block_position = Globals.player_position
	else:
		block_position = Globals.get_future_position()

	return _calculate_position(block_position)


func _calculate_position(center_position) -> Vector3:
	var range_x: int
	var range_z: int

	center_position.y = 4

	var random_short = randi() % 8 + 16
	var random_wide = randi() % 24

	if Globals.get_random_bool():
		range_x = random_short
		range_z = random_wide
	else:
		range_x = random_wide
		range_z = random_short

	if Globals.get_random_bool():
		center_position.x += range_x
	else:
		center_position.x -= range_x

	if Globals.get_random_bool():
		center_position.z += range_z
	else:
		center_position.z -= range_z

	return center_position
