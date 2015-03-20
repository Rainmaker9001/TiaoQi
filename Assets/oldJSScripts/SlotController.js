#pragma strict

var hasMarble : boolean; //does the slot have a marble?
var isValid : boolean; //is the slot a valid move?
var board : GameObject; //the board

function Awake () {
	hasMarble = false;
}

//when slot is clicked
function OnMouseDown() {
	//place marble in slot if slot is valid; if slot invalid, do nothing
	if (isValid) {
		hasMarble = true;
		board.SendMessage("placeInSlot", gameObject);
	}
}

//light the slot
function lightslot() {
	if (!hasMarble) {
		renderer.material.color = Color.green;
		isValid = true;
	}
}

//dim the slot
function dimslot() {
	renderer.material.color = Color.gray;
	isValid = false;
}

//toggle hasmarble
function setMarble() { hasMarble = true;}
function desetMarble() {hasMarble = false;}