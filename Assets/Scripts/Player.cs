using UnityEngine;

public class Player {

	public GameObject panelPlayerJoined {get; private set;}
	public int id {get; private set;}
	public Controller controller {get; private set;}

	public Player(int id, Controller controller, GameObject panelPlayerJoined) {
		this.id = id;
		this.controller = controller;
		this.panelPlayerJoined = panelPlayerJoined;
	}
}
