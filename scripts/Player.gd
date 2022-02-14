extends Spatial


var _tween: Tween
var _animation: AnimationPlayer

var _bounce_particles: CPUParticles
var _game_over_particles: CPUParticles
var _body_particles: CPUParticles
var _body_particles2: CPUParticles

var _result: bool


func _ready():
	_tween = get_node("Tween")
	_animation = get_node("SpatialAnim")

	_bounce_particles = get_node("Spatial/Bounce")
	_game_over_particles = get_node("Spatial/GameOver")
	_body_particles = get_node("Spatial/BodyP")
	_body_particles2 = get_node("Spatial/BodyCenterP")


func animate_movement():
	var position = Globals.GetFuturePosition()
	position.y = 2

	var speed = Globals.animationSpeed
	_play_tween_animation("translation", position, speed)


func update_color():
	var hue_small = Globals.GetEmissionMaterial(0.025)
	var hue_big = Globals.GetEmissionMaterial(0.25)

	_body_particles.mesh.surface_set_material(0, hue_small)

	_game_over_particles.mesh.surface_set_material(0, hue_big)
	_bounce_particles.mesh.surface_set_material(0, hue_big)
	_body_particles2.mesh.surface_set_material(0, hue_big)


func play_spatial_animation(animation):
	if _animation.is_playing():
		if _is_game_over_animation(animation):
			_play_game_over_animation(animation)
	else:
		_animation.play(animation)


func rotate_camera_by(rotations):
	var clockwise = Globals.GetRandomBool()

	if clockwise:
		Globals.cameraRotation += rotations
	else:
		Globals.cameraRotation -= rotations

	if Globals.cameraRotation > 3:
		Globals.cameraRotation -= 4

	if Globals.cameraRotation < 0:
		Globals.cameraRotation += 4

	Globals.canPlayerMove = false
	_play_camera_rotation_animation(clockwise, rotations)


func _play_game_over_animation(animation):
	_animation.stop()
	# Fix particles rarely emitting non stop after game over
	_bounce_particles.emitting = false
	_animation.play(animation)


func _play_camera_rotation_animation(clockwise, rotations):
	var rotate_by = 90 * rotations
	# More rotations take longer
	var time = rotations * Globals.animationSpeed * 1.5

	var new_rotation = self.rotation_degrees

	if clockwise:
		new_rotation.y += rotate_by
	else:
		new_rotation.y -= rotate_by

	_play_tween_animation("rotation_degrees", new_rotation, time)


func _play_tween_animation(type, new_vector, time):
	_result = _tween.interpolate_property(self, type, null,
			new_vector, time, Tween.TRANS_SINE)
	_result = _tween.start()


func _is_game_over_animation(animation) -> bool:
	return animation == "camera_up" or animation == "camera_up_long"


# Reenable controls
func _on_tween_animation_finished():
	# Update global position at the end of animation
	Globals.playerPosition = self.translation
	Globals.canPlayerMove = true


func _on_spatial_animation_finished(animation):
	if _is_game_over_animation(animation):
		_result = get_tree().reload_current_scene()
	else:
		Globals.canPlayerMove = true
