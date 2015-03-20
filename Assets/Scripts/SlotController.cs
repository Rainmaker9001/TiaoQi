using UnityEngine;
using System.Collections;

public class SlotController : MonoBehaviour {

	public bool hasMarble; //does the slot have a marble?
	public bool isValid; //is the slot a valid move?
	public GameObject board; //the board
	
	void Awake () {
		hasMarble = false;
	}

	//when slot is clicked
	void OnMouseDown() {
		//place marble in slot if slot is valid; if slot invalid, do nothing
		if (isValid) {
			hasMarble = true;
			board.SendMessage("placeInSlot", gameObject);
		}
	}
	
	//light the slot
	void lightslot() {
		if (!hasMarble) {
			renderer.material.color = Color.white;
			isValid = true;
		}
	}
	
	//dim the slot
	void dimslot() {
		renderer.material.color = Color.gray;
		isValid = false;
	}
	
	//toggle hasmarble
	void setMarble() { hasMarble = true;}
	void desetMarble() {hasMarble = false;}

}

