using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    public enum CellType { Empty,Bomb,Number}

    // The cells type
    public CellType _Type;

    // how many bombs are adjacent to this cell
    public int BombCloseCount;

    // Reference to the board generator
    public BoardGenerator BG;

    // The cells index in the board generators 2d array
    public Vector2 index;

    // Reference to the cells text component
    public Text MyText;

    // the string of the cell (how many adjacent bombs are there)
    public string MyString;

    // is the cell reveald
    public bool Revealed;

    // is the cell flagged
    public bool Flag;


    // Colours to use for each number
    public Color One;
    public Color Two;
    public Color Three;
    public Color Founr;
    public Color Five;
    public Color Six;
    public Color Seven;
    public Color Eight;

    // All sprites that can be used for the cell
    public Sprite _Flag;
    public Sprite _Unknown;
    public Sprite _Bomb;
    public Sprite _SpecialBomb;
    public Sprite _Blank;
    
    //public reference to the cells image
    public Image img;

    // Bools for the left and right mouse buttons
    public bool LMD;
    public bool RMD;

    private void Awake()
	{
        img = GetComponent<Image>();
	}

    // set the cells type
    public void SetCellType(CellType type) 
    {
        _Type = type;
    }

    // set the flagged state of the cell
    public void SetFlag() 
    {
        Debug.Log($"Set flag called revealed = {Revealed} flag = {Flag}");
        if (!Revealed) 
        {
            if (Flag)
            {
                MyText.text = "";
                img.sprite = _Unknown;
                Flag = false;
                BG.FlagChange(-1);
            }
            else 
            {
                if (BG.usedFlags == 0)
                {
                    return;
                }
                //MyText.text = "f";
                img.sprite = _Flag;
                Flag = true;
                BG.FlagChange(1);
                BG.CheckWin();
            }
        }
        if (BG._GameState == BoardGenerator.GameState.Idle)
        {
            BG._GameState = BoardGenerator.GameState.Running;
        }
    }

    // reveals the clicked cell
    public void Reveal() 
    {
        // if the game or cell is not in the correct state then return 
        if (BG._GameState == BoardGenerator.GameState.Won || BG._GameState == BoardGenerator.GameState.Lost || BG._GameState == BoardGenerator.GameState.Gen || Flag) 
        {
            return;
        }

        // if this is the first click of the game then generate the board around this cell to ensure that the first click is always safe
        if (BG._GameState == BoardGenerator.GameState.ListBuild) 
        {
            Debug.Log($"Sending index {this.index}");
            BG.ListBuild(this.index);
        }

        // if the cell is flagged then unflag the cell
        if (Flag) 
        {
            SetFlag();
        }

        // set the text colour based on the number of adjacent flags
        SetTextColour(MyText, BombCloseCount);

        // set the sprites display image as blank
        img.sprite = _Blank;


        // if the cell type is a number then set the cells string to display the number.
        if(_Type == CellType.Number)
        {
            MyText.text = MyString;

            StartCoroutine(BG.FaceChange(true, BG.Shock));
		}

        // if the cell type is empty then continue to check adjacent cells
		if (_Type == CellType.Empty)
		{
			BG.CheckAdjacentCells(index);
			StartCoroutine(BG.FaceChange(true, BG.Shock));
		}

        // if the cell type is a bomb then end the game.
		if (_Type == CellType.Bomb)
		{
			img.sprite = _SpecialBomb;
			BG.RevealAllBombs(this);
			BG.Loose();
			BG.PC.Open(false);
			StartCoroutine(BG.FaceChange(false, BG.Dead));
		}

        // if the game state is idle then set it to running
		if (BG._GameState == BoardGenerator.GameState.Idle) 
        {
            BG._GameState = BoardGenerator.GameState.Running;
        }

        // set this cells reveald state to true
        Revealed = true;

        // check if the game is won or lost
        BG.CheckWin();
    }

    // reveals only this cell
    public void RevealSelf() 
    {
        if (Flag)
        {
            SetFlag();
        }
        img.sprite = _Bomb;
        Revealed = true;
    }

    // gets input for the specified cell
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            LMD = true;
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            RMD = true;
        }
    }

    // determines if both the left and right mouse button have been pressed or either have been pressed.
    public void OnPointerUp(PointerEventData eventData)
    {
        if (LMD && RMD)
        {
            BG.RevealSurrounding(index);
        }
        else 
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                SetFlag();
            }
            else
            {
                Reveal();
            }
        }
        LMD = false;
        RMD = false;
    }

    // sets the text colour based on the cells value
    public void SetTextColour(Text txt, int num) 
    {
        switch (num) 
        {
            case 1:
                txt.color = One;
                break;
            case 2:
                txt.color = Two;
                break;
            case 3:
                txt.color = Three;
                break;
            case 4:
                txt.color = Founr;
                break;
            case 5:
                txt.color = Five;
                break;
            case 6:
                txt.color = Six;
                break;
            case 7:
                txt.color = Seven;
                break;
            case 8:
                txt.color = Eight;
                break;
        }
    
    }
}
