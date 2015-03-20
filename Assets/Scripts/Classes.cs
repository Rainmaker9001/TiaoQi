using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//this file contains classes used for organizing information in PlayerController.cs

//board representation (just data packet, no functions)
public class BoardRep {
	public string[,] tiles; //array representing tiles on board: "N", "invalid", "empty" are states
	public Dictionary<string, int[,]> ptp; //lookup table: each player's marbles' positions

	public BoardRep(string[,] tiles, Dictionary<string, int[,]> playerToPieces) {
		this.tiles = tiles;
		this.ptp = playerToPieces;
	}
}