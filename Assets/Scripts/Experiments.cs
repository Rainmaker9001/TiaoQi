using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Experiments : MonoBehaviour {

	readonly static string[] PLAYERS = new string[]{"N","NW","NE","S","SW","SE"};
	readonly static char[] STRATS = new char[]{'a','h','o','s'};

	//generate instances of all 4p setups (fixed player order)
	public static List<Combo> genFixedCombos() {

		//get permutations
		char[][] perms = new char[24][];
		perms[0] = new char[]{'a','h','o','s'}; perms[1] = new char[]{'a','h','s','o'};
		perms[2] = new char[]{'a','o','h','s'}; perms[3] = new char[]{'a','o','s','h'};
		perms[4] = new char[]{'a','s','h','o'}; perms[5] = new char[]{'a','s','o','h'};
		perms[6] = new char[]{'h','a','o','s'}; perms[7] = new char[]{'h','a','s','o'};
		perms[8] = new char[]{'h','o','a','s'}; perms[9] = new char[]{'h','o','s','a'};
		perms[10] = new char[]{'h','s','a','o'}; perms[11] = new char[]{'h','s','o','a'};
		perms[12] = new char[]{'o','a','h','s'}; perms[13] = new char[]{'o','a','s','h'};
		perms[14] = new char[]{'o','h','a','s'}; perms[15] = new char[]{'o','h','s','a'};
		perms[16] = new char[]{'o','s','a','h'}; perms[17] = new char[]{'o','s','h','a'};
		perms[18] = new char[]{'s','a','h','o'}; perms[19] = new char[]{'s','a','o','h'};
		perms[20] = new char[]{'s','h','a','o'}; perms[21] = new char[]{'s','h','o','a'};
		perms[22] = new char[]{'s','o','a','h'}; perms[23] = new char[]{'s','o','h','a'};

		//for each permutation, make combo out of it
		List<Combo> combos = new List<Combo> ();
		string[] players = new string[]{"N", "S", "NE", "SW"};
		for (int j = 0; j < 1; j++) {
			for (int i = 0; i < 24; i++) {
					Combo combo = new Combo (players, perms [i]);
					combos.Add (combo);
			}
		}

		return combos;
	}

	//helper function: generate random sequence of n players
	static string[] randPlayers(int n) {
		List<string> players = new List<string> ();
		players.Add ("N"); players.Add ("NW"); players.Add ("NE");
		players.Add ("S"); players.Add ("SE"); players.Add ("SW");

		string[] chosen = new string[n];
		for (int i = 0; i < n; i++) {
			int index = UnityEngine.Random.Range (0, n - i);
			chosen[i] = players[index];
			players.Remove(players[index]);
		}

		return chosen;
	}

	//helper function: generate random sequence of n strats
	static char[] randStrats(int n) {
		char[] strats = new char[n];
		for (int i = 0; i < n; i++) {
			strats[i] = STRATS[UnityEngine.Random.Range (0, STRATS.Length)];
		}
		return strats;
	}

	//generate n random combos for each number of players
	public static List<Combo> genRandCombos(int n) {
		List<Combo> combos = new List<Combo> ();
		for (int np = 6; np <= 6; np++) {
			for (int i = 0; i < n; i++) {
				char[] strats = randStrats(np);
				string[] players = randPlayers(np);
				Combo combo = new Combo(players, strats);
				combos.Add(combo);
			}
		}
		return combos;
	}

	//generate combos for timing
	public static List<Combo> genTimingCombos(int n) {
		List<Combo> combos = new List<Combo>();
		for (int np = 3; np <= 4; np++) {
			for (int i = 0; i < n; i++) {
				char[] strats = new char[np];
				for (int j = 0; j < np; j++) strats[j] = 's';
				string[] players = randPlayers(np);
				Combo combo = new Combo(players, strats);
				combos.Add(combo);
			}
		}
		return combos;
	}

	//run experiment on one combo (do this by playing actual game)
	public static void runExperiment(Combo combo) {

		//configure menu settings
		Properties.numRows = 2;
		Properties.orderedPlayers = new List<string>(combo.players);
		Properties.orderedControllers = new List<string> ();
		for (int i = 0; i < Properties.orderedPlayers.Count; i++) {
			Properties.orderedControllers.Add("AI");
		}
		Properties.playerToDepth = new Dictionary<string, int> ();
		for (int i = 0; i < combo.players.Length; i++) {
			Properties.playerToDepth.Add(combo.players[i], 8);
		}
		Properties.playerToAIType = new Dictionary<string, string> ();
		for (int i = 0; i < combo.strats.Length; i++) {
			if (combo.strats[i] == 'a') Properties.playerToAIType.Add(combo.players[i], "All");
			else if (combo.strats[i] == 'h') Properties.playerToAIType.Add(combo.players[i], "Highest");
			else if (combo.strats[i] == 'o') Properties.playerToAIType.Add(combo.players[i], "Overtake");
			else Properties.playerToAIType.Add(combo.players[i], "Self");
		}

		//update number of runs, start game
		Properties.runs = Properties.runs + 1;
		Application.LoadLevel ("MainScene");
	}


	
	
	//after each game, store info. after entire thing, write to file (try c# way, then unityw ay)

	//note: include something that indicates progress

}
