using UnityEngine;
using System.Collections;

public class MenuControllerMain : MonoBehaviour {

	public static bool showWinner = false; //has a winner been chosen?
	public string winner;

	//set winner
	void selectWinner(string winner) { 
		showWinner = true;
		this.winner = winner;
	}

	//gui for menu
	void OnGUI() {

		//show whose turn it is
		GUILayout.Label ("Current Player: " + Properties.currentPlayer, GUILayout.Width (200));

		//let user play again
		if (GUILayout.Button ("Play Again")) {
			showWinner = false;
			Application.LoadLevel ("MainMenu");
		}

		//if a winner has been determined, show some info regarding the victory
		if (showWinner) {
			GUILayout.Label (winner + " wins!", GUILayout.Width(200));
		}
	}
}
