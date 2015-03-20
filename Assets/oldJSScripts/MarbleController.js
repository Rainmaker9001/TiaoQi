#pragma strict

var selected: boolean; //is the marble selected or not
var board: GameObject; //the board
var canSelect : boolean; //can the marble be selected? depends on player turn

//z indices
var slotZ : int= -1; 
var marbleZ : int = -2;

//initialize
function Awake () {
	selected = false;
	canSelect = false;
}

//when marble is clicked
function OnMouseDown() {

	//only if marble enabled
	if (canSelect) {
		//deselect this marble
		if (selected) deselect();
		
		//deselect previously selected marble, select this marble
		else {
			board.SendMessage("deselectSelected");
			select();
		}
	}
}

//place the marble
function place(newSlot : GameObject) {
	var oldSlot : GameObject;
	var oldSlotPos : Vector3;
	
	//deselect marble
	deselect();
	
	//empty old slot, fill new slot
	oldSlotPos = transform.position; oldSlotPos.z = slotZ;
	oldSlot = (Physics.OverlapSphere(oldSlotPos,0.1))[0].gameObject;
	oldSlot.SendMessage("desetMarble");
	newSlot.SendMessage("setMarble");
	
	//update position
	transform.position = newSlot.transform.position;
	transform.position.z = marbleZ;
}

//select the marble: nothing to do with placement
function select() {
	selected = true;
	renderer.material.color = Color.red;
	board.SendMessage("lightSlots", gameObject);
}

//deselect the marble: nothing to do with placement
function deselect() {
	selected = false;
	renderer.material.color = Color.blue;
	board.SendMessage("dimSlots", gameObject);
}

//toggle canSelect
function freezeMarble() {canSelect = false;}
function enableMarble() {canSelect = true;} 
