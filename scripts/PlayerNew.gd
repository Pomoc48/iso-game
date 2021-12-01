extends Spatial


var cameraAnimation
var level
var platformI
var platform
var playerLight


const SPEED = 2
const FULL_ANIM = 10
const FULL_MOVE = 20
const HISTORY = 4


var firstMove: bool
var canMove: bool
var isAnimating: bool
var playerDead: bool


var life_loss_rate_f: float
var life_gain_f: float


var anim_progress: int
var anim_direction: int
var prev_direction: int
var total_platforms: int
var session_score: int
var speedup_counter: int


var prev_block: String


var keys = ["ui_up", "ui_right", "ui_down", "ui_left"]

var pMoves = [
	["Long0", "Corner0", "Corner1"],
	["Long1", "Corner1", "Corner2"],
	["Long0", "Corner2", "Corner3"],
	["Long1", "Corner3", "Corner0"]
]


func _ready():
	
	randomize()

	cameraAnimation = get_node("Camera/AnimationPlayer")
	level = get_node("/root/Level/Platforms")
	playerLight = get_node("PlayerLight")
	
	firstMove = true
	session_score = 0
	speedup_counter = 0
	total_platforms = 1

	life_loss_rate_f = 0.04
	life_gain_f = 2
	playerLight.omni_range = 24

	startStopAnim(0, false)
	cameraAnimation.play("CameraDown")


func _process(_delta):
	
	if canMove:
		for x in range(0,4):
			if Input.is_action_pressed(keys[x]):
				startStopAnim(x, true)


func _physics_process(_delta):

	if !firstMove:
		playerLight.omni_range -= life_loss_rate_f
	
	if isAnimating:
		
		anim_progress += 1
		
		if anim_progress <= FULL_ANIM:
			playerMove(anim_direction)
			
		else: startStopAnim(0, false)

	if playerLight.omni_range <= 0:
		gameOver()


func correctScoreCalculation():

	session_score += 1
	speedup_counter += 1

	if speedup_counter >= 10:

		life_loss_rate_f += 0.01
		life_gain_f += 0.25
		speedup_counter = 0

	generatePlatform()
	giveHealth(life_gain_f)


func startStopAnim(direction: int, start: bool):
	
	if start:
		anim_direction = direction
		
		if isMoveLegal():
			correctScoreCalculation()

		else: gameOver()
		
		isAnimating = true
		canMove = false
		anim_progress = 0
		
	else:

		if !playerDead:

			isAnimating = false
			canMove = true


func playerMove(direction: int):
	
	canMove = false
	var newCalc = directionCalc(direction, self.translation, SPEED)
	self.translation = newCalc


func getFuturePos():
	
	var futurePos = self.translation
	futurePos.y = -16

	return directionCalc(anim_direction, futurePos, FULL_MOVE)


func directionCalc(dir, vect, ammo):
	
	if dir == 0: vect.x += ammo
	elif dir == 1: vect.z += ammo
	elif dir == 2: vect.x += -ammo
	elif dir == 3:  vect.z += -ammo
		
	return vect


func isMoveLegal():
	
	if firstMove:
		
		firstMove = false
		return true
		
	else:
		if anim_direction == 0:
			if checkMatch("Corner0", "Corner1", "Long1", 2):
				return false
			
		elif anim_direction == 1:
			if checkMatch("Corner1", "Corner2", "Long0", 3):
				return false
			
		elif anim_direction == 2:
			if checkMatch("Corner2", "Corner3", "Long1", 0):
				return false
			
		elif anim_direction == 3:
			if checkMatch("Corner0", "Corner3", "Long0", 1):
				return false

		return true


func checkMatch(corner1, corner2, long, direction):
	
	var corners = [corner1, corner2, long]
		
	for x in range(0, corners.size()):
		if prev_block == corners[x]:
			return true
		
	if prev_direction == direction: return true
	else: return false


func generatePlatform():
	
	var randomNumber = randi() % 3
	prev_block = pMoves[anim_direction][randomNumber]
	
	platform = load("res://assets/platforms/"+ prev_block +".tscn")

	platformI = platform.instance()
	platformI.translation = getFuturePos()
	level.add_child(platformI)
	
	total_platforms += 1
	platformI.get_node("Spatial/AnimationPlayer").play("Up")
	
	prev_direction = anim_direction
	
	if total_platforms >= HISTORY:
		var childIndex = total_platforms - HISTORY
		level.get_child(childIndex).get_node("Spatial/AnimationPlayer").play("Down")


func gameOver():
	
	playerDead = true

	print("Game Over!")
	print("Final Score:", session_score)
	cameraAnimation.play("CameraUp")
	
	yield(get_tree().create_timer(1.0), "timeout")
	# warning-ignore:return_value_discarded
	get_tree().reload_current_scene()


func giveHealth(ammount: float):

	if (playerLight.omni_range + ammount) > 24:
		playerLight.omni_range = 24

	else:
		playerLight.omni_range += ammount
