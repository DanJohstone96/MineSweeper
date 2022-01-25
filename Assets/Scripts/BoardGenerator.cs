using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class BoardGenerator : MonoBehaviour
{
	//Cell Prefab used in board generation
	public GameObject _Cell;

	//List of all cells
	public List<Cell> cells = new List<Cell>();

	//List of all bombs
	public List<Cell> Bombs = new List<Cell>();

	// 2d list of game board
	public List<List<Cell>> GridList = new List<List<Cell>>();

	// flattened version of the 2d list used for bomb generation
	public List<Cell> FlatList = new List<Cell>();

	// list of bomb locations in the grid list
	public List<Vector2> BombIndex = new List<Vector2>();

	// number of rows and columns to generate (Currently fixed could be expanded later)
	public int RowCount = 16;
    public int ColumnCount = 30;
    //public int CellCount = 480;

    public Transform GameBody;

    // maximum number of bombs to add to grid (Currently fixed could be expanded later)
    public int MaxBombCount;
    public int BombsAdded;

    // a collection of Sprites for the face at the top of the board
    public Sprite Happy;
    public Sprite Dead;
    public Sprite Shock;
    public Sprite Win;

    //Reference to the image component of the game face
    public Image TopFace;

    public enum GameState {Gen,ListBuild,Idle,Running,Lost, Won }
	//The current Game State
	public GameState _GameState;

	// this is used to set the maximum number of allowed flags.
	public int AllowedFlags = 99;

	// int to store the current ammount of used flags
	public int usedFlags;

	// reference to the text tracking the flag count in the UI.
	public Text FlagText;

	// reference to the popup controller for when the game ends.
	public PopupController PC;

	// Vector 2 list that stores the index of bombs form the previous game board
	public List<Vector2> OldIndex = new List<Vector2>();

	// public stopwatch for assesing loading times
	public Stopwatch SW;

	// Public reference to timer class
	public Timer _Timer;

    void Start()
    {
        BoardGen(false);
    }

    // generates the initial game board visual ready for the users first click
    private void BoardGen(bool UseOld) 
    {
        
        _GameState = GameState.Gen;
        for (int i = 0;i<RowCount; i++) 
        {
            List<Cell> Row = new List<Cell>();
            for (int y = 0; y < ColumnCount;y++) 
            {
                Cell _cell = Instantiate(_Cell, GameBody).GetComponent<Cell>();
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
                FoundCell.MyString = "B";

                Bombs.Add(FoundCell);
                BombIndex.Add(chord);
            }
            SetNumbers();
        }
		else 
        {
            _GameState = GameState.ListBuild;
        }
    }

    // Builds the Grid list and flat list based of the users initial click
    public void ListBuild(Vector2 StartIndex) 
    {
        SW = new Stopwatch();
        SW.Start();
        _GameState = GameState.ListBuild;

        List<Cell> cells = new List<Cell>();
       
        foreach (Vector2 vec in ReturnAdjacentList(StartIndex))
        {
            try
            {
                cells.Add(GridList[(int)vec.x][(int)vec.y]);
            }
            catch (Exception ex) 
            {
                UnityEngine.Debug.Log(ex);
            }
        }
        cells.Add(GridList[(int)StartIndex.x][(int)StartIndex.y]);
        for (int i = 0; i < RowCount; i++)
        {
            
            for (int y = 0; y < ColumnCount; y++)
            {
                Cell CurrentCell = GridList[i][y];

                if (!cells.Contains(CurrentCell)) 
                {
                    FlatList.Add(CurrentCell);
                }

            }
            
        }
        SetBombs();
    }

    // sets the bombs randomly based on the max bomb count
    private void SetBombs() 
    {
        for (int i =0; i < MaxBombCount;i++) 
        {
            int index = UnityEngine.Random.Range(0, FlatList.Count);

            Cell FoundCell = FlatList[index];

            FoundCell._Type = Cell.CellType.Bomb;

            Bombs.Add(FoundCell);
            BombIndex.Add(FoundCell.index);
            FlatList.RemoveAt(index);
        }

        foreach (Vector3 vec in BombIndex) 
        {
            OldIndex.Add(vec);
        }
        SetNumbers();
    }

    // Sets the bomb numbers for cells that are adjacent to bombs 
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
        UnityEngine.Debug.Log($"Elapsed time {SW.ElapsedMilliseconds}");
    }

    // reveals all bombs on the board
    public void RevealAllBombs(Cell _cell) 
    {
        foreach (Cell cell in Bombs) 
        {
            if (cell != _cell) 
            {
                cell.RevealSelf();
            }
        }
    }

    // checks cells adjacent to the index and reveals them if they are not a bomb
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

    // Reveals all adjacent cells to the index cell
    public void RevealSurrounding(Vector2 index) 
    {
        Vector2[] cellsToCheck = ReturnAdjacentList(index);
        foreach (Vector2 cell in cellsToCheck)
        {
            try
            {
                Cell CurrentCell = GridList[(int)cell.x][(int)cell.y];
                if (!CurrentCell.Revealed)
                {
                    CurrentCell.Reveal();
                }
            }
            catch
            {

            }
        }
    }

    // returns a vector 2 array of all cell indexes adjacent to the start index
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

    // a debug method for showing all empty spaces
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

    // set the game state to lost
    public void Loose() 
    {
        _GameState = GameState.Lost;
        UnityEngine.Debug.Log("BG.Loose called");
    }

    // clear the game board and all associated lists.
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
        _Timer.Reset();
        TopFace.sprite = Happy;
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

    // Update the flag display
    public void FlagChange(int change) 
    {
        usedFlags -= change;
        FlagText.text = usedFlags.ToString("000");
    }

    // check the win state of the game and set the game state accordingly 
    public void CheckWin() 
    {
        bool result = true;
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
            StartCoroutine(FaceChange(false,Win));
        }
    }

    // quit the application
    public void Quit() 
    {
        Application.Quit();
    }

    // This function controlls changing the face sprite at the top of the screen.
    public IEnumerator FaceChange(bool changeBack,Sprite Face) 
    {
        Sprite OldFace = TopFace.sprite;

        if (changeBack)
        {
            TopFace.sprite = Face;
            yield return new WaitForSeconds(.15f);
            TopFace.sprite = Happy;

        }
        else 
        {
            TopFace.sprite = Face;
            yield return null;
        }
    }
}
