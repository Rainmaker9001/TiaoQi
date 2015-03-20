using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardController : MonoBehaviour {

	//gameobjects
	public GameObject selectedMarble; //the currently selected marble
	public GameObject slotPrefab, marblePrefab, playerPrefab; //objects to create
	public Camera cam; //the camera
	public Dictionary<string, GameObject> players; //the players

	//z indices
	public int slotZ, marbleZ, boardZ, cameraZ; 
	
	//6 directions
	public static Vector3[] directions;
	
	//dimensions
	public static float w, h;
	
	//called by menu, initializes board
	void Awake() {

		//initialize values
		slotZ = -1; marbleZ = -2; boardZ = 0; cameraZ = -10;
		w = 3.464f; h = 3f;
		float theScale = 3f*w*Properties.numRows+1;
		directions = new Vector3[6];
		directions[0] = new Vector3(-1.0f*w/2.0f, h, 0); directions[1] = new Vector3(w/2.0f, h, 0); 
		directions[2] = new Vector3(w, 0, 0); directions[3] = new Vector3(w/2.0f, -1.0f*h, 0); 
		directions[4] = new Vector3(-1.0f*w/2.0f, -1.0f*h, 0); directions[5] = new Vector3(-1.0f * w, 0, 0);
		
		//resize board and camera
		gameObject.transform.localScale = new Vector3(theScale*1.2f, theScale*1.2f, 1);
		cam.orthographic = true;
		cam.orthographicSize = theScale/1.5f;

		//initialize playerToEnd
		Dictionary<string, int[]> dict = new Dictionary<string, int[]> ();
		int n = Properties.numRows;
		dict.Add ("S", new int[] {3 * n, 0}); dict.Add ("N", new int[] {3 * n, 4 * n});
		dict.Add ("SE", new int[] {0, n}); dict.Add ("SW", new int[] {6 * n, n});
		dict.Add ("NE", new int[] {0, 3 * n}); dict.Add ("NW", new int[] {6 * n, 3 * n});
		Properties.playerToEnd = dict;

		//initialize slots
		initStar();
		
		//initialize/configure marbles/players
		initPlayers();

		//initialize player to winning indices lookup
		Properties.winInds = new Dictionary<string, int[,]> ();
		int num = Properties.numRows;
		int[,] nIndices = new int[num * (num + 1) / 2, 2];
		int[,] sIndices = new int[num * (num + 1) / 2, 2];
		int[,] nwIndices = new int[num * (num + 1) / 2, 2];
		int[,] swIndices = new int[num * (num + 1) / 2, 2];
		int[,] neIndices = new int[num * (num + 1) / 2, 2];
		int[,] seIndices = new int[num * (num + 1) / 2, 2];
		int count = 0;
		for (int i = 0; i < num; i++) {
			for (int j = 0; j <= i; j++) {
				nIndices[count, 0] = 3 * n - i + 2 * j;
				nIndices[count, 1] = i;
				sIndices[count, 0] = 3 * n - i + 2 * j;
				sIndices[count, 1] = 4 * n - i;
				nwIndices[count, 0] = i + j;
				nwIndices[count, 1] = n + i - j;
				swIndices[count, 0] = i + j;
				swIndices[count, 1] = 3 * n - i + j;
				neIndices[count, 0] = 6 * n - i - j;
				neIndices[count, 1] = n + i - j;
				seIndices[count, 0] = 6 * n - i - j;
				seIndices[count, 1] = 3 * n - i + j;
				count = count + 1;
			}
		}
		Properties.winInds.Add ("N", sIndices);
		Properties.winInds.Add ("S", nIndices);
		Properties.winInds.Add ("NW", seIndices);
		Properties.winInds.Add ("SW", neIndices);
		Properties.winInds.Add ("NE", swIndices);
		Properties.winInds.Add ("SE", nwIndices);
	}

	//initialize marbles corresponding to PLAYER
	//TIP: location of board tip; HUP and WRIGHT: normalized, then mult by width or height
	void initPlayerMarbles(GameObject player, Color pColor, Vector3 hUp, Vector3 wRight, Vector3 tip) {
		
		//local variables
		int n = Properties.numRows;
		List<Vector3> winCoords = new List<Vector3> (); //when the player's marbles reach these
											            //coordinates, he wins

		//loop to set marbles
		for (int i = 0; i < n; i++) {
			Vector3 startPos = tip - i * hUp - i * wRight/2.0f;
			for (int j = 0; j <= i; j++) {

				//get positions
				Vector3 marblePos = startPos + j * wRight; marblePos.z = marbleZ;
				Vector3 slotPos = marblePos; slotPos.z = slotZ;
				Vector3 winMarblePos = marblePos * -1; winMarblePos.z = marbleZ;
				winCoords.Add(winMarblePos);

				//initialize/set marble
				marblePrefab.renderer.material.color = pColor;
				GameObject marbleClone = (GameObject)Instantiate(marblePrefab, marblePos, Quaternion.identity);
				GameObject theSlot = (Physics.OverlapSphere(slotPos,0.1f))[0].gameObject;
				theSlot.SendMessage("setMarble");
				marbleClone.transform.parent = player.transform;
			}
		}

		player.SendMessage ("setWinCoords", winCoords);
		player.SendMessage ("setOtherEnd", tip * -1);
	}
	
	//create and configure players
	void initPlayers() {

		int n = Properties.numRows;
		
		//create players
		players = new Dictionary<string, GameObject> ();
		for (int i = 0; i < Properties.orderedPlayers.Count; i++) {
			string playerName = Properties.orderedPlayers[i];

			players.Add(playerName, (GameObject)Instantiate(playerPrefab));
			players[playerName].SendMessage("setPlayerColor", Properties.playerColorNames[playerName]);
			players[playerName].SendMessage("setIsAI", Properties.orderedControllers[i]);
			players[playerName].SendMessage("setName", playerName);
		}
		Properties.players = players;

		//initialize marbles, setting parent of each
		foreach (string playerName in Properties.orderedPlayers) {
			if (playerName == "N") {
				Vector3 normUp = new Vector3(0,1,0);
				initPlayerMarbles(players["N"], Properties.playerColors["N"], normUp*h, new Vector3(1, 0, 0)*w, normUp*2*h*n);
			}
			else if (playerName == "NW") {
				Vector3 normUp = new Vector3(-0.8660f, 0.5f, 0);
				initPlayerMarbles(players["NW"], Properties.playerColors["NW"], normUp*h, new Vector3(0.5f, 0.8660f,0)*w, normUp*2*h*n);
			}
			else if (playerName == "NE") {
				Vector3 normUp = new Vector3(0.8660f, 0.5f, 0);
				initPlayerMarbles(players["NE"], Properties.playerColors["NE"], normUp*h , new Vector3(0.5f, -0.8660f,0)*w, normUp*2*h*n);
			}
			else if (playerName == "S") {
				Vector3 normUp = new Vector3(0, -1, 0);
				initPlayerMarbles(players["S"], Properties.playerColors["S"], normUp*h, new Vector3(-1, 0, 0)*w, normUp*2*h*n);
			}
			else if (playerName == "SE") {
				Vector3 normUp = new Vector3(0.8660f, -0.5f, 0);
				initPlayerMarbles(players["SE"], Properties.playerColors["SE"], normUp*h, new Vector3(-0.5f, -0.8660f,0)*w, normUp*2*h*n);
			}
			else if (playerName == "SW") {
				Vector3 normUp = new Vector3(-0.8660f, -0.5f, 0);
				initPlayerMarbles(players["SW"], Properties.playerColors["SW"], normUp*h, new Vector3(0.5f, -0.8660f,0)*w, normUp*2*h*n);
			}
		}
		
		//configure player order
		for (int i = 0; i < players.Count; i++) {
			players[Properties.orderedPlayers[i]].SendMessage("setNextPlayer",
			            players[Properties.orderedPlayers[(i+1)%players.Count]]);
		}
		Properties.nextPlayers = new Dictionary<string, string>();
		for (int i = 0; i < Properties.orderedPlayers.Count; i++) {
			int next = (i + 1) % Properties.orderedPlayers.Count;
			Properties.nextPlayers.Add(Properties.orderedPlayers[i], Properties.orderedPlayers[next]);
		}

		//initially, unfreeze first player
		players [Properties.orderedPlayers[0]].SendMessage ("enableMarbles");
	}
	
	//create star-shaped slots on board in which each player has n lines
	void initStar() {
		int n = Properties.numRows;

		//local variables
		float startCol; float row; float col; //updated vars
		float topRow = 2 * n * h; float midCol = 0; //fixed positions
		GameObject slotClone; //newly created slot

		//create first and last n rows
		for (int i = 0; i < n; i++) {
			startCol = midCol - i * w/2.0f;
			row = topRow - i * h;
			for (int j = 0; j <= i; j++) {
				col = startCol + j * w;
				
				//initialize top and bottom slots
				slotClone = (GameObject)Instantiate(slotPrefab, new Vector3(col, row, slotZ), Quaternion.identity);
				slotClone.transform.parent = transform;
				slotClone = (GameObject)Instantiate(slotPrefab, new Vector3(col, row * -1, slotZ), Quaternion.identity);
				slotClone.transform.parent = transform;
			}
		}
		
		//create next n rows (both top and bottom)
		for (int i = 0; i < n; i++) {
			startCol = midCol - n * w * 1.5f + i * w/2.0f;
			row = topRow - (n + i) * h;
			for (int j = 0; j < (3*n + 1 - i); j++) {
				col = startCol + j * w;
				slotClone = (GameObject)Instantiate(slotPrefab, new Vector3(col, row, slotZ), Quaternion.identity);
				slotClone.transform.parent = transform;
				slotClone = (GameObject)Instantiate(slotPrefab, new Vector3(col, row * -1, slotZ), Quaternion.identity);
				slotClone.transform.parent = transform;
			}
		}
		
		//create middle row
		startCol = midCol - n * w;
		for (int i = 0; i < (n * 2 + 1); i++) {
			col = startCol + i * w;
			slotClone = (GameObject)Instantiate(slotPrefab, new Vector3(col, 0, slotZ), Quaternion.identity);
			slotClone.transform.parent = transform;
		} 
	}
	
	//--------------------Light or Dim Slots-------------------------
	
	//dim all the slots
	void dimSlots(GameObject theMarble) {
		selectedMarble = null;
		gameObject.BroadcastMessage("dimslot");
	}
	
	//light the slots that the marble can go to
	void lightSlots(GameObject theMarble) {
		Vector3 slotPosition;
		GameObject theSlot;
		
		//obtain slot position
		selectedMarble = theMarble;
		slotPosition = theMarble.transform.position + new Vector3(0, 0, slotZ - marbleZ);
		theSlot = (Physics.OverlapSphere(slotPosition,0.1f))[0].gameObject;
		
		//light 3 types of slots
		theSlot.SendMessage("lightslot");
		lightAdjacentSlots (slotPosition);
		List<GameObject> jumpSlots = new List<GameObject> ();
		lightJumpSlots(slotPosition, jumpSlots);
	}
	
	//light adjacent slots
	public static List<GameObject> lightAdjacentSlots(Vector3 slotPosition) {
		List<GameObject> adjacentSlots = new List<GameObject> ();
		
		//for each of the directions
		for (int i = 0; i < directions.Length; i++) {

			//if slot present and not full
			Collider[] adjacentSlotColls = Physics.OverlapSphere(slotPosition+directions[i],0.1f);
			if (adjacentSlotColls.Length != 0) {
				SlotController sc = adjacentSlotColls[0].GetComponent<SlotController>();
				if (sc.hasMarble == false) {

					//add it to list and light it
					adjacentSlots.Add(adjacentSlotColls[0].gameObject);
					adjacentSlotColls[0].gameObject.SendMessage("lightslot");
				}
			}
		}

		return adjacentSlots;
	}
	
	//recursively light slots that you can jump to
	public static void lightJumpSlots(Vector3 slotPosition, List<GameObject> jumpSlots) {
		
		//for each of the directions
		for (int i = 0; i < directions.Length; i++) {
			
			//positions
			Vector3 adj = slotPosition + directions[i];
			Vector3 jump = adj + directions[i];
			
			//if both adjacent AND jump slots are not null
			Collider[] adjacentSlotColls = Physics.OverlapSphere(adj,0.1f);
			Collider[] jumpSlotColls = Physics.OverlapSphere(jump,0.1f);
			if (adjacentSlotColls.Length != 0 && jumpSlotColls.Length != 0) {
				
				//AND if 1) adj full 2) jump empty 3) jump is not already lit
				SlotController adjSlotController = adjacentSlotColls[0].GetComponent<SlotController>();
				SlotController jumpSlotController = jumpSlotColls[0].GetComponent<SlotController>();
				if (adjSlotController.hasMarble && !jumpSlotController.hasMarble &&
				    !jumpSlotController.isValid) {
					
					//slot lighting logic
					jumpSlots.Add(jumpSlotColls[0].gameObject);
					jumpSlotColls[0].gameObject.SendMessage("lightslot");
					lightJumpSlots(jump, jumpSlots);
				}
			}
		}
	}

	
	//-------State Control Methods------------------------------------------

	//place selected Marble in slot, handle freezing/unfreezing
	void placeInSlot(GameObject slot) {
		if (selectedMarble != null) {

			PlayerController pc = selectedMarble.transform.parent.GetComponent<PlayerController>();
			PlayerController nextPc = pc.nextPlayer.GetComponent<PlayerController>();
			MarbleController mc = selectedMarble.GetComponent<MarbleController>();

			pc.freezeMarbles(); //freeze previous player
			mc.place(slot); //place marble

			nextPc.enableMarbles(); //enable next player
		}
	}
	
	//deselect selectedMarble
	void deselectSelected() {
		if (selectedMarble != null) selectedMarble.SendMessage("deselect");
	}



	//-------Methods to get board representation (for AI)--------------

	//helper function: given Vector3, convert to "boardrep coordinates"
	public static int[] getRepCoords(Vector3 coords) {
		int n = Properties.numRows;
		int x = (int)System.Math.Round(coords.x / w * 2.0f) + 3 * n;
		int y = 2 * n - (int)System.Math.Round(coords.y / h);
		int[] repCoords = {x, y};
		return repCoords;
	}

	//helper function: given "boardrep coordinates", convert to Vector3
	public static Vector3 getBoardCoords(int[] coords) {
		int n = Properties.numRows;
		float x = coords [0] - 3 * n;
		float y = coords [1] - 2 * n;
		x = x * w / 2.0f;
		y = y * h * -1;
		return new Vector3 (x, y, -2);
	}

	//return array representation of board
	public static BoardRep getBoardRep () {

		//----Get board array first---------------------

		//array dimensions
		int numRows = 4 * Properties.numRows + 1;
		int numCols = 6 * Properties.numRows + 1;

		//calculate the northwest position of the board
		float topY = 2.0f * Properties.numRows * h;
		float leftX = -3.0f / 2.0f * Properties.numRows * w;

		//initialize string array representation of board
		string[,] boardArray = new string[numRows, numCols];
		float curX = leftX; float curY = topY;
		for (int i = 0; i < numRows; i++) {
			curX = leftX;
			for (int j = 0; j < numCols; j++) {

				//check if slot and marble are there
				Collider[] slots = Physics.OverlapSphere(new Vector3(curX, curY, Properties.slotZ),0.1f);
				Collider[] marbles = Physics.OverlapSphere(new Vector3(curX, curY, Properties.marbleZ), 0.1f);
				bool isSlot = (slots.Length != 0);
				bool isMarble = (marbles.Length != 0);

				//initialize array based on presence of slot/marble
				if (!isSlot) boardArray[i,j] = "Invalid";
				else if (isSlot && !isMarble) boardArray[i,j] = "Empty";
				else {
					PlayerController pc = marbles[0].transform.parent.GetComponent<PlayerController>();
					boardArray[i,j] = pc.playerName;
				}


				curX += (w/2.0f);
			}
			curY -= h;
		}

		//------Get players to pieces lookup table--------

		Dictionary<string, int[,]> playerToPieces = new Dictionary<string, int[,]> ();

		//for each player
		foreach(KeyValuePair<string, GameObject> entry in Properties.players) {
			int n = Properties.numRows;
			int[,] piecePositions = new int[(1 + n) * n / 2, 2];

			//for each child marble of the player
			int i = 0;
			foreach (Transform child in entry.Value.transform) {
				//get the rep coordinates of that marble
				int[] repCoords = getRepCoords(child.position);
				piecePositions[i, 0] = repCoords[0];
				piecePositions[i, 1] = repCoords[1];
				i = i + 1;
			}

			//set key, value in playerToPieces
			playerToPieces.Add(entry.Key, piecePositions);
		}


		//----Return overall data structure-------------------

		//made a mistake: now must transpose array
		string[,] transArray = new string[numCols, numRows];
		for (int j = 0; j < numCols; j++)
			for (int r = 0; r < numRows; r++)
				transArray[j, r] = boardArray[r, j];

		return new BoardRep (transArray, playerToPieces);
	}
}