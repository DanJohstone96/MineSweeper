using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour,IPointerClickHandler
{
    public enum CellType { Empty,Bomb,Number}

    public CellType _Type;

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCellType(CellType type) 
    {
        _Type = type;
    }

    public void SetFlag() 
    {
        Debug.Log($"Set flag called revealed = {Revealed} flag = {Flag}");
        if (!Revealed) 
        {
            if (Flag)
            {
                MyText.text = "";
                Flag = false;
                BG.FlagChange(-1);
            }
            else 
            {
                if (BG.usedFlags == 0)
                {
                    return;
                }
                MyText.text = "f";
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

    public void Reveal() 
    {
        if (BG._GameState == BoardGenerator.GameState.Won || BG._GameState == BoardGenerator.GameState.Lost || BG._GameState == BoardGenerator.GameState.Gen || Flag) 
        {
            return;
        }

        if (Flag) 
        {
            SetFlag();
        }
        SetTextColour(MyText, BombCloseCount);

        if (MyString == "E")
        {
            Image img = GetComponent<Image>();
            img.color = new Color(0.4705882f, 0.4705882f, 0.4705882f);
        }
        else 
        {
            MyText.text = MyString;
        }


        
        Revealed = true;
        if (_Type == CellType.Empty) 
        {
            BG.CheckAdjacentCells(index);
        }
        if (_Type == CellType.Bomb) 
        {
            Image img = GetComponent<Image>();
            img.color = new Color(0.7921569f, 0.07450981f, 0.09411765f);
            BG.RevealAllBombs();
            BG.Loose();
            BG.PC.Open(false);
        }
        if (BG._GameState == BoardGenerator.GameState.Idle) 
        {
            BG._GameState = BoardGenerator.GameState.Running;
        }
        BG.CheckWin();
    }

    public void RevealSelf() 
    {
        if (Flag)
        {
            SetFlag();
        }
        MyText.text = MyString;
        Revealed = true;
    }

	public void OnPointerClick(PointerEventData eventData)
	{
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //Debug.Log("Right Mouse Button Clicked on: " + name);
            SetFlag();
        }
        else 
        {
            Reveal();
            //Debug.Log("Left Mouse Button Clicked on: " + name);
        }
    }

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
