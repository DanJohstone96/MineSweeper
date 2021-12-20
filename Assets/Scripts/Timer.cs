using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text TimeText;
    public float TimeCounter = 1;

    public BoardGenerator Bg;

	private void Awake()
	{
        Bg = FindObjectOfType<BoardGenerator>();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Bg._GameState == BoardGenerator.GameState.Running) 
        {
            TimeCounter += Time.deltaTime;
            TimeText.text = Mathf.Floor(TimeCounter).ToString("000");
        }
        if (Bg._GameState == BoardGenerator.GameState.Idle && TimeCounter != 1) 
        {
            TimeCounter = 1;
            TimeText.text = Mathf.Floor(0).ToString("000");
        }
    }
}
