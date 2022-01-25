using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    // Reference to the timer text component
    public Text TimeText;
    
    // Float used to store the current game time
    public float TimeCounter = 1;

    // Reference to the board generator
    public BoardGenerator Bg;

	private void Awake()
	{
        // finds the board generator in the scene.
        Bg = FindObjectOfType<BoardGenerator>();
	}

    void Update()
    {
        // if the game is running then update and format the timer display
        if (Bg._GameState == BoardGenerator.GameState.Running) 
        {
            TimeCounter += Time.deltaTime;
            TimeText.text = Mathf.Floor(TimeCounter).ToString("000");
        }

        // if the user has lost or run out of time then stop the timer and end the game
        if (TimeCounter >= Mathf.Floor(999) && Bg._GameState != BoardGenerator.GameState.Lost) 
        {
            Bg.RevealAllBombs(null);
            Bg.Loose();
            Bg.PC.Open(false);
        }
    }

    // Reset the timer
	public void Reset()
	{
        TimeCounter = 1;
        TimeText.text = Mathf.Floor(0).ToString("000");
    }
}
