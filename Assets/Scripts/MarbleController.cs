using UnityEngine;
using System.Collections;

public class MarbleController : MonoBehaviour {
	
	public bool selected; //is the marble selected or not
	public GameObject board; //the board
	public bool canSelect; //can the marble be selected? depends on player turn
	public Color natColor; //the color of the marble
	
	//z indices
	int slotZ = -1, marbleZ = -2;
	
	//initialize
	void Awake () {
		selected = false;
		canSelect = false;
		natColor = renderer.material.color;
	}
	
	//when marble is clicked
	void OnMouseDown() {
		
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
	public void place(GameObject newSlot) {
		StartCoroutine(placeCoroutine(newSlot));
	}
	IEnumerator placeCoroutine(GameObject newSlot) {
		
		//deselect marble
		deselect();
		
		//empty old slot, fill new slot
		Vector3 oldMarblePos = transform.position;
		Vector3 oldSlotPos = oldMarblePos; oldSlotPos.z = slotZ;
		GameObject oldSlot = (Physics.OverlapSphere(oldSlotPos,0.1f))[0].gameObject;
		oldSlot.SendMessage("desetMarble");
		newSlot.SendMessage("setMarble");
		
		//update position
		Vector3 newMarblePos = newSlot.transform.position; newMarblePos.z = marbleZ;
		transform.position = newMarblePos;

		//update number of winning marbles in parent player (for victory condition)
		Vector3[] positions = new Vector3[2];
		positions [0] = oldMarblePos; positions [1] = newMarblePos;
		transform.parent.SendMessage ("updateNumWinners", positions);

		yield return new WaitForFixedUpdate();
	}
	
	//select the marble: nothing to do with placement
	void select() {
		selected = true;
		renderer.material.color = Color.white;
		board.SendMessage("lightSlots", gameObject);
	}
	
	//deselect the marble: nothing to do with placement
	void deselect() {
		selected = false;
		renderer.material.color = natColor;
		board.SendMessage("dimSlots", gameObject);
	}
	
	//toggle canSelect
	void freezeMarble() {canSelect = false;}
	void enableMarble() {canSelect = true;} 

}
