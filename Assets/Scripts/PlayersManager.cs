//This script checks for player trying to enter or quit the game.
//Pressing the A button get's a player to join the game.
//Pressing the B button get's a player to quit the game.
//A simple UI let the users know who has joined the game already.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayersManager : MonoBehaviour {

	public GameObject panelPlayerJoinedPrefab;
	public GameObject panelPlayerList;
	public GameObject panelJoinGameInvite;
	public int maxNumPlayers;	//Maximum is 11
	public List<Player> listOfPlayers {get; private set;}
	private List<Controller> listOfAvailableContollers;
	private List<int> listOfAvailableIds;

	void Start () {
		listOfAvailableIds = new List<int> ();
		for (int id = 1, max = Mathf.Min(maxNumPlayers, 11); id <= max; id++) {
			listOfAvailableIds.Add (id);
		}
		listOfPlayers = new List<Player> ();
		listOfAvailableContollers = new List<Controller>() {
			new Controller("C1"),	//Controller 1
			new Controller("C2"),	//Controller 2
			new Controller("C3"),	//Controller 3
			new Controller("C4"),	//Controller 4
			new Controller("C5"),	//Controller 5
			new Controller("C6"),	//Controller 6
			new Controller("C7"),	//Controller 7
			new Controller("C8"),	//Controller 8
			new Controller("C9"),	//Controller 9
			new Controller("C10"),	//Controller 10
			new Controller("C11")	//Controller 11
		};
	}

	void Update () {
		foreach (Controller controller in listOfAvailableContollers) {
			if (Input.GetButtonDown(controller.buttonA)) {
				AddPlayer (controller);
				break;
			}
		}
		foreach (Player player in listOfPlayers) {
			if (Input.GetButtonDown(player.controller.buttonB)) {
				RemovePlayer (player);
				break;
			}
		}
	}

	//Adds a player to the game
	private void AddPlayer(Controller controller) {
		if (listOfAvailableIds.Count > 0) {
			GameObject panelPlayerJoined = GameObject.Instantiate(panelPlayerJoinedPrefab, panelPlayerList.transform);
			panelPlayerJoined.GetComponent<RectTransform> ().localScale = Vector3.one;
			panelPlayerJoined.transform.Find ("Text").GetComponent<Text>().text = "Player " + listOfAvailableIds[0].ToString() + " joined the game!";
			listOfPlayers.Add (new Player (listOfAvailableIds[0], controller, panelPlayerJoined));
			listOfAvailableIds.RemoveAt(0);
			listOfAvailableContollers.Remove (controller);
			if (listOfAvailableIds.Count == 0) {
				panelJoinGameInvite.SetActive(false);
			}
			Canvas.ForceUpdateCanvases ();
		}
	}

	//Removes a player from the game
	private void RemovePlayer(Player player) {
		listOfAvailableContollers.Add (player.controller);
		listOfAvailableIds.Add (player.id);
		listOfAvailableIds.Sort ();
		Destroy (player.panelPlayerJoined);
		listOfPlayers.Remove (player);
		panelJoinGameInvite.SetActive(true);
	}
}
