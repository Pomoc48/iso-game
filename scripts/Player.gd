extends Spatial


var level
var interface
var particles
var playerTween
var decorationsSpace
var platformsSpace
var cameraRotation
var cameraAnimation


const HISTORY = 4
const DECO_HISTORY = 8


var canMove: bool = true
var playerDead: bool = false
var cameraRotating: bool = false


var life_loss_rate_f: float = 0.04
var life_gain_f: float = 2


var total_platforms: int = 1
var total_deco: int

var speedup_counter: int = 0

var frames: int


# Inputs array
var keys = ["ui_up", "ui_right", "ui_down", "ui_left"]

# Array of possible moves
var pMoves = [
	["Long0", "Corner0", "Corner1"],
	["Long1", "Corner1", "Corner2"],
	["Long0", "Corner2", "Corner3"],
	["Long1", "Corner3", "Corner0"],
]


# Init function
func _ready():
	
	# Randomize RNG
	randomize()

	level = get_node("/root/Level")
	interface = get_node("/root/Level/Interface")

	playerTween = get_node("Tween")

	particles = get_node("Spatial/Particles")
	cameraAnimation = get_node("Camera/CameraPan")
	cameraRotation = get_node("CameraRotation")

	platformsSpace = level.get_node("Platforms")
	decorationsSpace = level.get_node("Decorations")

	create_decorations(false)
	create_decorations(true)


# Debug only
# Runs every frame
func _process(_delta):
	
	# Keyboard input collection
	if canMove:
		for x in range(0,4):
			if Input.is_action_pressed(keys[x]):
				check_move(x)


# Prevent double inputs
func touch_controls(dir: int):
	if canMove:
		check_move(dir)	


# Runs every game tick
func _physics_process(_delta):

	# var fps = Engine.get_frames_per_second()

	# Loose hp after game started
	if !Globals.firstMove and !cameraRotating:

		Globals.player_health -= life_loss_rate_f
		interface.calculate_health_bar()

	# No life game_over check
	if (Globals.player_health <= 0) and !playerDead:
		_game_over()

	if Globals.firstMove:
		frames += 1
		
		if frames >= 50:
			frames = 0
			create_decorations(false)


# Create floating cubes decorations
func create_decorations(duration: bool):

	var blockPos

	# Idle animation position fix
	if Globals.firstMove:
		blockPos = self.translation

	else:
		# Future move pos
		blockPos = level.direction_calc(self.translation)

	# Random offset
	blockPos.x += level.decorations_calc()
	blockPos.z += level.decorations_calc()

	# Always below platforms
	var tempY = randi() % 10
	blockPos.y = -16
	blockPos.y += tempY

	var block = load("res://assets/Block.tscn")
	var blockI = block.instance()
	blockI.translation = blockPos
	
	decorationsSpace.add_child(blockI)

	# Give animation long or short duration
	if duration:
		blockI.get_node("AnimationPlayer").play("ShowLong")
	else:
		blockI.get_node("AnimationPlayer").play("Show")

	total_deco += 1

	# Remove old blocks and disable particles
	if total_deco >= DECO_HISTORY:

		#var decoIndex = total_deco - DECO_HISTORY
		var blockDeco = decorationsSpace.get_child(0)
		#print(decorationsSpace.get_child_count())

		blockDeco.get_node("AnimationPlayer").play("Hide")
		blockDeco.get_node("CPUParticles").set_emitting(false)

		#yield(get_tree().create_timer(0.5), "timeout")
		#blockDeco.set_visible(false)
		blockDeco.free()


func correct_score_calculation():

	# Movement animation
	playerTween.interpolate_property(self, "translation", self.translation,
			level.direction_calc(self.translation), 0.25,
			Tween.TRANS_QUAD, Tween.EASE_IN_OUT)
	playerTween.start()
	
	Globals.session_score += 1
	interface.add_score()
	
	speedup_counter += 1
	
	# Progress the game
	generate_platform()
	give_health(life_gain_f)
	
	create_decorations(false)
	create_decorations(true)
	
	# Slowly increase difficulty
	if speedup_counter >= 10:

		life_loss_rate_f += 0.01
		life_gain_f += 0.25
		speedup_counter = 0

		rotate_camera()
		

func rotate_camera():
	# Get random rotation direction
	var clockwise: bool = level.random_bool()
		
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
		cameraRotation.play("RotationCW" + str(Globals.camera_rotation_index))

	else:
		var ccwArray = ["2", "1", "0", "3"]
		cameraRotation.play("RotationCCW" + ccwArray[Globals.camera_rotation_index])


# Reenable controls
func _on_CameraRotation_animation_finished(_anim_name):
	canMove = true
	cameraRotating = false


func _on_Tween_tween_all_completed():
	canMove = true


func give_health(ammount: float):

	if (Globals.player_health + ammount) > Globals.FULL_HEALTH:
		# Health cap check
		Globals.player_health = Globals.FULL_HEALTH

	else:
		Globals.player_health += ammount

				
func check_move(direction: int):
	
	# Calculation based on camera rotation
	Globals.anim_direction = level.retranslate_direction(direction)
	canMove = false
	
	if level.is_move_legal():
		correct_score_calculation()

	else:
		rebounce_check(direction)
	
	#particles.set_emitting(true)


func rebounce_check(original_dir: int):

	# Wrong move penalty
	Globals.player_health -= 8

	# Instant game over
	if Globals.player_health <= 0 and !playerDead:
		_game_over()

	else:
		# Animate player rebounce
		cameraRotation.play("Bounce" + str(original_dir))		


func generate_platform():

	var randomNumber = randi() % 3

	# Save for latter backtracking check
	Globals.prev_block = pMoves[Globals.anim_direction][randomNumber]

	var platform
	var decoratePlatform = level.random_bool()

	if decoratePlatform:
		platform = load("res://assets/platforms/alt/"+
		Globals.prev_block +".tscn")

	else:
		platform = load("res://assets/platforms/plain/"+
		Globals.prev_block +".tscn")


	# Load and place new platforms
	var platformI = platform.instance()
	platformI.translation = level.get_future_pos(self.translation)
	platformsSpace.add_child(platformI)
	
	total_platforms += 1
	platformI.get_node("Spatial/AnimationPlayer").play("Up")
	
	Globals.prev_direction = Globals.anim_direction
	
	# Animate remove old platforms
	if total_platforms >= HISTORY:

		var childIndex = total_platforms - HISTORY
		var child = platformsSpace.get_child(childIndex)

		child.get_node("Spatial/AnimationPlayer").play("Down")

		# Cleanup
		yield(get_tree().create_timer(0.2), "timeout")
		child.set_visible(false)


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
