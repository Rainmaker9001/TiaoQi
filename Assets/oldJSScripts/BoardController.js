#pragma strict

//gameobjects
var selectedMarble : GameObject; //the currently selected marble
var slotPrefab : GameObject; //slots to create
var marblePrefab : GameObject; //marbles to create
var playerPrefab : GameObject; //players to create
var cam : Camera; //the camera
var players : GameObject[]; //the players

//z indices
var slotZ : int;
var marbleZ : int;
var boardZ : int; 
var cameraZ : int; 

//6 directions
var directions : Vector3[];

//dimensions
var w : float; var h : float;

//initialize slots and marbles on board
function Awake () {

	//initialize z-indices
	slotZ = -1; marbleZ = -2; boardZ = 0; cameraZ = -10;
	
	//NOTE: for testing: later, delete
	initBoard(2, 2);
}

//called by menu, initializes board
function initBoard(rows : int, numPlayers : int) {
	var i : int;
	
	//initialize dimensions
	w = 3.464; h = 3;
	var theScale : float = 3*w*rows+1;
	
	//initialize 6 directions
	directions = new Array(6);
	directions[0] = Vector3(-1.0*w/2.0, h, 0); directions[1] = Vector3(w/2.0, h, 0); 
	directions[2] = Vector3(w, 0, 0); directions[3] = Vector3(w/2.0, -1.0*h, 0); 
	directions[4] = Vector3(-1.0*w/2.0, -1.0*h, 0); directions[5] = Vector3(-1.0 * w, 0, 0);
	
	//resize board and camera
	gameObject.transform.localScale = Vector3(theScale*1.2, theScale*1.2, 1);
	cam.orthographic = true;
	cam.orthographicSize = theScale/1.5;
	
	//initialize slots and marbles
	initStar(rows);
	
	//initialize and configure players
	initPlayers(numPlayers, [1, 2, 3]);
}

//create and configure players
function initPlayers(numPlayers : int, playerPositions : int[]) {
	var i : int;

	//create players
	players = new Array(numPlayers);
	for (i = 0; i < numPlayers; i++) {
		players[i] = Instantiate(playerPrefab);
	}
	
	//-----Set Marble Parents --------------------
	
	//set first two players' marbles: top and bottom
	/*marbleClone = Instantiate(marblePrefab, Vector3(col, row, marbleZ), Quaternion.identity);
	slotClone.SendMessage("setMarble");
	//marbleClone.transform.parent = players[0].transform;
			
	marbleClone = Instantiate(marblePrefab, Vector3(col, row * -1, marbleZ), Quaternion.identity);
	slotClone.SendMessage("setMarble");*/
	//marbleClone.transform.parent = players[1].transform;
	
	//--------------------------------------------
	
	//configure player order
	players[0].SendMessage("enableMarbles");
	for (i = 0; i < numPlayers; i++) {
		players[i].SendMessage("setNextPlayer",players[(i+1)%numPlayers]);
	}
}

//create star-shaped slots on board in which each player has n lines
function initStar(n : int) {

	//local variables
	var i : int; var j : int; //counters
	var startCol : float; var row : float; var col : float; //updated vars
	var topRow : float; var botRow : float; var midCol : float = 0; //fixed positions
	var slotClone : GameObject; var marbleClone : GameObject; //newly created objects
	
	//toprow and botrow
	topRow = 2 * n * h;
	botRow = topRow * -1;
	
	
	//create first and last n rows
	for (i = 0; i < n; i++) {
		startCol = midCol - i * w/2.0;
		row = topRow - i * h;
		for (j = 0; j <= i; j++) {
			col = startCol + j * w;
			
			//initialize top and bottom slots
			slotClone = Instantiate(slotPrefab, Vector3(col, row, slotZ), Quaternion.identity);
			slotClone.transform.parent = transform;
			slotClone = Instantiate(slotPrefab, Vector3(col, row * -1, slotZ), Quaternion.identity);
			slotClone.transform.parent = transform;
		}
	}
	
	//create next n rows (both top and bottom)
	for (i = 0; i < n; i++) {
		startCol = midCol - n * w * 1.5 + i * w/2.0;
		row = topRow - (n + i) * h;
		for (j = 0; j < (3*n + 1 - i); j++) {
			col = startCol + j * w;
			slotClone = Instantiate(slotPrefab, Vector3(col, row, slotZ), Quaternion.identity);
			slotClone.transform.parent = transform;
			slotClone = Instantiate(slotPrefab, Vector3(col, row * -1, slotZ), Quaternion.identity);
			slotClone.transform.parent = transform;
		}
	}
	
	//create middle row
	startCol = midCol - n * w;
	for (i = 0; i < (n * 2 + 1); i++) {
		col = startCol + i * w;
		slotClone = Instantiate(slotPrefab, Vector3(col, 0, slotZ), Quaternion.identity);
		slotClone.transform.parent = transform;
	} 
}

