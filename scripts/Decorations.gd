extends Spatial


func create():
	_load(_get_position())


func _load(position):
	var block = load("res://scenes/Block.tscn") as PackedScene
	var block_instance = block.instance() as Spatial
	
	block_instance.translation = position
	_recolor(block_instance)
	add_child(block_instance)


func _recolor(instance):
	var hue = Globals.GetEmissionMaterial(0.05)

	var mesh_body = instance.get_node("MeshInstance") as MeshInstance
	var particles = instance.get_node("CPUParticles") as CPUParticles

	particles.mesh.surface_set_material(0, hue)
	mesh_body.set_surface_material(0, hue)


func _get_position() -> Vector3:
	var block_position = Vector3()

	if Globals.firstMove:
		block_position = Globals.playerPosition
	else:
		block_position = Globals.GetFuturePosition()

	return _calculate_position(block_position)


func _calculate_position(center_position) -> Vector3:
	var range_x: int
	var range_z: int

	center_position.y = 4

	var random_short = Globals.GetRandomNumber(16, 24)
	var random_wide = Globals.GetRandomNumber(24)

	if Globals.GetRandomBool():
		range_x = random_short
		range_z = random_wide
	else:
		range_x = random_wide
		range_z = random_short

	if Globals.GetRandomBool():
		center_position.x += range_x
	else:
		center_position.x -= range_x

	if Globals.GetRandomBool():
		center_position.z += range_z
	else:
		center_position.z -= range_z

	return center_position
