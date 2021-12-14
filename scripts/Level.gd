extends Spatial


var platformsSpace
var decorationsSpace


var history: int = 4
var deco_history: int = 8
var total_deco: int = 0
var total_platforms: int = 1
var frames: int = 0


# Array of possible moves
var pMoves = [
	["Long0", "Corner0", "Corner1"],
	["Long1", "Corner1", "Corner2"],
	["Long0", "Corner2", "Corner3"],
	["Long1", "Corner3", "Corner0"],
]


# Init function
func _ready():

	platformsSpace = get_node("Platforms")
	decorationsSpace = get_node("Decorations")

	create_decorations()


# Runs every game tick
func _physics_process(_delta):

	if Globals.firstMove:
		frames += 1
		
		if frames >= 50:
			frames = 0
			create_decorations()


# Create floating cubes decorations
func create_decorations():

	var blockPos

	# Idle animation position fix
	if Globals.firstMove:
		blockPos = Globals.playerPosition

	else:
		# Future move pos
		blockPos = Globals.direction_calc()

	# Random offset
	blockPos.x += Globals.decorations_calc()
	blockPos.z += Globals.decorations_calc()

	# Always below platforms
	var tempY = randi() % 10
	blockPos.y = -8
	blockPos.y += tempY

	var block = load("res://assets/Block.tscn")
	var blockI = block.instance()
	blockI.translation = blockPos
	
	decorationsSpace.add_child(blockI)

	# # Give animation long or short duration
	# if duration:
	# 	blockI.get_node("AnimationPlayer").play("ShowLong")
	# else:
	# 	blockI.get_node("AnimationPlayer").play("Show")

	blockI.get_node("AnimationPlayer").play("Show")

	total_deco += 1

	# Remove old blocks and disable particles
	if total_deco >= deco_history:

		var decoIndex = total_deco - deco_history
		var blockDeco = decorationsSpace.get_child(decoIndex)
		#print(decorationsSpace.get_child_count())

		blockDeco.get_node("AnimationPlayer").play("Hide")
		blockDeco.get_node("CPUParticles").set_emitting(false)

		yield(get_tree().create_timer(0.25), "timeout")
		# blockDeco.set_visible(false)
		
		blockDeco.queue_free()
		total_deco -= 1

	
func generate_platform():

	var randomNumber = randi() % 3

	# Save for latter backtracking check
	Globals.prev_block = pMoves[Globals.anim_direction][randomNumber]

	var platform
	var decoratePlatform = Globals.random_bool()

	if decoratePlatform:
		platform = load("res://assets/platforms/alt/"+
		Globals.prev_block +".tscn")

	else:
		platform = load("res://assets/platforms/plain/"+
		Globals.prev_block +".tscn")


	# Load and place new platforms
	var platformI = platform.instance()

	platformI.translation = Globals.direction_calc()
	platformI.translation.y = -16

	platformsSpace.add_child(platformI)
	
	total_platforms += 1
	platformI.get_node("Spatial/AnimationPlayer").play("Up")
	
	Globals.prev_direction = Globals.anim_direction
	
	# Animate remove old platforms
	if total_platforms >= history:

		var childIndex = total_platforms - history
		var child = platformsSpace.get_child(childIndex)

		child.get_node("Spatial/AnimationPlayer").play("Down")

		# Cleanup
		yield(get_tree().create_timer(0.2), "timeout")
		# child.set_visible(false)

		child.queue_free()
		total_platforms -= 1
