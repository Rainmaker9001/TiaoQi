#pragma strict

var nextPlayer : GameObject; //the next player to move

//freeze marbles when it's another player's turn
function freezeMarbles() {
	gameObject.BroadcastMessage("freezeMarble");
}

//enable marbles when it's this player's turn
function enableMarbles() {
	gameObject.BroadcastMessage("enableMarble");
}

//set nextPlayer
function setNextPlayer(thePlayer : GameObject) {
	nextPlayer = thePlayer;
}