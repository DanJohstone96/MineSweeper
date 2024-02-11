using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class BoardGenerator : MonoBehaviour
{
    public enum GameState { Gen, ListBuild, Idle, Running, Lost, Won }

    public int usedFlags => _usedFlags;

    [Header("Board Settings")]
    [SerializeField]
    private int RowCount = 16;
    [SerializeField]
    private int ColumnCount = 30;
    [SerializeField]
    private int MaxBombCount;
    [SerializeField]
    private List<Sprite> FaceSprites = new List<Sprite>();
    [SerializeField]
    private Image TopFace;
    [SerializeField]
    private Text FlagText;
    [SerializeField]
    private Timer _Timer;
    [SerializeField]
    private Transform GameBody;

    [Header("Board Info")]
    [SerializeField]
    private GameState _GameState;
    [SerializeField]
    private GameObject _Cell;
    [SerializeField]
    private List<Cell> cells = new List<Cell>();
    [SerializeField]
    private List<Cell> Bombs = new List<Cell>();
    [SerializeField]
    private List<List<Cell>> GridList = new List<List<Cell>>();
    [SerializeField]
    private List<Cell> FlatList = new List<Cell>();
    [SerializeField]
    private List<Vector2> BombIndex = new List<Vector2>();
    
    [SerializeField]
    private List<Vector2> OldIndex = new List<Vector2>();


    private int AllowedFlags => MaxBombCount;
    private int _usedFlags;
	// stopwatch for assesing loading times
	private Stopwatch SW;

    private void Start()
    {      
        BoardGen(false);
    }

#if UNITY_EDITOR

    private void Update()
	{
        if (Input.GetKeyDown(KeyCode.W)) 
        {
            AutoWin();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            ShowEmpty();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
           ShowBombs();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayerPrefs.DeleteAll();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            FlagAllBombs();
        }
    }
#endif

    /// <summary>
    /// Generate the Initial board.
    /// </summary>
    /// <param name="UseOld">If true uses the board layout from a previous game</param>
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
        _usedFlags = AllowedFlags;
        FlagText.text = AllowedFlags.ToString("000");
        if (UseOld) 
        {
            foreach (Vector2 chord in OldIndex) 
            {
                Cell FoundCell = GridList[(int)chord.x][(int)chord.y];
                FoundCell.SetCellType(Cell.CellType.Bomb);
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

    /// <summary>
    /// Builds lists used for game logic.
    /// </summary>
    /// <param name="StartIndex">Idex used to find adjacent cells</param>
    public void ListBuild(Vector2 StartIndex) 
    {
        SW = new Stopwatch();
        SW.Start();
        _GameState = GameState.ListBuild;

        List<Cell> cells = new List<Cell>();
       
        foreach (Vector2 vec in ReturnAdjacentArray(StartIndex))
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

    /// <summary>
    /// Sets random cells to bombs
    /// </summary>
    private void SetBombs() 
    {
        for (int i =0; i < MaxBombCount;i++) 
        {
            int index = UnityEngine.Random.Range(0, FlatList.Count);

            Cell FoundCell = FlatList[index];

            FoundCell.SetCellType(Cell.CellType.Bomb);

            Bombs.Add(FoundCell);
            BombIndex.Add(FoundCell.index);
            FlatList.RemoveAt(index);
        }

        foreach (Vector2 vec in BombIndex) 
        {
            OldIndex.Add(vec);
        }
        SetNumbers();
    }

    /// <summary>
    /// Sets the bomb numbers for cells adjacent to bombs
    /// </summary>
    private void SetNumbers() 
    {
        for (int i = 0;i<BombIndex.Count;i++) 
        {
            Vector2 indexPair = BombIndex[i];
            Vector2[] cellsToCheck = ReturnAdjacentArray(indexPair);

            foreach (Vector2 index in cellsToCheck) 
            {
                try
                {
                    Cell FoundCell = GridList[(int)index.x][(int)index.y];
                   
                    if (FoundCell.ReturnType() != Cell.CellType.Bomb) 
                    {
                        FoundCell.BombCloseCount += 1;
                        FoundCell.SetCellType(Cell.CellType.Number);

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
        UnityEngine.Debug.Log($"Elapsed time {SW.ElapsedMilliseconds} Ms");
    }

    /// <summary>
    /// Method to reveal bombs on game loss
    /// </summary>
    /// <param name="_cell">The cell that lost the game</param>
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

    /// <summary>
    /// Used to check adjacent cells of the cell at the given index
    /// </summary>
    /// <param name="index">Index of origin cell to check</param>
    public void CheckAdjacentCells(Vector2 index) 
    {
        Vector2[] cellsToCheck = ReturnAdjacentArray(index);

        foreach (Vector2 cell in cellsToCheck)
        {
            try
            {
                Cell CurrentCell = GridList[(int)cell.x][(int)cell.y];
                if (!CurrentCell.Revealed && CurrentCell.ReturnType() != Cell.CellType.Bomb)
                {
                    CurrentCell.Reveal();
                }
            }
            catch 
            {
            
            }
        }
    }

    /// <summary>
    /// Reveals all cells around surrounding cell
    /// </summary>
    /// <param name="index">Index of origin cell to reveal from</param>
    public void RevealSurrounding(Vector2 index) 
    {
        Vector2[] cellsToCheck = ReturnAdjacentArray(index);
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

    /// <summary>
    /// Returns an array of adjacent cell indexes
    /// </summary>
    /// <param name="StartIndex">Index of origin cell to return adjacent cells</param>
    /// <returns>array of adjacent cell indexes</returns>
    private Vector2[] ReturnAdjacentArray(Vector2 StartIndex) 
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

    /// <summary>
    /// Debug method to win the game
    /// </summary>
    private void AutoWin() 
    {
        foreach (List<Cell> cells in GridList)
        {
            foreach (Cell cell in cells)
            {
                if (cell.ReturnType() == Cell.CellType.Bomb)
                {
                    cell.SetFlag();
                }
                else
                {
                    cell.Reveal();
                }
            }
        }
    }

    /// <summary>
    /// Debug method to show empty spaces
    /// </summary>
    private void ShowEmpty() 
    {
        if (_GameState != GameState.Running) 
        {
            return;
        }

        foreach (Cell cell in cells) 
        {
            if (cell.ReturnType() == Cell.CellType.Empty) 
            {
                Text CellText = cell.MyText;
                CellText.text = "E";
            }
        }
    }

    /// <summary>
    /// Debug Method to show bombs
    /// </summary>
    private void ShowBombs() 
    {
        foreach (Cell cell in Bombs) 
        {
            Text CellText = cell.MyText;
            CellText.text = "B";
        }
    }

    /// <summary>
    /// Debug Method To Flag all Bombs
    /// </summary>
    private void FlagAllBombs() 
    {
        foreach (Cell cell in Bombs)
        {
            cell.SetFlag();
        }
    }

    /// <summary>
    /// Sets the game state to loss.
    /// </summary>
    public void Loss() 
    {
        _GameState = GameState.Lost;
        UnityEngine.Debug.Log("BG.Loss called");
    }

    /// <summary>
    /// Sets the game state to win.
    /// </summary>
    private void GameWin() 
    {
        float winTime = _Timer.ReturnTime();

        float BestTime = PlayerPrefs.GetFloat("BestTime");

        UnityEngine.Debug.Log($"Stored best time = {PlayerPrefs.GetFloat("BestTime")}");
        if (BestTime == 0)
        {
            PopupController.Instance.SetBestTimeString($"New Record - {winTime}");

            PlayerPrefs.SetFloat("BestTime", winTime);
            UnityEngine.Debug.Log($"Best time is now set to {(int)PlayerPrefs.GetFloat("BestTime")}");
        }
        else if (winTime < BestTime) 
        {
            PopupController.Instance.SetBestTimeString($"New Record - {winTime} \n Previous Best Time - {BestTime}");
            PlayerPrefs.SetFloat("BestTime", winTime);
        }
        else
        {
            PopupController.Instance.SetBestTimeString($"Previous Best Time - {BestTime} \n Current Time - {winTime}");
        }
    }

    /// <summary>
    /// Method to clear the board for reset
    /// </summary>
    /// <param name="UseOld">If true the old board layout will be preserved</param>
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
        PopupController.Instance.Close();
        _Timer.Reset();
        CallFaceChange(false, 0);
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

    /// <summary>
    /// Updates the flag display
    /// </summary>
    /// <param name="change">number to change display by</param>
    public void FlagChange(int change) 
    {
        _usedFlags -= change;
        FlagText.text = usedFlags.ToString("000");
    }

     /// <summary>
     /// Checks the win state of the game
     /// </summary>
    public void CheckWin() 
    {
        bool result = true;
        foreach (Cell cell in cells) 
        {
            if (cell.ReturnType() == Cell.CellType.Bomb)
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
            PopupController.Instance.Open(true);
            CallFaceChange(false,3);
            GameWin();
        }
    }

    /// <summary>
    /// Used to call the face change coroutine
    /// </summary>
    /// <param name="changeBack">If true the face will revert to idle after delay</param>
    /// <param name="index">index of face sprite</param>
    public void CallFaceChange(bool changeBack, int index) 
    {
        StopAllCoroutines();
        StartCoroutine(FaceChange(changeBack, index));
    }

    /// <summary>
    /// Sets the face sprite to the given index
    /// </summary>
    /// <param name="index">Index of face sprite</param>
    private void SetFace(int index) 
    {
        TopFace.sprite = FaceSprites[index];
    }

    /// <summary>
    /// Coroutine to handle face changing.
    /// </summary>
    /// <param name="changeBack">if true the face will change back to idle</param>
    /// <param name="index">index of face to change</param>
    /// <returns>null</returns>
    private IEnumerator FaceChange(bool changeBack,int index) 
    {        
        if (changeBack)
        {
            SetFace(index);
            yield return new WaitForSeconds(.15f);
            SetFace(0);

        }
        else 
        {
            SetFace(index);
            yield return null;
        }
    }

    /// <summary>
    /// Returns the games state
    /// </summary>
    /// <returns>Game State</returns>
    public GameState ReturnGameState() 
    {
        return _GameState;
    }

    /// <summary>
    /// Sets the state of the game
    /// </summary>
    /// <param name="state">State to set</param>
    public void SetGameState(GameState state) 
    {
        _GameState=state;
    }

    /// <summary>
    /// Quits application
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }
}
