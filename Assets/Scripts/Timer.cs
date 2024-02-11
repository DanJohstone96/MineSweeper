using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField]
    private Text TimeText;

    [SerializeField]
    private float TimeCounter = 1;

    private BoardGenerator Bg;

	private void Awake()
	{
        Bg = FindObjectOfType<BoardGenerator>();
	}

    private void Update()
    {
        if (Bg.ReturnGameState() == BoardGenerator.GameState.Running) 
        {
            TimeCounter += Time.deltaTime;
            TimeText.text = Mathf.Floor(TimeCounter).ToString("000");
        }

        if (TimeCounter >= Mathf.Floor(999) && Bg.ReturnGameState() != BoardGenerator.GameState.Lost) 
        {
            Bg.RevealAllBombs(null);
            Bg.Loss();
            PopupController.Instance.Open(false);
        }
    }

    /// <summary>
    /// Returns active timer
    /// </summary>
    /// <returns>Time elapsed</returns>
    public float ReturnTime() 
    {
        return TimeCounter;
    }

    /// <summary>
    /// Reset Timer
    /// </summary>
	public void Reset()
	{
        TimeCounter = 1;
        TimeText.text = Mathf.Floor(0).ToString("000");
    }
}
