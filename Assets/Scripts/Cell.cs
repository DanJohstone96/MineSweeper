using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    public enum CellType { Empty,Bomb,Number}

    [SerializeField]
    private CellType _Type;

    public int BombCloseCount;

    public BoardGenerator BG;

    public Vector2 index;

    public Text MyText;

    public string MyString;

    public bool Revealed;

    public bool Flag;

    public Color One;
    public Color Two;
    public Color Three;
    public Color Founr;
    public Color Five;
    public Color Six;
    public Color Seven;
    public Color Eight;

    public Sprite _Flag;
    public Sprite _Unknown;
    public Sprite _Bomb;
    public Sprite _SpecialBomb;
    public Sprite _Blank;
    
    public Image img;

    private bool LMD;
    private bool RMD;

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
                img.sprite = _Flag;
                Flag = true;
                BG.FlagChange(1);
                BG.CheckWin();
            }
        }
        if (BG.ReturnGameState() == BoardGenerator.GameState.Idle)
        {
            BG.SetGameState(BoardGenerator.GameState.Running);
        }
    }

    // reveals the clicked cell
    public void Reveal() 
    {
        BoardGenerator.GameState currentState = BG.ReturnGameState();

        if (currentState == BoardGenerator.GameState.Won || currentState == BoardGenerator.GameState.Lost || currentState == BoardGenerator.GameState.Gen || Flag) 
        {
            return;
        }

        Revealed = true;

        if (currentState == BoardGenerator.GameState.ListBuild) 
        {
            Debug.Log($"Sending index {this.index}");
            BG.ListBuild(this.index);
        }

        if (Flag) 
        {
            SetFlag();
        }

        SetTextColour(MyText, BombCloseCount);

        img.sprite = _Blank;

        switch (_Type) 
        {
            case CellType.Number:
                MyText.text = MyString;
                BG.CallFaceChange(true,1);
                break;
            case CellType.Empty:
                BG.CheckAdjacentCells(index);
                BG.CallFaceChange(true, 1);
                break;
            case CellType.Bomb:
                img.sprite = _SpecialBomb;
                BG.RevealAllBombs(this);
                BG.Loss();
                PopupController.Instance.Open(false);
                BG.CallFaceChange(false, 2);
                break;
        }

		if (currentState == BoardGenerator.GameState.Idle) 
        {
            BG.SetGameState(BoardGenerator.GameState.Running);
        }
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

    public CellType ReturnType() 
    {
        return _Type;
    }

    public void SetType(CellType type) 
    {
        _Type = type;
    }
}
