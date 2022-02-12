extends Spatial


var _moves = [
	[3, 0, 1],
	[1, 0, 2],
	[3, 1, 2],
	[3, 0, 2],
]


func _ready():
	_update_possible_moves()


func _update_possible_moves():
	var valid = []
	var direction = Globals.animationDirection

	for i in 3:
		valid.append(_moves[direction][i])
		Globals.possibleMoves = valid
