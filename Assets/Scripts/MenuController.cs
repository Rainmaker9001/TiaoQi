using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class MenuController : MonoBehaviour {
	
	//selected players
	private Dictionary<string, bool> selectedPlayers; //is each player selected

	//selected controllers
	private Dictionary<string, int> selectedControllers; //is each player human or ai
	enum ControllerTypes {HUMAN, AI};
	private string[] optionsController = {"Hum","AI"};

	//selected type of AI
	private Dictionary<string, int> selectedAIType; //type of each AI
	enum AITypes {ALL, SELF, HIGHEST, OVERTAKE};
	private string[] optionsAIType = {"All","Self","Highest","Overtake"};

	//indicated player order
	private Dictionary<string, int> selectedOrder;
	private string[] optionsOrder = {"1","2","3","4","5","6"};

	//selected AI depth
	private Dictionary<string, int> selectedDepth;
	private string[] optionsDepth = {"1","2","3","4","5","6","7","8"};

	//player colors: north is red, south is green, etc
	private Dictionary<string, Color> playerColors;
	private Dictionary<string, string> playerColorNames;

	//numRows
	private int selNumRows = 1; //state of numRows radiobuttons
	private string[] optionsNumRows = {"1","2","3","4","5"}; //choices for #rows
	

	void Awake() {
		//set defaults for which players selected
		selectedPlayers = new Dictionary<string,bool>();
		selectedPlayers.Add("N",false); selectedPlayers.Add ("NE", false);
		selectedPlayers.Add("NW",false); selectedPlayers.Add ("S", false);
		selectedPlayers.Add("SE",false); selectedPlayers.Add ("SW", false);

		//set defaults for ai/human
		selectedControllers = new Dictionary<string,int> ();
		selectedControllers.Add ("N", (int)ControllerTypes.HUMAN); selectedControllers.Add ("NE", (int)ControllerTypes.HUMAN);
		selectedControllers.Add ("NW", (int)ControllerTypes.HUMAN); selectedControllers.Add ("S", (int)ControllerTypes.HUMAN);
		selectedControllers.Add ("SE", (int)ControllerTypes.HUMAN); selectedControllers.Add ("SW", (int)ControllerTypes.HUMAN);

		//set defaults for type of AI
		selectedAIType = new Dictionary<string,int> ();
		selectedAIType.Add ("N", (int)AITypes.ALL); selectedAIType.Add ("NE", (int)AITypes.ALL);
		selectedAIType.Add ("NW", (int)AITypes.ALL); selectedAIType.Add ("S", (int)AITypes.ALL);
		selectedAIType.Add ("SE", (int)AITypes.ALL); selectedAIType.Add ("SW", (int)AITypes.ALL);

		//set defaults for player order
		selectedOrder = new Dictionary<string,int> ();
		selectedOrder.Add ("N", 0); selectedOrder.Add ("NE", 0);
		selectedOrder.Add ("NW", 0); selectedOrder.Add ("S", 0);
		selectedOrder.Add ("SE", 0); selectedOrder.Add ("SW", 0);

		//set defaults for ai depth
		selectedDepth = new Dictionary<string,int> ();
		selectedDepth.Add ("N", 5); selectedDepth.Add ("NE", 5);
		selectedDepth.Add ("NW", 5); selectedDepth.Add ("S", 5);
		selectedDepth.Add ("SE", 5); selectedDepth.Add ("SW", 5);

		//set default player colors
		playerColors = new Dictionary<string, Color> ();
		playerColors.Add ("N", Color.red); playerColors.Add ("NE", Color.yellow);
		playerColors.Add("NW", Color.blue); playerColors.Add ("S", Color.green);
		playerColors.Add ("SE", new Color(1f, 0.5f, 0f, 1f)); playerColors.Add ("SW", Color.magenta);
		Properties.playerColors = playerColors;

		//set default player color names
		playerColorNames = new Dictionary<string, string> ();
		playerColorNames.Add ("N", "Red"); playerColorNames.Add ("NE", "Yellow");
		playerColorNames.Add("NW", "Blue"); playerColorNames.Add ("S", "Green");
		playerColorNames.Add ("SE", "Orange"); playerColorNames.Add ("SW", "Magenta");
		Properties.playerColorNames = playerColorNames;


		//for timing experiment
		int N = 1;
		if (Properties.timing) {

			//if first time in experiment, generate combos, initialize
			if (Properties.runs == 0) {
				Properties.combos = Experiments.genTimingCombos(N);

				//initialize items
				Properties.npToTotalTime = new Dictionary<int, double>();
				for (int i = 2; i <= 6; i++) Properties.npToTotalTime.Add(i, 0);
				Properties.npToNumMoves = new Dictionary<int, int>();
				for (int i = 2; i <= 6; i++) Properties.npToNumMoves.Add(i, 0);
			}

			//if combos remain, continue running experiment
			if (Properties.runs < Properties.combos.Count) {
				Experiments.runExperiment (Properties.combos [Properties.runs]);
			}

			//if no more combos, record results
			else {
				StreamWriter sw = new StreamWriter("Timings.txt");

				for (int i = 2; i <= 6; i++) {
					sw.WriteLine(i + ": ");
					sw.WriteLine(Properties.npToTotalTime[i] / (double)Properties.npToNumMoves[i]);
				}

				sw.Close();
			}
		}

		//for experiment
		if (Properties.exping) {
			//if first time in experiment, generate combos, initialize
			if (Properties.runs == 0) {
				Properties.combos = Experiments.genRandCombos (5);
				//Properties.combos = Experiments.genFixedCombos();

				Properties.stratPlacing = new Dictionary<string, int>();
				Properties.stratMovesLeft = new Dictionary<string, int>();
				Properties.stratRanks = new Dictionary<string, List<int>>();
				Properties.stratMovings = new Dictionary<string, List<int>>();

				string[] STRATS = new string[]{"Overtake", "All", "Self", "Highest"};
				foreach (string s in STRATS) {
					Properties.stratPlacing.Add(s, 0);
					Properties.stratMovesLeft.Add(s, 0);
					Properties.stratRanks.Add(s, new List<int>());
					Properties.stratMovings.Add(s, new List<int>());
				}

			}

			//if combos remain, continue running experiment
			if (Properties.runs < Properties.combos.Count) {
				Experiments.runExperiment (Properties.combos [Properties.runs]);
			}

			//if no more combos, record results
			else {
				StreamWriter sw = new StreamWriter("Output.txt");
				
				//record results for placement
				sw.WriteLine("stratPlacing: ");
				foreach(KeyValuePair<string, int> entry in Properties.stratPlacing) {
					sw.WriteLine(entry.Key + " " + (double)entry.Value / 
					             (double)Properties.stratRanks[entry.Key].Count);
				}

				//record results for moves left
				sw.WriteLine("stratMovesLeft: ");
				foreach(KeyValuePair<string, int> entry in Properties.stratMovesLeft) {
					sw.WriteLine(entry.Key + " " + (double)entry.Value / 
					             (double)Properties.stratMovings[entry.Key].Count);
				}

				//record results for ranks
				sw.WriteLine("Ranks");
				foreach(KeyValuePair<string, List<int>> entry in Properties.stratRanks) {
					sw.WriteLine("Key: " + entry.Key);
					foreach (int i in entry.Value) {
						sw.WriteLine(i + " ");
					}
				}

				//record results for movings
				sw.WriteLine("Movings");
				foreach(KeyValuePair<string, List<int>> entry in Properties.stratMovings) {
					sw.WriteLine("Key: " + entry.Key);
					foreach (int i in entry.Value) {
						sw.WriteLine(i + " ");
					}
				}
				
				//Close the file
				sw.Close();
			}
		}
	}

	void OnGUI() {

		//configure number of rows
		selNumRows = GUILayout.SelectionGrid (selNumRows, optionsNumRows, 5, "toggle");
		Properties.numRows = selNumRows + 1;

		//-----#Player Table-----------------------

		//widths
		int playersW = 100;
		int contW = 100;
		int aiW = 300;
		int orderW = 200;
		int depthW = 300;

		//label top row of table
		GUILayout.BeginHorizontal ("box");
		GUILayout.Label ("Selected Players", GUILayout.Width (playersW));
		GUILayout.Label ("Controlled By", GUILayout.Width (contW));
		GUILayout.Label ("AI Type", GUILayout.Width (aiW));
		GUILayout.Label ("Turn Order", GUILayout.Width (orderW));
		GUILayout.Label ("Depth", GUILayout.Width (depthW));
		GUILayout.EndHorizontal ();

		//create table
		List<string> keys = new List<string> (selectedPlayers.Keys); //"N", "NW", etc
		foreach (string key in keys) {
			GUILayout.BeginHorizontal("box");
			selectedPlayers[key] = GUILayout.Toggle(selectedPlayers[key], key+" Player", GUILayout.Width(playersW));
			selectedControllers[key] = GUILayout.SelectionGrid(selectedControllers[key], optionsController, 2, "toggle", GUILayout.Width(contW));
			selectedAIType[key] = GUILayout.SelectionGrid(selectedAIType[key], optionsAIType, 4, "toggle", GUILayout.Width(aiW));
			selectedOrder[key] = GUILayout.SelectionGrid(selectedOrder[key], optionsOrder, 6, "toggle", GUILayout.Width(orderW));
			selectedDepth[key] = GUILayout.SelectionGrid(selectedDepth[key], optionsDepth, 8, "toggle", GUILayout.Width(depthW));
			GUILayout.EndHorizontal();
		}

		//set ordered 1) list of players 2) controllers
		List<string> orderedPlayers = new List<string>();
		List<string> orderedControllers = new List<string> ();
		for (int i = 0; i < 6; i++) {
			foreach (string key in selectedOrder.Keys) {
				//if the player is selected AND has a certain value
				if (selectedPlayers[key] && selectedOrder[key] == i) {
					orderedPlayers.Add(key);
					orderedControllers.Add(optionsController[selectedControllers[key]]);
				}
			}
		}
		Properties.orderedPlayers = orderedPlayers;
		Properties.orderedControllers = orderedControllers;

		//set 1) player to AIType 2) player to depth
		Dictionary<string, string> playerToAIType = new Dictionary<string, string> ();
		playerToAIType.Add ("N", optionsAIType [selectedAIType ["N"]]);
		playerToAIType.Add ("S", optionsAIType [selectedAIType ["S"]]);
		playerToAIType.Add ("NW", optionsAIType [selectedAIType ["NW"]]);
		playerToAIType.Add ("NE", optionsAIType [selectedAIType ["NE"]]);
		playerToAIType.Add ("SW", optionsAIType [selectedAIType ["SW"]]);
		playerToAIType.Add ("SE", optionsAIType [selectedAIType ["SE"]]);
		Properties.playerToAIType = playerToAIType;
		Properties.playerToDepth = selectedDepth;



		//--------------------------------------------

		//-------load MainScene if user has filled necessary fields-------------

		//find number of selected players
		int numPlayers = 0;
		foreach (bool val in selectedPlayers.Values) {
			if (val) numPlayers++;
		}

		//load mainscene if conditions met
		if (GUILayout.Button ("Play Game!") && numPlayers >= 2) {
			Application.LoadLevel ("MainScene");
		}
		//--------------------------------------------------------------------
	}
}