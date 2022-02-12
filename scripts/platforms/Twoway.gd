extends Spatial


var _randomSide: int
var _randomSideIndex: int

var _moves = [
	[[1, 3], [0, 1], [0, 3]],
	[[0, 2], [2, 1], [0, 1]],
	[[1, 3], [3, 2], [1, 2]],
	[[2, 0], [0, 3], [3, 2]],
]


func _ready():
	_randomSide = Globals.GetRandomNumber(30)

	if _randomSide < 5:
		_randomSideIndex = 1

	if _randomSide >= 5 and _randomSide < 10:
		_randomSideIndex = 2

	_rotate()
	_update_possible_moves()


func _rotate():
	self.rotation_degrees = _get_rotation()


func _update_possible_moves():
	var valid = []
	var direction = Globals.animationDirection

	for i in 2:
		valid.append(_moves[direction][_randomSideIndex][i])
		Globals.possibleMoves = valid
	

func _get_rotation() -> Vector3:
	var rotationVector = Vector3()

	if Globals.animationDirection == 1:
		rotationVector.y = -90
	
	if Globals.animationDirection == 2:
		rotationVector.y = 180

	if Globals.animationDirection == 3:
		rotationVector.y = 90

	rotationVector.y += _get_override_rotation()

	return rotationVector
	

func _get_override_rotation() -> int:

	if _randomSideIndex == 2:
		return -90

	if _randomSideIndex == 1:
		return 90
	
	return 0
