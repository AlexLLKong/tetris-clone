using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class TetriminoGroup : MonoBehaviour
{
    public Transform TetriminoPrefab;
    internal List<Transform> tetriTransforms;
    public float movespeed=1;
    private int frameCount=0;
    public List<Tetrimino> tetriminos;
    public bool canMoveDown = true;
    public bool canMoveRight = true;
    public bool canMoveLeft = true;
    public bool isDead = false;
    public bool isHeld = false;
    public int maxCol;
    public int minCol;
    public int maxRow;
    public int minRow;
    public int[,,] rotations;
    public int currentRotation = 0;
    private PlayerInputActions playerInputActions;
    public Game gameManager;
    private Coroutine deathTimerCoroutine;
    public int[] cols;
    public int[] rows;
    private bool fastDropped = false;
    private float deathWaitTime = 1f;
    private float leftHeldDown = 0f;
    private float rightHeldDown = 0f;
    private float dasThreshold = 0.1f;
    protected virtual void Awake()
    {
        tetriminos = new List<Tetrimino>();
        gameManager = FindObjectOfType<Game>();
        playerInputActions = new PlayerInputActions();
        rotations = new int[4, 4, 2];
        InstantiateTetriminos();
        cols = new int[tetriminos.Count];
        rows = new int[tetriminos.Count];
    }
    protected virtual void OnEnable()
    {
        playerInputActions.Enable();
        playerInputActions.Player.ClockwiseRotate.performed += RotateClockwise;
        playerInputActions.Player.CounterClockwiseRotate.performed += RotateCounterClockwise;
    }
    protected virtual void OnDisable()
    {
        playerInputActions.Player.ClockwiseRotate.performed -= RotateClockwise;
        playerInputActions.Player.CounterClockwiseRotate.performed -= RotateCounterClockwise;
    }
    void Start()
    {
  
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (gameManager.getIsPaused())
            return;
        if (isHeld)
        {
            playerInputActions.Player.ClockwiseRotate.performed -= RotateClockwise;
            playerInputActions.Player.CounterClockwiseRotate.performed -= RotateCounterClockwise;
            return;
        }
        getCols();
        getRows();
        Vector2 move = playerInputActions.Player.Move.ReadValue<Vector2>();        
        if (!isDead)
        {
            if (move.x > 0)
            {
                leftHeldDown = 0;
                if (rightHeldDown == 0 || (rightHeldDown > dasThreshold && frameCount % 3 == 0 ))
                {
                    moveRight();
                    rightHeldDown += Time.deltaTime;
                }
                else
                    rightHeldDown += Time.deltaTime;
            }
            else if (move.x < 0)
            {
                rightHeldDown = 0;
                if (leftHeldDown == 0 || leftHeldDown > dasThreshold && frameCount % 3 == 0)
                {
                    moveLeft();
                    leftHeldDown += Time.deltaTime;
                }
                else
                    leftHeldDown += Time.deltaTime;
            }
            else if (move.y < 0)
                moveDown(true);
            if (move.x == 0)
            {
                leftHeldDown = 0;
                rightHeldDown = 0;
            }
        }
        // Assume max framerate of 60 which is set in Game class
        if (frameCount >= 60 / movespeed)
        {
            frameCount = 0;
            moveDown(false);
        }
     
        float dropInput = playerInputActions.Player.FastDrop.ReadValue<float>();
        if (dropInput == 1f && canMoveDown)
            fastDrop();
        if(!isDead)
            checkForDeath();

        //debugLogColsRows();
        frameCount++;
    }
    protected virtual void InstantiateTetriminos()
    {
    }
    public virtual void RotateClockwise(InputAction.CallbackContext context)
    {
        if (isDead)
            return;
        int[] testRows = new int[3];
        int[] testCols = new int[3];
        bool normalRotate = true;
        for (int i = 0; i < 3; i++)
        {
            testRows[i] = tetriminos[i].row + rotations[i, currentRotation, 0];
            testCols[i] = tetriminos[i].col + rotations[i, currentRotation, 1];
        }
        if (gameManager.canIRotate(testRows, testCols, rows, cols))
        {
            performClockwiseRotation();
            postRotateChecks();
        }
        else
        {
            normalRotate = false;
        }
        for (int i = 0; i < 3; i++)
        {
            testCols[i]--;
        }
        if (!normalRotate && gameManager.canIRotate(testRows, testCols, rows, cols))
        {
            moveLeft();
            performClockwiseRotation();
            postRotateChecks();
            return;
        }
        for (int i = 0; i < 3; i++)
        {
            testCols[i] = testCols[i] + 2;
        }
        if (!normalRotate && gameManager.canIRotate(testRows, testCols, rows, cols))
        {
            moveRight();
            performClockwiseRotation();
            postRotateChecks();
        }
    }
    protected virtual void performClockwiseRotation()
    {
        for (int i = 0; i < 3; i++)
        {
            int addRow = rotations[i, currentRotation, 0];
            int addCol = rotations[i, currentRotation, 1];
            tetriminos[i].row += addRow;
            tetriminos[i].col += addCol;
            tetriTransforms[i].localPosition = new Vector3(
                tetriTransforms[i].localPosition.x + (0.64f * addCol),
                tetriTransforms[i].localPosition.y + (-0.64f * addRow),
                0);
        }
        if (currentRotation < 3)
            currentRotation++;
        else
            currentRotation = 0;
    }
    public virtual void RotateCounterClockwise(InputAction.CallbackContext context)
    {
        if (isDead)
            return;
        int prevRotation = currentRotation - 1 < 0 ? 3 : currentRotation - 1;
        int[] testRows = new int[3];
        int[] testCols = new int[3];
        bool normalRotate = true;
        for (int i = 0; i < 3; i++)
        {
            testRows[i] = tetriminos[i].row - rotations[i, prevRotation, 0];
            testCols[i] = tetriminos[i].col - rotations[i, prevRotation, 1];
        }
        if (gameManager.canIRotate(testRows, testCols, rows, cols))
        {
            performCounterClockwiseRotation();
            postRotateChecks();
        }
        else
        {
            normalRotate = false;
        }
        for (int i = 0; i < 3; i++)
        {
            testCols[i]--;
        }
        if (!normalRotate && gameManager.canIRotate(testRows, testCols, rows, cols))
        {
            moveLeft();
            performCounterClockwiseRotation();
            postRotateChecks();
            return;
        }
        for (int i = 0; i < 3; i++)
        {
            testCols[i] = testCols[i] + 2;
        }
        if (!normalRotate && gameManager.canIRotate(testRows, testCols, rows, cols))
        {
            moveRight();
            performCounterClockwiseRotation();
            postRotateChecks();
        }
    }
    protected virtual void performCounterClockwiseRotation()
    {
        int prevRotation = currentRotation - 1 < 0 ? 3 : currentRotation - 1;
        for (int i = 0; i < 3; i++)
        {
            int addRow = rotations[i, prevRotation, 0];
            int addCol = rotations[i, prevRotation, 1];
            tetriminos[i].row -= addRow;
            tetriminos[i].col -= addCol;
            tetriTransforms[i].localPosition = new Vector3(
                tetriTransforms[i].localPosition.x - (0.64f * addCol),
                tetriTransforms[i].localPosition.y - (-0.64f * addRow),
                0);
        }
        if (currentRotation == 0)
            currentRotation = 3;
        else
            currentRotation--;
    }
    protected void postRotateChecks()
    {
        checkCols();
        checkRows();
        checkForDeath();
    }
    public void moveLeft()
    {
        if (minCol > 1 && gameManager.canIMoveLeft(rows, cols) && !fastDropped)
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x - 0.64f, gameObject.transform.position.y, 0);
            decrementTetriminoCols();
            checkCols();
            checkIfCanMoveDown();
        }
    }
    public void moveRight()
    {
        if (maxCol < 10 && gameManager.canIMoveRight(rows, cols) && !fastDropped)
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x + 0.64f, gameObject.transform.position.y, 0);
            incrementTetriminoCols();
            checkCols();
            checkIfCanMoveDown();
        }
    }
    public void moveDown(bool isSoftDrop)
    {
        getCols();
        getRows();
        if (maxRow < 20 && gameManager.canIMoveDown(rows, cols))
        {
            canMoveDown = true;
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 0.64f, 0);
            incrementTetriminoRows();
            checkRows();
            if (isSoftDrop)
                gameManager.triggerSoftDrop();
        }
        else
            canMoveDown = false;
    }
    private void fastDrop()
    {
        getCols();
        getRows();
        int numRowsToDrop = gameManager.getFastDropCoordinates();
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - (numRowsToDrop * 0.64f), 0);
        foreach (Tetrimino tetri in tetriminos)
        {
            tetri.row += numRowsToDrop;
        }
        deathWaitTime = 0.1f;
        canMoveDown = false;
        fastDropped = true;
    }
    protected void incrementTetriminoRows()
    {
        foreach(Tetrimino tetri in tetriminos)
        {
            tetri.row += 1;
        }
    }
    protected void incrementTetriminoCols()
    {
        foreach (Tetrimino tetri in tetriminos)
        {
            tetri.col += 1;
        }
    }
    protected void decrementTetriminoCols()
    {
        foreach (Tetrimino tetri in tetriminos)
        {
            tetri.col -= 1;
        }
    }
    private void checkCols()
    {
        int tempMaxCol = 0;
        int tempMinCol = 9;
        foreach (Tetrimino tetri in tetriminos)
        {
            if (tetri.col > tempMaxCol)
                tempMaxCol = tetri.col;
            if (tetri.col < tempMinCol)
                tempMinCol = tetri.col;
        }
        maxCol = tempMaxCol;
        minCol = tempMinCol;
    }
    private void checkRows()
    {
        foreach (Tetrimino tetri in tetriminos)
        {
            if (tetri.row> maxRow)
                maxRow = tetri.row;
            else if (tetri.row < minRow)
                minRow = tetri.row;
        }
    }
    private void checkForDeath()
    {
        checkIfCanMoveDown();
        if (!canMoveDown && deathTimerCoroutine == null)
        {
            deathTimerCoroutine = StartCoroutine(deathTimer());
        }
        else if (canMoveDown && deathTimerCoroutine != null)
        {
            //Debug.Log("Death timer cancelled");
            StopCoroutine(deathTimerCoroutine);
            deathTimerCoroutine = null;
        }
    }
    private IEnumerator deathTimer()
    {
        //Debug.Log("Death timer started");
        yield return new WaitForSecondsRealtime(deathWaitTime);
        //Debug.Log("Death timer finished");
        isDead = true;
        gameManager.playLandSound();
        gameManager.TetriminoGroupDead();
    }
    private void getRows()
    {
        for(int i = 0; i < tetriminos.Count; i++)
        {
            rows[i] = ((Tetrimino)tetriminos[i]).row;
        }
    }
    private void getCols()
    {
        for (int i = 0; i < tetriminos.Count; i++)
        {
            cols[i] = ((Tetrimino)tetriminos[i]).col;
        }
    }
    private void checkIfCanMoveDown()
    {
        getCols();
        getRows();
        if (maxRow < 20 && gameManager.canIMoveDown(rows, cols))
        {
            canMoveDown = true;
        }
        else
        {
            canMoveDown = false;
        }
    }
    private void debugLogColsRows()
    {
        foreach(Tetrimino tetri in tetriminos)
        {
            Debug.Log("row: " + tetri.row + "minRow: " + minRow + "maxRow: " + maxRow + "col: " + tetri.col + "minCol: " + minCol + "maxCol: " + maxCol);
        }
    }
}
