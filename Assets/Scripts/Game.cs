using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
/* Must setup the TetriminoGroupPrefab array like this:
 0 - I
 1 - J
 2 - L 
 3 - O
 4 - S
 5 - T
 6 - Z 
 7 - .
     */

public class Game : MonoBehaviour
{
    public Transform[] TetriminoGroupPrefabs = new Transform[8];
    public Action TetriminoGroupDead;
    [SerializeField]
    private GameObject deadTetriminos;
    [SerializeField]
    private GameObject holdArea;
    private Vector3 spawnLocation;
    private bool[,] gridBitFlags = new bool[10,20];
    private List<Tetrimino> tetris = new List<Tetrimino>();
    private TetriminoGroup activeTetriminoGroup;
    private Transform heldTetriminoGroupTransform;
    private Queue<int> upcomingTetriminoGroups = new Queue<int>();
    private Queue<Transform> upcomingTetriminoGroupTransforms = new Queue<Transform>();
    [SerializeField]
    private GameObject stackBackground;
    private PlayerInputActions playerInputActions;
    private int currentTetriminoGroupNum;
    private int heldTetriminoGroup = -1;
    private bool alreadyHeld = false;
    [SerializeField]
    private bool debugTetriminoGroup = false;
    [SerializeField]
    private int debugTetriminoGroupNum = -1;
    private int nextInQueue = 3;
    private ScoreManager scoreManager;
    private bool isPaused = false;
    [SerializeField]
    private GameObject pauseMenu;
    [SerializeField]
    private GameObject gameOverScreen;
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip[] audioClips;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1;
        Screen.SetResolution(800, 600, false);
        playerInputActions = new PlayerInputActions();
        audioSource = gameObject.GetComponent<AudioSource>();
        scoreManager = gameObject.GetComponent<ScoreManager>();
        resetGridBitFlags();
    }
    void Start()
    {
        load();
        spawnLocation = new Vector3(-0.32f, 6.08f, 0);
        upcomingTetriminoGroups = initTetriminoGroupQueue();
        initUpcomingQueue();
        if (debugTetriminoGroup)
            SpawnTetriminoGroup(debugTetriminoGroupNum);
        else
        {
            SpawnTetriminoGroup(upcomingTetriminoGroups.Dequeue());
            updateUpcomingStack();
        }
    }
    private void OnEnable()
    {
        playerInputActions.Enable();
        TetriminoGroupDead += onTetriminoGroupDead;
        playerInputActions.Player.Hold.performed += holdTetriminoGroup;
        playerInputActions.UI.Pause.performed += onPause;
    }
    private void OnDisable()
    {
        TetriminoGroupDead -= onTetriminoGroupDead;
        playerInputActions.Player.Hold.performed -= holdTetriminoGroup;
        playerInputActions.UI.Pause.performed -= onPause;
    }
    // Update is called once per frame
    void Update()
    {
        updateGridBitFlags();
    }
    private void OnApplicationQuit()
    {
        save();
    }
    private void SpawnTetriminoGroup(int k)
    {
        bool isGameOver = false;
        for (int i = 3; i < 6; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                if (gridBitFlags[i, j] == true)
                    isGameOver = true;
            }
        }
        TetriminoGroup group = Instantiate(TetriminoGroupPrefabs[k], spawnLocation, Quaternion.identity).GetComponent<TetriminoGroup>();
        foreach (Tetrimino tetri in group.tetriminos)
        {
            tetris.Add(tetri);
        }
        group.movespeed += (scoreManager.getLevel() - 1) / 2f;
        currentTetriminoGroupNum = k;
        activeTetriminoGroup = group;
        if (isGameOver)
            gameOver();
    }
    private void resetGridBitFlags()
    {
        for(int i = 0; i < 10; i++)
        {
            for(int j = 0; j < 20; j++)
            {
                gridBitFlags[i, j] = false;
            }
        }
    }
    private void updateGridBitFlags()
    {
        resetGridBitFlags();
        foreach (Tetrimino tetri in tetris)
        {
            gridBitFlags[tetri.col - 1, tetri.row - 1] = true;
        }
    }
    private void onTetriminoGroupDead()
    {
        alreadyHeld = false;
        reParentTetris();
        checkLines();
        if(activeTetriminoGroup != null)
            Destroy(activeTetriminoGroup.gameObject);
        if (debugTetriminoGroup)
            SpawnTetriminoGroup(debugTetriminoGroupNum);
        else
        {
            SpawnTetriminoGroup(upcomingTetriminoGroups.Dequeue());
            updateUpcomingStack();
        }
    }

    public bool canIMoveLeft(int[] rows, int[] cols)
    {
        for(int i = 0; i < rows.Length; i++)
        {
            if (cols[i] < 2)
                return false;
            else if (gridBitFlags[cols[i] - 2, rows[i] -1] && !checkBlockerInSameGroup(cols[i] - 1, rows[i], rows, cols))
                    return false;
        }
        return true;
    }
    public bool canIMoveRight(int[] rows, int[] cols)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            if (cols[i] > 9)
                return false;
            else if (gridBitFlags[cols[i], rows[i] - 1] && !checkBlockerInSameGroup(cols[i]+1, rows[i], rows, cols))
                return false;
        }
        return true;
    }
    public bool canIMoveDown(int[] rows, int[] cols)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] > 19)
                return false;
            else if (gridBitFlags[cols[i] - 1, rows[i]])
            {
                if(!checkBlockerInSameGroup(cols[i], rows[i] + 1, rows, cols))
                {
                    //Debug.Log("Blocker not in same group at row: " + (rows[i] + 1) + " col: " + cols[i] + "tetriminos: " + activeTetriminoGroup.tetriminos);
                    return false;
                }
            }
        }
        return true;
    }
    public bool canIRotate(int[] rows, int[] cols, int[] curRows, int[] curCols)
    {
        for(int i = 0; i < rows.Length; i++)
        {
            if (rows[i] < 1 || rows[i] > 20 || cols[i] < 1 || cols[i] > 10)
            {
                return false;
            }
            if (gridBitFlags[cols[i] - 1, rows[i] - 1] && !checkBlockerInSameGroup(cols[i], rows[i], curRows, curCols))
            {
                return false;
            }
        }
        return true;
    }
    private bool checkBlockerInSameGroup(int testCol, int testRow, int[] rows, int[] cols)
    {
        for(int i = 0; i < cols.Length; i++)
        {
            if (testCol == cols[i] && testRow == rows[i])
                return true;
        }
        return false;
    }
    public int getFastDropCoordinates()
    {
        int numRowsToDrop = 0;
        int[] cols = new int[activeTetriminoGroup.tetriminos.Count];
        int[] rows = new int[activeTetriminoGroup.tetriminos.Count];
        for(int i = 0; i < activeTetriminoGroup.tetriminos.Count; i++)
        {
            cols[i] = ((Tetrimino)activeTetriminoGroup.tetriminos[i]).col;    
            rows[i] = ((Tetrimino)activeTetriminoGroup.tetriminos[i]).row;
        }

        bool canMoveDown = true;
        
        while (canMoveDown)
        {
            canMoveDown = canIMoveDown(rows, cols);
            if (canMoveDown)
            {
                numRowsToDrop++;
                for(int i = 0; i < rows.Length; i++)
                {
                    rows[i]++;
                }
            }
        }
        if (numRowsToDrop > 0)
            scoreManager.hardDrop(numRowsToDrop);
        return numRowsToDrop;
    }
    private void checkLines()
    {
        bool[] fullLines = new bool[20];
        int numFullLines = 0;
        for (int i = 0; i < 20; i++)
        {
            bool lineIsFull = true;
            for (int j = 0; j < 10; j++)
            {
                if(!gridBitFlags[j, i])
                {
                    lineIsFull = false;
                    break;
                }    
            }
            if (lineIsFull)
            {
                fullLines[i] = true;
                numFullLines++;
            }
        }
        if (numFullLines > 0)
        {
            for(int i = 0; i < 20; i++)
            {
                if (fullLines[i])
                {
                    Tetrimino[] tempToDelete = new Tetrimino[tetris.Count];
                    tetris.CopyTo(tempToDelete);
                    tetris.RemoveAll(s => s.row == i + 1);
                    for(int j = 0; j < tempToDelete.Length; j++)
                    {
                        if(tempToDelete[j] != null)
                            if (tempToDelete[j].row == i + 1)
                                flickerTetri(tempToDelete[j].gameObject.transform.GetComponent<SpriteRenderer>());
                    }
                    foreach(Tetrimino tetri in tetris)
                    {
                        if(tetri.row < i + 1)
                        {
                            tetri.row++;
                            tetri.gameObject.transform.position = new Vector3(tetri.gameObject.transform.position.x, tetri.gameObject.transform.position.y - 0.64f, 0);
                        }
                    }
                    updateGridBitFlags();
                }
            }
            audioSource.PlayOneShot(audioClips[0]);
            scoreManager.clearedLine(numFullLines);
        }
    }
    private void flickerTetri(SpriteRenderer tetriSR)
    {
        tetriSR.enabled = false;
        removeTetri(tetriSR.gameObject);
    }
    private void removeTetri(GameObject tetri)
    {
        Destroy(tetri);
    }
    private void reParentTetris()
    {
        if(activeTetriminoGroup != null)
            foreach(Tetrimino tetri in activeTetriminoGroup.tetriminos)
            {
                tetri.gameObject.transform.SetParent(deadTetriminos.transform);
            }
    }
    private Queue<int> initTetriminoGroupQueue()
    {
        Queue<int> ret = new Queue<int>();
        int[] nums = {0, 1, 2, 3, 4, 5, 6};
        for (int i = 0; i < 7; i++)
        {
            int rand = UnityEngine.Random.Range(i, 7);
            int temp = nums[i];
            nums[i] = nums[rand];
            nums[rand] = temp;
            ret.Enqueue(nums[i]);
        }
        return ret;
    }
    private void holdTetriminoGroup(InputAction.CallbackContext context)
    {
        // if just held a tetrimino group, return
        if (alreadyHeld)
            return;
        // if hold is empty, hold current tetrimino group and spawn the next one
        if (heldTetriminoGroup == -1)
        {
            heldTetriminoGroup = currentTetriminoGroupNum;
            spawnHeldTetriminoGroup();
            deleteCurrentTetriminoGroup();
            onTetriminoGroupDead();
            alreadyHeld = true;
        }
        // else delete current tetrimino group with its tetriminos and spawn one of the type in hold
        else
        {
            deleteCurrentTetriminoGroup();
            deleteHeldTetriminoGroup();
            spawnHeldTetriminoGroup();
            int temp = currentTetriminoGroupNum;
            SpawnTetriminoGroup(heldTetriminoGroup);
            heldTetriminoGroup = temp;
            alreadyHeld = true;
        }
    }
    private void deleteCurrentTetriminoGroup()
    {
        tetris.RemoveAll(s => activeTetriminoGroup.tetriminos.Contains(s));
        updateGridBitFlags();
        foreach(Tetrimino tetri in activeTetriminoGroup.tetriminos)
        {
            Destroy(tetri.gameObject);
        }
        Destroy(activeTetriminoGroup.gameObject);
        //activeTetriminoGroup = null;
    }
    // spawns the held tetrimino in the hold area
    private void spawnHeldTetriminoGroup()
    {
        heldTetriminoGroupTransform = Instantiate(TetriminoGroupPrefabs[currentTetriminoGroupNum], holdArea.transform);
        heldTetriminoGroupTransform.localPosition = new Vector3(-0.32f, 0.32f, 0);
        heldTetriminoGroupTransform.GetComponent<TetriminoGroup>().isHeld = true;
    }
    private void deleteHeldTetriminoGroup()
    {
        if(heldTetriminoGroupTransform != null)
        Destroy(heldTetriminoGroupTransform.gameObject);
    }
    private void initUpcomingQueue()
    {
        Vector3 spawnLocation = new Vector3(0, 4.16f, 0);
        int[] first4 = upcomingTetriminoGroups.ToArray();
        for(int i = 0; i < 4; i++)
        {
            Transform temp = Instantiate(TetriminoGroupPrefabs[first4[i]], stackBackground.transform);
            temp.GetComponent<TetriminoGroup>().isHeld = true;
            temp.localPosition = spawnLocation;
            spawnLocation = new Vector3(0, spawnLocation.y - 2.56f, 0);
            upcomingTetriminoGroupTransforms.Enqueue(temp);
        }
    }
    private void updateUpcomingStack()
    {
        Destroy(upcomingTetriminoGroupTransforms.Dequeue().gameObject);
        foreach(Transform tf in upcomingTetriminoGroupTransforms)
        {
            tf.localPosition = new Vector3(0, tf.localPosition.y + 2.56f, 0);
        }
        Transform temp = Instantiate(TetriminoGroupPrefabs[upcomingTetriminoGroups.ToArray()[nextInQueue]], stackBackground.transform);
        temp.GetComponent<TetriminoGroup>().isHeld = true;
        temp.localPosition = new Vector3(0, -3.52f, 0);
        upcomingTetriminoGroupTransforms.Enqueue(temp);
        Queue<int> nextUpcomingTetriminoGroups = initTetriminoGroupQueue();
        if (upcomingTetriminoGroups.Count == 4)
        {
            for(int i = 0; i < 7; i++)
            {
                upcomingTetriminoGroups.Enqueue(nextUpcomingTetriminoGroups.Dequeue());
            }
        }
    }
    public void triggerSoftDrop()
    {
        scoreManager.softDrop();
    }
    public bool getIsPaused()
    {
        return isPaused;
    }
    private void onPause(InputAction.CallbackContext context)
    {
        if(isPaused)
        {
            Time.timeScale = 1;
            pauseMenu.SetActive(false);
        }
        else
        {
            Time.timeScale = 0;
            pauseMenu.SetActive(true);
        }
        isPaused = !isPaused;
    }
    public void unPause()
    {
        if (!isPaused)
            return;
        else
        {
            Time.timeScale = 1;
            pauseMenu.SetActive(false);
            isPaused = !isPaused;
        }
    }
    public void restart()
    {
        SceneManager.LoadScene("MainScene");
    }
    public void quit()
    {
        Application.Quit();
    }
    public void playLandSound()
    {
        audioSource.PlayOneShot(audioClips[1]);
    }
    private void gameOver()
    {
        //Debug.Log("GameOver");
        isPaused = true;
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
    }
    private void save()
    {
        if (PlayerPrefs.HasKey("highScore"))
        {
            if(PlayerPrefs.GetInt("highScore") < scoreManager.getHighScore())
                PlayerPrefs.SetInt("highScore", scoreManager.getHighScore());
        }
        else
            PlayerPrefs.SetInt("highScore", scoreManager.getHighScore());
    }
    private void load()
    {
        scoreManager.loadHighScore();
    }
    private void debugUpcomingStack()
    {
        string print = "";
        foreach(int i in upcomingTetriminoGroups)
        {
            print += i.ToString();
            print += " ";
        }
        Debug.Log(print);
    }
}