//--------------------Light or Dim Slots-------------------------

//dim all the slots
function dimSlots(theMarble : GameObject) {
	selectedMarble = null;
	gameObject.BroadcastMessage("dimslot");
}

//light the slots that the marble can go to
function lightSlots(theMarble : GameObject) {
	var slotPosition : Vector3;
	var theSlot : GameObject;
	
	//obtain slot position
	selectedMarble = theMarble;
	slotPosition = theMarble.transform.position + Vector3(0, 0, slotZ - marbleZ);
	theSlot = (Physics.OverlapSphere(slotPosition,0.1))[0].gameObject;
	
	//light 3 types of slots
	theSlot.SendMessage("lightslot");
	lightAdjacentSlots(slotPosition);
	lightJumpSlots(slotPosition, slotPosition);
}

//light adjacent slots
function lightAdjacentSlots(slotPosition : Vector3) {
	var adjacentSlotColls : Collider[];
	var i : int;
	var hasMarble : boolean;
	
	//for each of the directions
	for (i = 0; i < directions.length; i++) {
		adjacentSlotColls = Physics.OverlapSphere(slotPosition+directions[i],0.1);
		//if slot present, try to light it
		if (adjacentSlotColls.length != 0) 
			adjacentSlotColls[0].gameObject.SendMessage("lightslot");
	}
}

//recursively light slots that you can jump to
function lightJumpSlots(slotPosition : Vector3, prevSlotPosition : Vector3) {
	var adjSlotController : SlotController; var jumpSlotController : SlotController;
	var adjacentSlotColls : Collider[]; var jumpSlotColls : Collider[]; var slotColls : Collider[];
	var i : int;
	var adj : Vector3; var jump : Vector3;
	
	
	//for each of the directions
	for (i = 0; i < directions.length; i++) {
	
		//positions
		adj = slotPosition + directions[i];
		jump = adj + directions[i];
	
		//if both adjacent AND jump slots are not null
		slotColls = Physics.OverlapSphere(slotPosition, 0.1);
		adjacentSlotColls = Physics.OverlapSphere(adj,0.1);
		jumpSlotColls = Physics.OverlapSphere(jump,0.1);
		if (adjacentSlotColls.length != 0 && jumpSlotColls.length != 0) {
			
			//AND if 1) adj full 2) jump empty 3) jump is not already lit
			adjSlotController = adjacentSlotColls[0].GetComponent("SlotController");
			jumpSlotController = jumpSlotColls[0].GetComponent("SlotController");
			if (adjSlotController.hasMarble && !jumpSlotController.hasMarble &&
				!jumpSlotController.isValid) {
				
				//slot lighting logic
				jumpSlotColls[0].gameObject.SendMessage("lightslot");
				lightJumpSlots(jump, slotPosition);
			}
		}
	}
}


//-------Setter Methods------------------------------------------

//place selected Marble in slot, handle freezing/unfreezing
function placeInSlot(slot : GameObject) {
	if (selectedMarble != null) {
		
		//freeze player who just moved, enable another player
		selectedMarble.transform.parent.SendMessage("freezeMarbles");
		var pc : PlayerController = selectedMarble.transform.parent.GetComponent("PlayerController");
		pc.nextPlayer.SendMessage("enableMarbles");
		
		//place the marble
		selectedMarble.SendMessage("place", slot);
	}
}

//deselect selectedMarble
function deselectSelected() {
	if (selectedMarble != null) selectedMarble.SendMessage("deselect");
}