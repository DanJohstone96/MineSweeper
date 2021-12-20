using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class BoardGenerator : MonoBehaviour
{
    public GameObject _Cell;

    public List<Cell> cells = new List<Cell>();

    public List<Cell> Bombs = new List<Cell>();

    public List<List<Cell>> GridList = new List<List<Cell>>();

    public List<Cell> FlatList = new List<Cell>();

    public List<Vector2> BombIndex = new List<Vector2>();

    public int RowCount = 16;
    public int ColumnCount = 30;

    public int CellCount = 480;

    public Transform GameBody;

    public int MaxBombCount;
    public int BombsAdded;

    

    public enum GameState {Gen,Idle,Running,Lost,Won}

    public GameState _GameState;

    public int AllowedFlags = 99;

    public int usedFlags;

    public Text FlagText;

    public PopupController PC;

    public List<Vector2> OldIndex = new List<Vector2>();

    public Stopwatch SW;

    // Start is called before the first frame update
    void Start()
    {
        //BoardGen(false);
    }

    private void BoardGen(bool UseOld) 
    {
        SW = new Stopwatch();
        SW.Start();
        _GameState = GameState.Gen;
        for (int i = 0;i<RowCount; i++) 
        {
            List<Cell> Row = new List<Cell>();
            for (int y = 0; y < ColumnCount;y++) 
            {
                Cell _cell = Instantiate(_Cell, GameBody).GetComponent<Cell>();
                FlatList.Add(_cell);
                Row.Add(_cell);
                Row[y].BG = this;
                Row[y].index = new Vector2(i,y);
                cells.Add(Row[y]);
            }
            GridList.Add(Row);
        }
        usedFlags = AllowedFlags;
        FlagText.text = AllowedFlags.ToString("000");
        if (UseOld) 
        {
            foreach (Vector2 chord in OldIndex) 
            {
                Cell FoundCell = GridList[(int)chord.x][(int)chord.y];
                FoundCell._Type = Cell.CellType.Bomb;
                Text CellText = FoundCell.GetComponentInChildren<Text>();
                //CellText.text = "b";
                FoundCell.MyString = "B";

                Bombs.Add(FoundCell);
                BombIndex.Add(chord);
            }
            SetNumbers();
        }
		else 
        {
            SetBombs();
        }
    }

    private void SetBombs() 
    {
        for (int i =0; i < MaxBombCount;i++) 
        {
            int index = UnityEngine.Random.Range(0, FlatList.Count);

            Cell FoundCell = FlatList[index];

            FoundCell._Type = Cell.CellType.Bomb;
            //Text CellText = FoundCell.GetComponentInChildren<Text>();
            //CellText.text = "b";
            FoundCell.MyString = "B";

            Bombs.Add(FoundCell);
            //BombIndex.Add(IndexPair);
            BombIndex.Add(FoundCell.index);
            FlatList.RemoveAt(index);
        }

        foreach (Vector3 vec in BombIndex) 
        {
            OldIndex.Add(vec);
        }
        SetNumbers();
    }

    private void SetNumbers() 
    {
        for (int i = 0;i<BombIndex.Count;i++) 
        {
            Vector2 indexPair = BombIndex[i];
            Vector2[] cellsToCheck = ReturnAdjacentList(indexPair);

            foreach (Vector2 index in cellsToCheck) 
            {
                try
                {
                    Cell FoundCell = GridList[(int)index.x][(int)index.y];
                   
                    if (FoundCell._Type != Cell.CellType.Bomb) 
                    {
                        FoundCell.BombCloseCount += 1;
                        FoundCell._Type = Cell.CellType.Number;
                        //Text CellText = FoundCell.GetComponentInChildren<Text>();
                        //CellText.text = FoundCell.BombCloseCount.ToString();

                        FoundCell.MyString = FoundCell.BombCloseCount.ToString();
                    }
                }
                catch (Exception ex) 
                {
                    //Debug.LogError($"Exeption encountered {ex}");
                }      
            }
        }
        _GameState = GameState.Idle;
        SW.Stop();
        ShowEmpty();
        UnityEngine.Debug.Log($"Elapsed time {SW.ElapsedMilliseconds}");
    }

    public void RevealAllBombs() 
    {
        foreach (Cell cell in Bombs) 
        {
            cell.RevealSelf();
        }
    }

    public void CheckAdjacentCells(Vector2 index) 
    {
        Vector2[] cellsToCheck = ReturnAdjacentList(index);

        foreach (Vector2 cell in cellsToCheck)
        {
            try
            {
                Cell CurrentCell = GridList[(int)cell.x][(int)cell.y];
                if (!CurrentCell.Revealed && CurrentCell._Type != Cell.CellType.Bomb)
                {
                    CurrentCell.Reveal();
                }
            }
            catch 
            {
            
            }
            
            
        }
    }

    private Vector2[] ReturnAdjacentList(Vector2 StartIndex) 
    {
        Vector2[] cellsToCheck = new Vector2[]
                    {   
                        //Top M
                        new Vector2(StartIndex.x + 1, StartIndex.y),
                        //Top R
                        new Vector2(StartIndex.x + 1, StartIndex.y+1),
                        //Top L
                        new Vector2(StartIndex.x + 1, StartIndex.y-1),
                        //Mid L
                        new Vector2(StartIndex.x, StartIndex.y-1),
                        //Mid R
                        new Vector2(StartIndex.x, StartIndex.y+1),
                        //Btm M
                        new Vector2(StartIndex.x - 1, StartIndex.y),
                        //Btm R
                        new Vector2(StartIndex.x - 1, StartIndex.y+1),
                        //Btm L
                        new Vector2(StartIndex.x - 1, StartIndex.y-1),
                    };
        return cellsToCheck;
    }

    private void ShowEmpty() 
    {
        foreach (Cell cell in FlatList) 
        {
            if (cell._Type == Cell.CellType.Empty) 
            {
                Text CellText = cell.GetComponentInChildren<Text>();
                CellText.text = "E";
            }
        }
    }

    private bool IndexFound(Vector2 Pair) 
    {
        if (BombIndex.Contains(Pair))
        {
            return true;
        }
        else 
        {
            return false;
        }
    }

    public void Loose() 
    {
        _GameState = GameState.Lost;
        UnityEngine.Debug.Log("BG.Loose called");
    }

    public void ClearBoard(bool UseOld) 
    {
        foreach (Transform child in GameBody) 
        {

            Destroy(child.gameObject);
        }
        cells.Clear();
        Bombs.Clear();
        GridList.Clear();
        BombIndex.Clear();
        FlatList.Clear();
        PC.Close();
        if (UseOld)
        {
            BoardGen(true);

        }
        else 
        {
            OldIndex.Clear();
            BoardGen(false);

        }

    }

    public void FlagChange(int change) 
    {
        UnityEngine.Debug.Log($"Set flag called with {change}");
        usedFlags -= change;
        UnityEngine.Debug.Log($"Used flags = {usedFlags}");
        //if (usedFlags < 0) 
        {
            //usedFlags = 0;
        }

        FlagText.text = usedFlags.ToString("000");
    }

    public void CheckWin() 
    {
        bool result = true;
        /*
        foreach (Cell bomb in Bombs)
        {
            if (!bomb.Flag) 
            {
                result = false;  
            }
        }
        */

        foreach (Cell cell in cells) 
        {
            if (cell._Type == Cell.CellType.Bomb)
            {
                if (!cell.Flag)
                {
                    result = false;
                }
            }
            else 
            {
                if (!cell.Revealed) 
                {
                    result = false;
                }
            }
        }

        if (result) 
        {
            _GameState = GameState.Won;
            PC.Open(true);
        }
    }

    public void Quit() 
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
