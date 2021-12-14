extends Spatial


var level
var healthBar
var scoreText
var particles
var playerTween
var decorationsSpace


var cameraRotation
var buttonsAnimLeft
var buttonsAnimRight
var textAnim
var cameraAnimation
var healthAnimation


const FULL_MOVE = 20
const HISTORY = 4
const DECO_HISTORY = 8
const FULL_HEALTH = 24


var firstMove: bool
var canMove: bool
var playerDead: bool
var cameraRotating: bool
var screenSizeCalculated: bool


var player_health: float
var life_loss_rate_f: float
var life_gain_f: float
var update_health_by: float


var anim_direction: int
var prev_direction: int
var total_platforms: int
var total_deco: int
var session_score: int
var speedup_counter: int
var camera_rotation_index: int
var frames: int


var prev_block: String

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

	playerTween = get_node("Tween")

	particles = get_node("Spatial/Particles")
	cameraAnimation = get_node("Camera/CameraPan")
	cameraRotation = get_node("CameraRotation")
	level = get_node("/root/Level/Platforms")

	var intMain = get_node("/root/Level/Interface/Main")
	healthBar = intMain.get_node("Health")
	healthAnimation = intMain.get_node("Health/HealthAnim")
	buttonsAnimLeft = intMain.get_node("Left/ShowHide")
	buttonsAnimRight = intMain.get_node("Right/ShowHide")

	decorationsSpace = get_node("/root/Level/Decorations")

	scoreText = get_node("/root/Level/Interface/Main/Score")
	textAnim = scoreText.get_node("Bump")

	initial_declarations()

	create_decorations(false)
	create_decorations(true)


func initial_declarations():

	firstMove = true
	canMove = true

	total_platforms = 1
	camera_rotation_index = 3

	life_loss_rate_f = 0.04
	life_gain_f = 2

	player_health = FULL_HEALTH


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


# Connect UI buttons
func _on_Left_button_down():
	touch_controls(3)

func _on_Right_button_down():
	touch_controls(1)

func _on_Down_button_down():
	touch_controls(2)

func _on_Up_button_down():
	touch_controls(0)


# Runs every game tick
func _physics_process(_delta):

	# var fps = Engine.get_frames_per_second()

	# Loose hp after game started
	if !firstMove and !cameraRotating:
		player_health -= life_loss_rate_f
		calculate_health_bar()

	# No life game_over check
	if (player_health <= 0) and !playerDead:
		_game_over()

	if firstMove:
		frames += 1
		
		if frames >= 50:
			frames = 0
			create_decorations(false)


# One time screen size calculation
func get_screen_size():

	var screenSize = get_viewport().get_visible_rect().size.x
	update_health_by = screenSize / FULL_HEALTH
	screenSizeCalculated = true


# Calculate healthbar pixels
func calculate_health_bar():

	if !screenSizeCalculated:
		get_screen_size()

	# Int cast for Vector2
	var health = player_health * update_health_by
	var pos = Vector2(health, 16)

	healthBar.set_size(pos, false)


# Create floating cubes decorations
func create_decorations(duration: bool):

	var blockPos

	# Idle animation position fix
	if firstMove:
		blockPos = self.translation

	else:
		# Future move pos
		blockPos = direction_calc(anim_direction,
		self.translation, FULL_MOVE)

	# Random offset
	blockPos.x += decorations_calc()
	blockPos.z += decorations_calc()

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
		var blockDeco = decorationsSpace.get_child_count()
		print(blockDeco)

		#blockDeco.get_node("AnimationPlayer").play("Hide")
		#blockDeco.get_node("CPUParticles").set_emitting(false)

		yield(get_tree().create_timer(0.5), "timeout")
		#blockDeco.set_visible(false)
		#blockDeco.queue_free()
		

func decorations_calc() -> int:
	
	var pos: bool = random_bool()
	
	# Add safe margin around the player
	var temp = randi() % 13 + 7
	if !pos:
		temp *= -1

	return temp


func correct_score_calculation():

	# Movement animation
	playerTween.interpolate_property(self, "translation", self.translation,
			direction_calc(anim_direction, self.translation, FULL_MOVE), 0.25,
			Tween.TRANS_QUAD, Tween.EASE_IN_OUT)
	playerTween.start()
	
	session_score += 1
	scoreText.set_text(str(session_score))
	textAnim.play("TextAnim")
	
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
	var clockwise: bool = random_bool()
		
	# Camera rotation section
	if clockwise:
		camera_rotation_index += 1

	else:
		camera_rotation_index -= 1

	# camera_rotation_index = 3 -> DEFAULT

	if camera_rotation_index > 3:
		camera_rotation_index = 0

	if camera_rotation_index < 0:
		camera_rotation_index = 3

	# Disable controls for animation duration
	canMove = false
	cameraRotating = true

	# Play correct camera animation
	if clockwise:
		cameraRotation.play("RotationCW" + str(camera_rotation_index))

	else:
		var ccwArray = ["2", "1", "0", "3"]
		cameraRotation.play("RotationCCW" + ccwArray[camera_rotation_index])


