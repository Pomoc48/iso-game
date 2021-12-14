extends Spatial


var level
var interface
var playerTween


var cameraRotation
var cameraAnimation


var canMove: bool = false
var playerDead: bool = false
var cameraRotating: bool = false


var life_loss_rate_f: float = 0.04
var life_gain_f: float = 2
var speedup_counter: int = 0


# Inputs array
var keys = ["ui_up", "ui_right", "ui_down", "ui_left"]


# Init function
func _ready():
	
	# Randomize RNG
	randomize()

	level = get_node("/root/Level")
	interface = get_node("/root/Level/Interface")

	playerTween = get_node("Tween")

	cameraAnimation = get_node("Camera/CameraPan")
	cameraRotation = get_node("CameraRotation")

	Globals.reset()
	Globals.playerPosition = self.translation


# # Debug only holding multiple buttons cause bugs
# # Runs every frame
# func _process(_delta):
	
# 	# Keyboard input collection
# 	if canMove:
# 		for x in range(0,4):
# 			if Input.is_action_pressed(keys[x]):
# 				_check_move(x)


# Prevent double inputs
func touch_controls(dir: int):
	if canMove:
		_check_move(dir)	


# Runs every game tick
func _physics_process(_delta):

	# Loose hp after game started
	if !Globals.firstMove and !cameraRotating:

		Globals.player_health -= life_loss_rate_f
		interface.calculate_health_bar()

	# No life game_over check
	if (Globals.player_health <= 0) and !playerDead:
		_game_over()

	interface.update_fps(Engine.get_frames_per_second())


func _correct_score_calculation():

	# Movement animation
	playerTween.interpolate_property(self, "translation", self.translation,
			Globals.direction_calc(), 0.25, Tween.TRANS_QUAD, Tween.EASE_IN_OUT)
	playerTween.start()
	
	Globals.session_score += 1
	interface.add_score()
	
	speedup_counter += 1
	
	# Progress the game
	level.generate_platform()
	_give_health(life_gain_f)
	
	level.create_decorations()
	
	# Slowly increase difficulty
	if speedup_counter >= 10:

		life_loss_rate_f += 0.01
		life_gain_f += 0.25
		speedup_counter = 0

		rotate_camera()
		

func rotate_camera():
	# Get random rotation direction
	var clockwise: bool = Globals.random_bool()
		
	# Camera rotation section
	if clockwise:
		Globals.camera_rotation_index += 1

	else:
		Globals.camera_rotation_index -= 1

	# camera_rotation_index = 3 -> DEFAULT

	if Globals.camera_rotation_index > 3:
		Globals.camera_rotation_index = 0

	if Globals.camera_rotation_index < 0:
		Globals.camera_rotation_index = 3

	# Disable controls for animation duration
	canMove = false
	cameraRotating = true

	# Play correct camera animation
	if clockwise:
		cameraRotation.play("RotationCW" +
				str(Globals.camera_rotation_index))

	else:
		var ccwArray = ["2", "1", "0", "3"]
		cameraRotation.play("RotationCCW" +
				ccwArray[Globals.camera_rotation_index])


# Reenable controls
func _on_CameraRotation_animation_finished(_anim_name):
	canMove = true
	cameraRotating = false


func _on_Tween_tween_all_completed():

	# Update global position at the end of animation
	Globals.playerPosition = self.translation

	# Small bug fix
	if !cameraRotating:
		canMove = true


func _give_health(ammount: float):

	if (Globals.player_health + ammount) > Globals.FULL_HEALTH:
		# Health cap check
		Globals.player_health = Globals.FULL_HEALTH

	else:
		Globals.player_health += ammount

				
func _check_move(dir: int):
	
	# Calculation based on camera rotation
	Globals.anim_direction = Globals.retranslate_direction(dir)
	canMove = false
	
	if Globals.is_move_legal():
		_correct_score_calculation()

	else:
		_rebounce_check(dir)


func _rebounce_check(original_dir: int):

	# Wrong move penalty
	Globals.player_health -= 8

	# Instant game over
	if Globals.player_health <= 0 and !playerDead:
		_game_over()

	else:
		# Animate player rebounce
		cameraRotation.play("Bounce" + str(original_dir))		


func _game_over():
	
	# Preventing movement after death
	playerDead = true
	canMove = false

	# Debug
	print("Final Score:", Globals.session_score)

	# Wait for outro anim and restart
	cameraAnimation.play("CameraUp")
	interface.hide_ui_animation()


func _on_CameraPan_animation_finished(anim_name):

	if anim_name == "CameraUp":
		# warning-ignore:return_value_discarded
		get_tree().reload_current_scene()

	else:
		canMove = true
