using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
	// Reference to the title text of the popup box
	public Text Title;

	// Reference to the Popups gameobject
	public GameObject Screen;

    // Text colour to use if the user wins or loses
	public Color WinColor;
	public Color LooseColor;

	private void Awake()
	{
        // ensures the popup is closed at start
        Close();
	}

    // openes up the popup and sets the text and colour based on the result passed in.
	public void Open(bool Result) 
    {
        Screen.SetActive(true);
        if (Result)
        {
            Title.color = WinColor;
            Title.text = "WINNER";
        }
        else 
        {
            Title.color = LooseColor;
            Title.text = "LOSER";
        }
    }

    //closes the popup
    public void Close() 
    {
        Screen.SetActive(false);
    }
}
