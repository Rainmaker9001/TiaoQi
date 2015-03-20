using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Properties {

	//for timing experiments
	public static bool timing = false;
	public static Dictionary<int, double> npToTotalTime;
	public static Dictionary<int, int> npToNumMoves;

	//for experiments
	public static bool exping = false;
	public static Dictionary<string, int> stratPlacing;
	public static Dictionary<string, int> stratMovesLeft;
	public static Dictionary<string, List<int>> stratRanks;
	public static Dictionary<string, List<int>> stratMovings;
	public static bool firstGuyWon = false; //has the first guy won yet
	public static int rank = 1; //rank of player when finish

	//for both experiments
	public static int runs = 0;
	public static List<Combo> combos;

	//set by player using menu
	public static int numRows = 4;
	public static List<string> orderedPlayers;
	public static List<string> orderedControllers;
	public static Dictionary<string, string> playerToAIType;
	public static Dictionary<string, int> playerToDepth;

	//player to nextplayer lookup table
	public static Dictionary<string, string> nextPlayers;

	//the players in the game
	public static Dictionary<string, GameObject> players;

	//lookup table: player to winning position
	public static Dictionary<string, int[]> playerToEnd;
	public static Dictionary<string, int[,]> winInds;

	//player colors - unchangeable
	public static Dictionary<string, Color> playerColors;
	public static Dictionary<string, string> playerColorNames;

	//properties that change during the game
	public static string currentPlayer; 

	//z positions
	public static int slotZ = -1, marbleZ = -2;
}
