extends Spatial


var _history = 4
var _total = 1

var _chances = [
	[10, 40, 75],
	[5, 35, 60],
	[2, 22, 40]
]


func generate():
	_place(_get_type())
	_total += 1

	if _total >= _history:
		_remove_old()


func _get_type() -> String:
	var difficulty = Globals.platformDifficulty
	var random = Globals.GetRandomNumber(100)
	var types = ["Cross", "TwoWay", "Long", "Corner"]

	for i in 3:
		if random < _chances[difficulty][i]:
			return types[i]

	return types[3]


func _place(type):
	var platform_path = "res://scenes/platforms/"+type+".tscn"

	var platform_block = load(platform_path) as PackedScene
	var block_instance = platform_block.instance() as Spatial

	block_instance.translation = Globals.GetFuturePosition()
	add_child(block_instance)


func _remove_old():
	var child_index = _total - _history
	var child = self.get_child(child_index) as Spatial

	_total -= 1
	child.get_node("Spatial").play_fade_out_animation()
