extends Spatial


var level
var healthBar
var scoreText
var particles
var playerTween


var cameraRotation
var buttonsAnimLeft
var buttonsAnimRight
var textAnim
var cameraAnimation
var healthAnimation


const FULL_MOVE = 20
const HISTORY = 4
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
var session_score: int
var speedup_counter: int
var camera_rotation_index: int


var prev_block: String

# Inputs array
var keys = ["ui_up", "ui_right", "ui_down", "ui_left"]

# Array of possible moves
var pMoves = [
	["Long0", "Corner0", "Corner1"],
	["Long1", "Corner1", "Corner2"],
	["Long0", "Corner2", "Corner3"],
	["Long1", "Corner3", "Corner0"]
]


# Init function
func _ready():
	
	# Randomize RNG
	randomize()

	playerTween = get_node("Tween")

	particles = get_node("/root/Level/Player/Spatial/Particles")
	cameraAnimation = get_node("Camera/CameraPan")
	cameraRotation = get_node("CameraRotation")
	level = get_node("/root/Level/Platforms")

	var intMain = get_node("/root/Level/Interface/Main")
	healthBar = intMain.get_node("Health")
	healthAnimation = intMain.get_node("Health/HealthAnim")
	buttonsAnimLeft = intMain.get_node("Left/ShowHide")
	buttonsAnimRight = intMain.get_node("Right/ShowHide")

	scoreText = get_node("/root/Level/Interface/Main/Label")
	textAnim = scoreText.get_node("Bump")
	
	initialVarDeclaration()

	# Intro animation
	cameraAnimation.play("CameraDown")


func initialVarDeclaration():

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
				checkMove(x)


# Prevent double inputs
func touchControls(dir: int):
	if canMove:
		checkMove(dir)	


# Connect UI buttons
func _on_Left_button_down():
	touchControls(3)

func _on_Right_button_down():
	touchControls(1)

func _on_Down_button_down():
	touchControls(2)

func _on_Up_button_down():
	touchControls(0)


# Runs every game tick
func _physics_process(_delta):

	# Loose hp after game started
	if !firstMove && !cameraRotating:
		player_health -= life_loss_rate_f
		calculateHealthBar()

	# No life gameover check
	if (player_health <= 0) && !playerDead:
		gameOver()


# One time screen size calculation
func getScreenSize():

	var screenSize = get_viewport().get_visible_rect().size.x
	update_health_by = screenSize / FULL_HEALTH
	screenSizeCalculated = true


# Calculate healthbar pixels
func calculateHealthBar():

	if !screenSizeCalculated:
		getScreenSize()

	# Int cast for Vector2
	var health = player_health * update_health_by
	var pos = Vector2(health, 16)

	healthBar.set_size(pos, false)


func correctScoreCalculation():

	# Movement animation
	playerTween.interpolate_property(self, "translation", self.translation,
		directionCalc(anim_direction, self.translation, FULL_MOVE), 0.25,
		Tween.TRANS_QUAD, Tween.EASE_IN_OUT)
	playerTween.start()

	session_score += 1
	scoreText.set_text(str(session_score))
	textAnim.play("TextAnim")

	speedup_counter += 1

	# Progress the game
	generatePlatform()
	giveHealth(life_gain_f)

	# Slowly increase difficulty
	if speedup_counter >= 10:

		life_loss_rate_f += 0.01
		life_gain_f += 0.25
		speedup_counter = 0

		# Get random rotation direction
		var clockwise: bool = randomBool()
		
		# Camera rotation section
		if clockwise: camera_rotation_index += 1
		else: camera_rotation_index -= 1

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


func giveHealth(ammount: float):

	if (player_health + ammount) > FULL_HEALTH:
		# Health cap check
		player_health = FULL_HEALTH

	else:
		player_health += ammount

				
func checkMove(direction: int):
	
	# Calculation based on camera rotation
	anim_direction = retranslateDirection(direction)
	
	if isMoveLegal(): correctScoreCalculation()
	else: rebounceCheck(direction)
	
	canMove = false
	particles.set_emitting(true)


func rebounceCheck(original_dir: int):

	# Wrong move penalty
	player_health -= 8

	# Instant game over
	if player_health <= 0 && !playerDead:
		gameOver()

	else:
		# Animate player rebounce
		cameraRotation.play("Bounce" + str(original_dir))
		


func retranslateDirection(dir: int) -> int:

	# (Clockwise)
	if camera_rotation_index != 3:
		dir -= (camera_rotation_index + 1)

		# Reverse overflow check
		if dir < 0:
			dir += 4

	return dir


# Translate directions to vectors
func directionCalc(dir: int, vect: Vector3, ammo: int) -> Vector3:
	
	if dir == 0: vect.x += ammo
	elif dir == 1: vect.z += ammo
	elif dir == 2: vect.x += -ammo
	elif dir == 3: vect.z += -ammo
		
	return vect


# Get position for the new platform
func getFuturePos() -> Vector3:
	
	var futurePos = self.translation
	futurePos.y = -16

	return directionCalc(anim_direction, futurePos, FULL_MOVE)


func isMoveLegal() -> bool:
	
	if firstMove:

		# First move always legal
		firstMove = false

		# Show healthbar
		healthAnimation.play("HealthDown")

		return true
		
	else:
		# Checking for wrong moves

		if anim_direction == 0:
			return !checkMatch("Corner0", "Corner1", "Long1", 2)
			
		elif anim_direction == 1:
			return !checkMatch("Corner1", "Corner2", "Long0", 3)
			
		elif anim_direction == 2:
			return !checkMatch("Corner2", "Corner3", "Long1", 0)
			
		elif anim_direction == 3:
			return !checkMatch("Corner0", "Corner3", "Long0", 1)

		return true


func checkMatch(corner1, corner2, long, direction) -> bool:
	
	var corners = [corner1, corner2, long]
		
	# Backtracking check
	for x in range(0, corners.size()):
		if prev_block == corners[x]:
			return true
		
	if prev_direction == direction: return true
	else: return false


func randomBool() -> bool:

	# Bigger range for better randomness
	var foo = randi() % 100

	if foo < 50:
		return false
	else:
		return true


func generatePlatform():

	var randomNumber = randi() % 3

	# Save for latter backtracking check
	prev_block = pMoves[anim_direction][randomNumber]
	var platform = load("res://assets/platforms/"+ prev_block +".tscn")

	# Load and place new platforms
	var platformI = platform.instance()
	platformI.translation = getFuturePos()
	level.add_child(platformI)
	
	total_platforms += 1
	platformI.get_node("Spatial/AnimationPlayer").play("Up")
	
	prev_direction = anim_direction
	
	# Animate remove old platforms
	if total_platforms >= HISTORY:
		var childIndex = total_platforms - HISTORY
		level.get_child(childIndex).get_node("Spatial/AnimationPlayer").play("Down")


func gameOver():
	
	# Preventing movement after death
	playerDead = true

	# Debug
	print("Final Score:", session_score)

	# Wait for outro anim and restart
	cameraAnimation.play("CameraUp")
	healthAnimation.play("HealthUp")
	textAnim.play("Hide")

	buttonsAnimRight.play("Hide")
	buttonsAnimLeft.play("Hide")

	yield(get_tree().create_timer(1.0), "timeout")
	# warning-ignore:return_value_discarded
	get_tree().reload_current_scene()