# Reenable controls
func _on_CameraRotation_animation_finished(_anim_name):
	canMove = true
	cameraRotating = false


func _on_Tween_tween_all_completed():
	canMove = true


func give_health(ammount: float):

	if (player_health + ammount) > FULL_HEALTH:
		# Health cap check
		player_health = FULL_HEALTH

	else:
		player_health += ammount

				
func check_move(direction: int):
	
	# Calculation based on camera rotation
	anim_direction = retranslate_direction(direction)
	canMove = false
	
	if is_move_legal():
		correct_score_calculation()

	else:
		rebounce_check(direction)
	
	#particles.set_emitting(true)


func rebounce_check(original_dir: int):

	# Wrong move penalty
	player_health -= 8

	# Instant game over
	if player_health <= 0 and !playerDead:
		_game_over()

	else:
		# Animate player rebounce
		cameraRotation.play("Bounce" + str(original_dir))		


func retranslate_direction(dir: int) -> int:

	# (Clockwise)
	if camera_rotation_index != 3:
		dir -= (camera_rotation_index + 1)

		# Reverse overflow check
		if dir < 0:
			dir += 4

	return dir


# Translate directions to vectors
func direction_calc(dir: int, vect: Vector3, ammo: int) -> Vector3:
	
	if dir == 0:
		vect.x += ammo

	elif dir == 1:
		vect.z += ammo

	elif dir == 2:
		vect.x += -ammo

	elif dir == 3:
		vect.z += -ammo
		
	return vect


# Get position for the new platform
func get_future_pos() -> Vector3:
	
	var futurePos = self.translation
	futurePos.y = -16

	return direction_calc(anim_direction, futurePos, FULL_MOVE)


func is_move_legal() -> bool:
	
	if firstMove:

		# First move always legal
		firstMove = false

		# Show healthbar
		healthAnimation.play("HealthDown")

		return true
		
	else:
		# Checking for wrong moves

		if anim_direction == 0:
			return !check_match("Corner0", "Corner1", "Long1", 2)
			
		elif anim_direction == 1:
			return !check_match("Corner1", "Corner2", "Long0", 3)
			
		elif anim_direction == 2:
			return !check_match("Corner2", "Corner3", "Long1", 0)
			
		elif anim_direction == 3:
			return !check_match("Corner0", "Corner3", "Long0", 1)

		return true


func check_match(corner1, corner2, long, direction) -> bool:
	
	var corners = [corner1, corner2, long]
		
	# Backtracking check
	for x in range(0, corners.size()):
		if prev_block == corners[x]:
			return true
		
	if prev_direction == direction:
		return true
	else:
		return false


func random_bool() -> bool:

	# Bigger range for better randomness
	var foo = randi() % 100

	if foo < 50:
		return false
	else:
		return true


func generate_platform():

	var randomNumber = randi() % 3

	# Save for latter backtracking check
	prev_block = pMoves[anim_direction][randomNumber]

	var platform
	var decoratePlatform = random_bool()

	if decoratePlatform:
		platform = load("res://assets/platforms/alt/"+
		prev_block +".tscn")

	else:
		platform = load("res://assets/platforms/plain/"+
		prev_block +".tscn")


	# Load and place new platforms
	var platformI = platform.instance()
	platformI.translation = get_future_pos()
	level.add_child(platformI)
	
	total_platforms += 1
	platformI.get_node("Spatial/AnimationPlayer").play("Up")
	
	prev_direction = anim_direction
	
	# Animate remove old platforms
	if total_platforms >= HISTORY:

		var childIndex = total_platforms - HISTORY
		var child = level.get_child(childIndex)

		child.get_node("Spatial/AnimationPlayer").play("Down")

		# Cleanup
		yield(get_tree().create_timer(0.2), "timeout")
		child.set_visible(false)


func _game_over():
	
	# Preventing movement after death
	playerDead = true
	canMove = false

	# Debug
	print("Final Score:", session_score)

	# Wait for outro anim and restart
	cameraAnimation.play("CameraUp")
	healthAnimation.play("HealthUp")
	textAnim.play("Hide")

	buttonsAnimRight.play("Hide")
	buttonsAnimLeft.play("Hide")


func _on_CameraPan_animation_finished(anim_name):
	if anim_name == "CameraUp":
		# warning-ignore:return_value_discarded
		get_tree().reload_current_scene()
