using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerController : MonoBehaviour {

	public String playerName, color; //name of this player
	public GameObject board, nextPlayer, menu; //gameobjects
	public List<Vector3> winCoords; //positions your marbles must be in to win
	public int numWinners = 0; //number of marbles in winning positions
	public Vector3 otherEnd; //end of board you want to reach
	public bool isAI; //is this player an ai
	public int[][] directions; //6 directions of movement
	readonly int slotZ = -1, marbleZ = -2; //slot and marble z coordinates
	readonly int NUMBEST = 10; //max number of moves to consider at each depth

	int movesSinceWin = 0; //for experiments: #moves taken since first guy won


	//do upon awakening
	void Awake() {
		//initialize 6 directions of marble movement
		directions = new int[][]{new int[]{-2,0},
			new int[]{-1, -1}, new int[]{1,-1}, new int[]{2,0},
			new int[]{1,1}, new int[]{-1,1}};
	}


	//freeze/enable players depending on whose turn it is
	public void freezeMarbles() { 
		gameObject.BroadcastMessage("freezeMarble"); 
	}
	public void enableMarbles() { 
		if (!MenuControllerMain.showWinner) { 
			gameObject.BroadcastMessage ("enableMarble"); 
			Properties.currentPlayer = color; //also set current player
			if (isAI)
					makeMove (); //if AI, make move
		}
	}
	
	//set values at beginning (pseudo-constructor)
	void setName(String playerName) { this.playerName = playerName; }
	void setNextPlayer(GameObject thePlayer) { nextPlayer = thePlayer; }
	void setWinCoords(List<Vector3> theWinCoords) { winCoords = theWinCoords; }
	void setOtherEnd(Vector3 otherEnd) { this.otherEnd = otherEnd; }
	void setPlayerColor(string playerColor) { color = playerColor; }
	void setIsAI(string controller) { 
		if (controller.Equals ("AI")) isAI = true;
		else isAI = false;
	}

	//update number of winning marbles, notify Board if this player wins
	void updateNumWinners(Vector3[] positions) {

		//determine if old and new positions are winning positions
		bool oldWin = false, newWin = false;
		for (int i = 0; i < winCoords.Count; i++) {
			if (positions[0].x < (winCoords[i].x + 0.1) && positions[0].y < (winCoords[i].y + 0.1) &&
			    positions[0].x > (winCoords[i].x - 0.1) && positions[0].y > (winCoords[i].y - 0.1))
				oldWin = true;
			if (positions[1].x < (winCoords[i].x + 0.1) && positions[1].y < (winCoords[i].y + 0.1) &&
			    positions[1].x > (winCoords[i].x - 0.1) && positions[1].y > (winCoords[i].y - 0.1))
				newWin = true;
		}

		//update the number of winning marbles
		if (!oldWin && newWin) numWinners++;
		else if (oldWin && !newWin) numWinners--;

		//handle what happens when this player wins
		if (numWinners == winCoords.Count) {

			//when doing timing studies, go back to the main menu
			if (Properties.timing) {
				Application.LoadLevel("MainMenu");
			}

			//when doing experiments, update results and continue playing
			else if (Properties.exping) {
				getResults();
			}

			//when not doing any experiments, just end the game
			else {
				menu.SendMessage ("selectWinner", color);
			}
		}
	}

	//coroutine to write results of experiment
	void getResults() {

		//change properties
		Properties.orderedPlayers.Remove(playerName);
		Properties.orderedControllers.Remove("AI");
		Properties.firstGuyWon = true;
		
		//find prevPlayer
		string prevPlayer = "";
		foreach (KeyValuePair<string, string> entry in Properties.nextPlayers) {
			if (entry.Value == playerName) {
				prevPlayer = entry.Key;
				break;
			}
		}
		
		//change prevPlayer's nextPlayers and nextPlayer to skip over this player
		Properties.nextPlayers[prevPlayer] = Properties.nextPlayers[playerName];
		Properties.nextPlayers.Remove(playerName);
		Properties.players[prevPlayer].GetComponent<PlayerController>().nextPlayer = nextPlayer;
		
		//update records
		string aiType = Properties.playerToAIType[playerName];
		Properties.stratMovesLeft[aiType] += movesSinceWin;
		Properties.stratPlacing[aiType] += Properties.rank;
		Properties.stratRanks [aiType].Add (Properties.rank);
		Properties.stratMovings [aiType].Add (movesSinceWin);
		Properties.rank++;

		
		//if finally over, go to menu
		if (Properties.orderedPlayers.Count == 0) {
			Properties.rank = 1; Properties.firstGuyWon = false; //reset
			MenuControllerMain.showWinner = false;
			Application.LoadLevel ("MainMenu");
		}
	}
	

	//--------Function for Artificial Intelligence---------------------

	//marble: int[] with x coord, y coord
	//move: int[] with x1, y1, x2, y2


	//-----Functions for Finding Next Possible Move-----------------------
	

	//helper function: add direction to marble position
	private int[] addDir(int[] marble, int[] dir) {
		int[] marble1 = new int[2];
		for (int i = 0; i < 2; i++) 
						marble1[i] = marble [i] + dir [i];
		return marble1;
	}

	//helper function: check if position is within board
	private bool inBoard(int[] marble, BoardRep br) {
		if (marble [0] < 0 || marble [1] < 0 || marble [0] >= br.tiles.GetLength (0) ||
						marble [1] >= br.tiles.GetLength (1))
						return false;
				else if (br.tiles [marble [0], marble [1]] == "Invalid")
						return false;
				else
						return true;
	}

	//for use in findJumpMoves: tracks all visited states
	HashSet<int> visited;

	//used in findJumpMoves: gets hashCode for position
	int hashCode(int[] pos) {
		return pos [0] * 1000 + pos [1];
	}

	//given boardrep and player, return all possible next moves
	public List<int[]> findMoves(BoardRep br, string player) {
		List<int[]> moves = new List<int[]>();

		//for each marble
		for (int i = 0; i < br.ptp[player].GetLength(0); i++) {
			
			//add all moves that marble can do
			int[] marble = new int[]{br.ptp[player][i,0], br.ptp[player][i,1]};

			findAdjMoves(marble, br, moves);
			visited = new HashSet<int>();
			findJumpMoves(marble, marble, br, moves);
		}
		
		return moves;
	}
	
	//find all adjacent moves that given marble can go to
	public void findAdjMoves(int[] marble, BoardRep br, List<int[]> moves) {
		
		//for each of the directions
		for (int i = 0; i < directions.Length; i++) {
			int[] marble1 = addDir(marble, directions[i]);

			//if slot present and empty, add it to list
			if (inBoard(marble1, br)) {
				if (br.tiles[marble1[0], marble1[1]] == "Empty") {
					moves.Add(new int[] {marble[0], marble[1],
					marble1[0], marble1[1]});
				}
			}
		}
	}

	//find all moves that given marble can jump to
	public void findJumpMoves (int[] origMarble, int[] marble, BoardRep br, List<int[]> moves) {

		//for each of the directions
		for (int i = 0; i < directions.Length; i++) {
			
			//positions
			int[] adj = addDir(marble, directions[i]);
			int[] jump = addDir(adj, directions[i]);
			
			//if both adjacent AND jump slots are not null
			if (inBoard(adj, br) && inBoard(jump, br)) {

				//AND if 1) adj full 2) jump empty 3) jump is not already lit
				if (br.tiles[adj[0], adj[1]] != "Empty" &&
				    br.tiles[jump[0], jump[1]] == "Empty" &&
				    !visited.Contains(hashCode(jump))) {
					
					//visit slot and recursively add more
					visited.Add(hashCode(jump));
					moves.Add(new int[]{origMarble[0], origMarble[1], jump[0], jump[1]});
					findJumpMoves(origMarble, jump, br, moves);
				}
			}
		}
	}

	//----------------------------------------------------------

	//----------------Testing Functions-----------------------------

	//testing function: print br.tiles
	void printTiles(BoardRep br) {
		for (int i = 0; i < br.tiles.GetLength(0); i++) {
			for (int j = 0; j < br.tiles.GetLength(1); j++) {
				Debug.Log (i + ", " + j + ": " + br.tiles [i, j]);
			}
		}
	}
	
	//testing function: print br.ptp for this player
	void printPtp(BoardRep br) {
		for (int i = 0; i < br.ptp[playerName].GetLength(0); i++) {
			Debug.Log(playerName + " has " + br.ptp[playerName][i, 0] + ", " +
			          br.ptp[playerName][i, 1]);
		}
		Debug.Log ("Printed ptp");
	}

	//---------------------------------------------------------------
	
	//-----------------Functions for Alpha Beta Pruning---------------

	//helper function: find number of moves from marble to end
	int findNumMoves(int[] marble, string player) {

		//compute variables
		int verts = Math.Abs (marble[1]
		                      - Properties.playerToEnd [player] [1]);
		int hors = Math.Abs (marble[0]
		                     - Properties.playerToEnd [player] [0]);
		int addition = (hors - verts) / 2;

		//compute #moves
		if (addition <= 0) return verts;
		else return verts + addition;
	}

	//helper function: score board for one player
	double scorePlayer(BoardRep br, string player) {
		double sum = 0;
		for (int j = 0; j < br.ptp[player].GetLength(0); j++) {
			
			//find #moves for piece to go to end
			int[] marble = {br.ptp[player][j,0], br.ptp[player][j,1]};
			int numMoves = findNumMoves(marble, player);
			
			//subtract numMoves from sum
			sum -= (numMoves * numMoves);
		}
		return sum;
	}

	//score the board: max own min rest
	double allScore(BoardRep br) {
		double sum = 0;
		foreach(KeyValuePair<string, int[,]> entry in br.ptp) {
			double pScore = scorePlayer(br, entry.Key);
			if (entry.Key == playerName) sum += pScore;
			else sum -= pScore;
		}
		return sum;
	}

	//score the board: max own
	double selfScore(BoardRep br) {
		return scorePlayer (br, playerName);
	}

	//score the board: max own min highest
	double highestScore(BoardRep br) {
		double highestScore = -1000000000000;
		foreach(KeyValuePair<string, int[,]> entry in br.ptp) {
			double pScore = scorePlayer(br, entry.Key);
			if (pScore > highestScore && entry.Key != playerName) 
				highestScore = pScore;
		}
		return scorePlayer (br, playerName) - highestScore;
	}

	//score the board: max own min guy above
	double overtakeScore (BoardRep br) {

		//calc highestScore and aboveScore(score of player directly above me)
		double myScore = scorePlayer (br, playerName);
		double aboveScore = 10000000000;
		double highestScore = -1000000000;
		foreach(KeyValuePair<string, int[,]> entry in br.ptp) {
			double pScore = scorePlayer(br, entry.Key);
			if (pScore < aboveScore && pScore > myScore)
				aboveScore = pScore;
			if (pScore > highestScore && entry.Key != playerName) 
				highestScore = pScore;
		}

		//return myScore - aboveScore OR myScore - highestScore
		if (aboveScore > 100000000) 
						return scorePlayer (br, playerName) - highestScore;
				else
						return scorePlayer (br, playerName) - aboveScore;
	}

	//helper function: determines if board is terminal
	bool isTerminal (BoardRep br) {
		Dictionary<string, int[,]> winInds = Properties.winInds;

		//for each player
		foreach(KeyValuePair<string, int[,]> entry in br.ptp) {

			bool allTrue = true;

			//check if corresponding winIndices are filled
			int[,] inds = winInds[entry.Key];
			for (int i = 0; i < inds.GetLength(0); i++) {
				if (br.tiles[inds[i, 0], inds[i, 1]] != entry.Key) allTrue = false;
			}

			if (allTrue) return true;
		}

		return false;
	}


	//helper function: do move on board, modifying the br
	BoardRep newBoard(int[] move, BoardRep br) {
		string player = br.tiles [move [0], move [1]];

		//change tiles
		br.tiles [move [2], move [3]] = player;
		br.tiles [move [0], move [1]] = "Empty";

		//change ptp: find old one, replace
		for (int i = 0; i < br.ptp[player].GetLength(0); i++) {
			if (br.ptp[player][i,0] == move[0] && 
			    br.ptp[player][i,1] == move[1]) {
				br.ptp[player][i,0] = move[2];
				br.ptp[player][i,1] = move[3];
			}
		}

		return br;
	}

	//helper function: reverse move
	int[] revMove(int[] move) {
		int[] rev = new int[4];
		rev [0] = move [2];
		rev [1] = move [3];
		rev [2] = move [0];
		rev [3] = move [1];
		return rev;
	}

	//player for use in compareMoves
	string compareMovesPlayer;

	//function to compare 2 moves
	int compareMoves(int[] m1, int[] m2) {
		int diff1 = findNumMoves(new int[]{m1[2], m1[3]}, compareMovesPlayer) - 
			findNumMoves(new int[]{m1[0], m1[1]}, compareMovesPlayer);
		int diff2 = findNumMoves(new int[]{m2[2], m2[3]}, compareMovesPlayer) - 
			findNumMoves(new int[]{m2[0], m2[1]}, compareMovesPlayer);
		if (diff1 < diff2) return -1;
		else if (diff1 > diff2) return 1;
		else return 0;
	}

	//return top NUMBEST moves
	List<int[]> findBestMoves(BoardRep br, string player) {
		List<int[]> moves = findMoves(br, player);
		compareMovesPlayer = player;
		moves.Sort(compareMoves);
		List<int[]> bestMoves = new List<int[]>();
		for (int i = 0; i < Math.Min(NUMBEST, moves.Count); i++) 
						bestMoves.Add (moves [i]);
		return bestMoves;
	}
		    
	//alpha beta pruning to select best move
	double alphabeta (BoardRep br, int depth, double a, double b, string player) {

		//if at end, return heuristic value of node
		if (depth == 0 || isTerminal(br)) {
			if (Properties.playerToAIType[playerName] == "All") return allScore(br);
			else if (Properties.playerToAIType[playerName] == "Self") return selfScore(br);
			else if (Properties.playerToAIType[playerName] == "Highest") return highestScore(br);
			else return overtakeScore(br);
		}
		    
		//if player is a max player
		if (player == playerName) {
			List<int[]> nextMoves = findBestMoves (br, player);
			foreach (int[] move in nextMoves) {
				//note: also do and undo move
				br = newBoard (move, br);
				a = Math.Max (a, alphabeta (br, depth - 1, a, b, Properties.nextPlayers[player]));
				br = newBoard (revMove (move), br);
				if (b <= a)
						break;
			}
			return a;
		}
        
		//if the player is a min player
		else {
			List<int[]> nextMoves = findBestMoves (br, player);
			foreach (int[] move in nextMoves) {
				//note: also do and undo move
				br = newBoard (move, br);
				b = Math.Min (b, alphabeta (br, depth - 1, a, b, Properties.nextPlayers[player]));
				br = newBoard (revMove (move), br);
				if (b <= a)
					break;
			}
			return b;
		}
	}


	//------------------------------------------------------------------

	//physically move the marble according to MOVE
	public void doMove(int[] move) {

		//turn move into GUI coordinates
		int[] rStart = {move [0], move [1]};
		int[] rEnd = {move [2], move [3]};
		Vector3 bStart = BoardController.getBoardCoords (rStart);
		Vector3 bEnd = BoardController.getBoardCoords (rEnd);
		bEnd.z = slotZ;

		//get appropriate marble and slot
		GameObject marble = (Physics.OverlapSphere (bStart, 0.1f)) [0].gameObject;
		GameObject slot = (Physics.OverlapSphere (bEnd, 0.1f)) [0].gameObject;

		//1) click the marble 2) click the slot
		marble.SendMessage ("OnMouseDown");
		slot.SendMessage ("OnMouseDown");
	}
	

	//move the marble to the position that yields the highest score
	void makeMove() {
		StartCoroutine(makeMoveCoroutine());
	}
	IEnumerator makeMoveCoroutine() {
		//wait for colliders to be updated
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate ();
		yield return new WaitForFixedUpdate ();

		//testing wininds
		Dictionary<string, int[,]> winInds = Properties.winInds;
		foreach (KeyValuePair<string, int[,]> entry in winInds) {
			Debug.Log(entry.Key);
			for (int i = 0; i < entry.Value.GetLength(0); i++) {
				Debug.Log(entry.Value[i, 0] + ", " + entry.Value[i, 1]);
			}
		}


		//for experiment: if first guy has won, count moves
		if (Properties.firstGuyWon)
						movesSinceWin++;

		//get board re
		BoardRep br = BoardController.getBoardRep ();

		//for timing: start timing
		System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch ();
		s.Start ();

		//get moves
		List<int[]> moves = findMoves(br, playerName);

		//for each move, score it and eventually get list of best moves
		List<int[]> bestMoves = new List<int[]> ();
		double bestScore = -10000000000;
		for (int i = 0; i < moves.Count; i++) {
			br = newBoard(moves[i], br);
			double score = alphabeta(br, Properties.playerToDepth[playerName], 
			                         -1000000000, 100000000, Properties.nextPlayers[playerName]);
			if (score > bestScore) {
				bestScore = score;
				bestMoves = new List<int[]>();
				bestMoves.Add(moves[i]);
			}
			else if (score == bestScore) {
				bestMoves.Add(moves[i]);
			}
			br = newBoard(revMove(moves[i]), br);
		}

		//for timing: save elapsed milliseconds
		if (Properties.timing) {
			s.Stop();
			int np = Properties.orderedPlayers.Count;
			Properties.npToTotalTime [np] += ((double)s.ElapsedMilliseconds / 1000);
			Properties.npToNumMoves[np]++;
			//if taking too long, just stop
			if (Properties.npToTotalTime[np] > 100) Application.LoadLevel("MainMenu");
		}

		//choose randomly from best moves
		int bmi = UnityEngine.Random.Range (0, bestMoves.Count);
		doMove (bestMoves[bmi]);
	}
}