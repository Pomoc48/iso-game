extends Node


enum Direction {
	RIGHT_UP,
	RIGHT_DOWN,
	LEFT_DOWN,
	LEFT_UP,
}

enum PlafformDifficulty {
	EASY,
	MEDIUM,
	HARD,
}

var emission_color: Color

var animation_direction: int
var camera_rotation: int = Direction.LEFT_UP
var starting_direction: int
var platform_diff: int
var possible_moves: Array

var FULL_HEALTH = 24
var FIVE_SEC_IN_FRAMES = 300

var player_position: Vector3
var player_can_move: bool = true

var first_move: bool

var player_health: float
var animation_speed: float = 0.25

var session_score: int
var high_score: int

var categories_array = [
	"HighScore",
	"NumberOfGames", 
	"CombinedScore",
	"CorrectMoves",
	"TotalMoves",
]


var _cycles_count: int
var _result: bool

var _config_file = ConfigFile.new()

var _FULL_MOVE = 20;
var _CHANGE_HUE_BY = 0.0016;


func _ready():
	var material_path = "res://materials/emission.tres"
	var material_blue = load(material_path) as StandardMaterial3D

	emission_color = material_blue.emission
	randomize()


func new_game():
	first_move = true
	player_can_move = true

	player_health = FULL_HEALTH
	session_score = 0
	_cycles_count = 0

	animation_speed = 0.25
	platform_diff = PlafformDifficulty.EASY


func save_stats(categories, values):
	for i in categories.size():
		_config_file.set_value("Main", categories[i], values[i])

	_result =_config_file.save("user://config")


func load_stats(category) -> int:
	if _config_file.load("user://config") != OK:
		var values = [0, 0, 0, 0, 0]
		save_stats(categories_array, values)

	return _config_file.get_value("Main", category, 0)


func get_random_bool() -> bool:
	return randi() % 100 < 50


func get_future_position() -> Vector3:
	var position = player_position
	position.y = 0

	if animation_direction == Direction.RIGHT_DOWN:
		position.z += _FULL_MOVE

	elif animation_direction == Direction.LEFT_DOWN:
		position.x += -_FULL_MOVE

	elif animation_direction == Direction.LEFT_UP:
		position.z += -_FULL_MOVE

	else: # animation_direction == Direction.RIGHT_UP
		position.x += _FULL_MOVE

	return position


func get_next_cycle(cycle) -> int:
	_cycles_count = posmod(_cycles_count + 1, 5)
	if _cycles_count > 5 and platform_diff < PlafformDifficulty.HARD:
		platform_diff += 1

	return cycle + randi() % 5


func get_emission_material(offset) -> StandardMaterial3D:
	var hue = StandardMaterial3D.new()
	hue.emission_enabled = true
	hue.emission = emission_color

	hue.emission.h += offset

	# Check cap
	hue.emission.h = fposmod(hue.emission.h, 1)

	return hue


func update_emission_material():
	emission_color.h = fposmod(emission_color.h + _CHANGE_HUE_BY, 1)
