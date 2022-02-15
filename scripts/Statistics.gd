extends TextureButton


var _interface

var _stats_text_label: Label
var _stats_opened: bool

# numberOfGames combinedScore correctMoves totalMoves
var _stats_array = []


func _ready():
	_interface = get_node("/root/Level/Interface")
	_stats_text_label = get_node("/root/Level/Interface/Main/Stats/Stats")
	_load()


func upload():
	var games = _stats_array[0] + 1;
	var score = _stats_array[1] + Globals.session_score;
	var correct_moves = _stats_array[2] + Globals.correct_moves;
	var moves = _stats_array[3] + Globals.total_moves;

	var values = [Globals.high_score, games, score, correct_moves, moves]
	Globals.save_stats(Globals.categories_array, values)


func _load():
	for i in 4:
		_stats_array.append(Globals.load_stats(Globals.categories_array[i+1]))
	_update_label()


func _update_label():
	var percentage_float = 0

	if _stats_array[3] != 0:
		percentage_float = _stats_array[2] / _stats_array[3] as float
		percentage_float *= 100

	# Limit float to 2 decimal places
	var percentage_string = stepify(percentage_float, 0.01) as String

	var games = "Number of games: " + str(_stats_array[0]) + "\n"
	var score = "Combined score: " + str(_stats_array[1]) + "\n"
	var percentage = "Correct move percentage: " + percentage_string + "%\n"

	_stats_text_label.text = percentage + games + score


func _on_stats_button_down():
	if _stats_opened:
		_close()
	else:
		_open()


func _open():
	_interface.play_interface_animation("stats_view_show")
	self.texture_normal = Globals.close_texture
	_stats_opened = true


func _close():
	_interface.play_interface_animation("stats_view_hide")
	self.texture_normal = Globals.open_texture
	_stats_opened = false
