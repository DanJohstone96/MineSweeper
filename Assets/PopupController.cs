using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    public Text Title;

    public GameObject Screen;

    public Color WinColor;
    public Color LooseColor;

	private void Awake()
	{
        Close();
	}

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
            Title.text = "LOOSER";
        }
    }

    public void Close() 
    {
        Screen.SetActive(false);
    }
}
