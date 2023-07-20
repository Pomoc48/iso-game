extends Node3D

var _bounce_particles: CPUParticles3D
var _game_over_particles: CPUParticles3D
var _body_particles: CPUParticles3D
var _body_particles2: CPUParticles3D

var _bounce_player: AnimationPlayer


func _ready():
	_bounce_player = get_node("BouncePlayer")
	
	_bounce_particles = get_node("Node3D/Bounce")
	_game_over_particles = get_node("Node3D/GameOver")
	_body_particles = get_node("Node3D/BodyP")
	_body_particles2 = get_node("Node3D/BodyCenterP")


func animate_movement():
	Globals.player_position = self.position
	
	var my_position = Globals.get_future_position()
	my_position.y = 2

	var speed = Globals.animation_speed
	
	var tween = self.create_tween()
	tween.set_trans(Tween.TRANS_SINE)
	tween.tween_property(self, "position", my_position, speed)
	tween.tween_callback(func(): Globals.player_can_move = true)


func update_color():
	var hue_small = Globals.get_emission_material(0.025)
	var hue_big = Globals.get_emission_material(0.25)

	_body_particles.mesh.surface_set_material(0, hue_small)

	_game_over_particles.mesh.surface_set_material(0, hue_big)
	_bounce_particles.mesh.surface_set_material(0, hue_big)
	_body_particles2.mesh.surface_set_material(0, hue_big)


func play_spatial_animation(animation):
	_bounce_player.play(animation)


func rotate_camera_by(rotations):
	var clockwise = Globals.get_random_bool()

	if clockwise:
		Globals.camera_rotation += rotations
	else:
		Globals.camera_rotation -= rotations

	Globals.camera_rotation = posmod(Globals.camera_rotation, 4)

	_play_camera_rotation_animation(clockwise, rotations)


# func _play_game_over_animation(animation):
# 	_animation.stop()
# 	# Fix particles rarely emitting non stop after game over
# 	_bounce_particles.emitting = false
# 	_animation.play(animation)


func _play_camera_rotation_animation(clockwise, rotations):
	var rotate_by = 90 * rotations
	# More rotations take longer
	var time = rotations * Globals.animation_speed * 1.5

	var new_rotation = self.rotation_degrees

	if clockwise:
		new_rotation.y += rotate_by
	else:
		new_rotation.y -= rotate_by
		
	var tween = self.create_tween()
	
	tween.set_trans(Tween.TRANS_SINE)
	tween.tween_property(self, "rotation_degrees", new_rotation, time)
	tween.tween_callback(func(): Globals.player_can_move = true)


# func _is_game_over_animation(animation) -> bool:
# 	return animation == "camera_up" or animation == "camera_up_long"


func _on_bounce_player_animation_finished(_anim_name):
	Globals.player_can_move = true
