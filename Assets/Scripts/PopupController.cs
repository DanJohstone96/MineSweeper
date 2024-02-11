using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    public static PopupController Instance { get; private set; }

    [SerializeField]
	private Text Title;

    [SerializeField]
    private Text BestTime;

    [SerializeField]
    private GameObject BestTimeDisplay;

    [SerializeField]
    private GameObject Screen;

    [SerializeField]
    private string WinString;
    [SerializeField]
    private string LoseString;

    [SerializeField]
    private Color WinColor;
    [SerializeField]
    private Color LooseColor;

    private RectTransform DisplayRect;

	private void Awake()
	{
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        DisplayRect = Screen.GetComponent<RectTransform>();
        Close();
	}

    // openes up the popup and sets the text and colour based on the result passed in.
    /// <summary>
    /// Open popup and set styling based on result param
    /// </summary>
    /// <param name="Result">if true game won</param>
	public void Open(bool Result) 
    {
        transform.localPosition = Vector3.zero;
        Screen.SetActive(true);
        Title.color = Result ? WinColor : LooseColor;
        Title.text = Result ? WinString : LoseString;
        BestTimeDisplay.SetActive(Result);
        LayoutRebuilder.ForceRebuildLayoutImmediate(DisplayRect);
    }

    /// <summary>
    /// Sets the String of the Best Time Display. 
    /// </summary>
    /// <param name="bestTime">String to be displayed in the best time display</param>
    public void SetBestTimeString(string bestTime) 
    {
        BestTime.text = bestTime;
    }

    /// <summary>
    /// Used to close the popup.
    /// </summary>
    public void Close() 
    {
        Screen.SetActive(false);
    }
}
