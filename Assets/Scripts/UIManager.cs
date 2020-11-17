using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour {

	public Player player;

	public void startGame()
	{
		player.stateToGame();
	}


}
