//class representing a combination of players
public class Combo {
	public string[] players; //N, S, NW, NE, SW, SE
	public char[] strats; //s (self), o (overtake), a (all), h (win)
	public Combo(string[] players, char[] strats) {
		this.players = players;
		this.strats = strats;
	}
}