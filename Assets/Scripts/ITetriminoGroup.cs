using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class ITetriminoGroup : TetriminoGroup
{
    protected override void Awake()
    {
        tetriTransforms = new List<Transform>();
        base.Awake();
        assignRotations();
    }
    void Start()
    {
        maxRow = 1;
        minRow = 1;
        minCol = 4;
        maxCol = 7;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void InstantiateTetriminos()
    {
        Transform tempTetriTransform = null;
        Tetrimino tetri;
        // far right block - A
        tempTetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tempTetriTransform.localPosition = new Vector3(1.28f, 0, 0);
        tetriTransforms.Add(tempTetriTransform);
        tetri = tempTetriTransform.GetComponent<Tetrimino>();
        tetri.col = 7;
        tetri.row = 1;
        tetri.rotLet = 'A';
        tetriminos.Add(tetri);

        // right middle block - B
        tempTetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tempTetriTransform.localPosition = new Vector3(0.64f, 0, 0);
        tetriTransforms.Add(tempTetriTransform);
        tetri = tempTetriTransform.GetComponent<Tetrimino>();
        tetri.col = 6;
        tetri.row = 1;
        tetri.rotLet = 'B';
        tetriminos.Add(tetri);

        // left middle block - C
        tempTetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tempTetriTransform.localPosition = new Vector3(0, 0, 0);
        tetriTransforms.Add(tempTetriTransform);
        tetri = tempTetriTransform.GetComponent<Tetrimino>();
        tetri.col = 5;
        tetri.row = 1;
        tetri.rotLet = 'C';
        tetriminos.Add(tetri);

        // lower middle block - D
        tempTetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tempTetriTransform.localPosition = new Vector3(-0.64f, 0, 0);
        tetriTransforms.Add(tempTetriTransform);
        tetri = tempTetriTransform.GetComponent<Tetrimino>();
        tetri.col = 4;
        tetri.row = 1;
        tetri.rotLet = 'D';
        tetriminos.Add(tetri);

    }
    public override void RotateClockwise(InputAction.CallbackContext context)
    {
        if (isDead)
            return;
        int[] testRows = new int[4];
        int[] testCols = new int[4];
        bool normalRotate = true;
        for (int i = 0; i < 4; i++)
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
        for (int i = 0; i < 4; i++)
        {
            testCols[i] -= 2;
        }
        if (!normalRotate && gameManager.canIRotate(testRows, testCols, rows, cols))
        {
            moveLeft();
            moveLeft();
            performClockwiseRotation();
            postRotateChecks();
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            testCols[i] = testCols[i] + 4;
        }
        if (!normalRotate && gameManager.canIRotate(testRows, testCols, rows, cols))
        {
            moveRight();
            moveRight();
            performClockwiseRotation();
            postRotateChecks();
        }
    }
    public override void RotateCounterClockwise(InputAction.CallbackContext context)
    {
        if (isDead)
            return;
        int prevRotation = currentRotation - 1 < 0 ? 3 : currentRotation - 1;
        int[] testRows = new int[4];
        int[] testCols = new int[4];
        bool normalRotate = true;
        for (int i = 0; i < 4; i++)
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
        for (int i = 0; i < 4; i++)
        {
            testCols[i] -= 2;
        }
        if (!normalRotate && gameManager.canIRotate(testRows, testCols, rows, cols))
        {
            moveLeft();
            moveLeft();
            performCounterClockwiseRotation();
            postRotateChecks();
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            testCols[i] = testCols[i] + 4;
        }
        if (!normalRotate && gameManager.canIRotate(testRows, testCols, rows, cols))
        {
            moveRight();
            moveRight();
            performCounterClockwiseRotation();
            postRotateChecks();
        }
    }
    protected override void performClockwiseRotation()
    {
        for (int i = 0; i < 4; i++)
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
    protected override void performCounterClockwiseRotation()
    {
        int prevRotation = currentRotation - 1 < 0 ? 3 : currentRotation - 1;
        for (int i = 0; i < 4; i++)
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
    public void performRotationWhenHeld()
    {
        performClockwiseRotation();
    }
    private void assignRotations()
    {
        // rotation A0
        rotations[0, 0, 0] = 2; // row
        rotations[0, 0, 1] = -1; // col
        // rotation A1
        rotations[0, 1, 0] = -1;
        rotations[0, 1, 1] = -2;
        // rotation A2
        rotations[0, 2, 0] = -2;
        rotations[0, 2, 1] = 1;
        // rotation A3
        rotations[0, 3, 0] = 1;
        rotations[0, 3, 1] = 2;

        // rotation B0
        rotations[1, 0, 0] = 1; // row
        rotations[1, 0, 1] = 0; // col
        // rotation B1
        rotations[1, 1, 0] = 0;
        rotations[1, 1, 1] = -1;
        // rotation B2
        rotations[1, 2, 0] = -1;
        rotations[1, 2, 1] = 0;
        // rotation B3
        rotations[1, 3, 0] = 0;
        rotations[1, 3, 1] = 1;

        // rotation C0
        rotations[2, 0, 0] = 0; // row
        rotations[2, 0, 1] = 1; // col
        // rotation C1
        rotations[2, 1, 0] = 1;
        rotations[2, 1, 1] = 0;
        // rotation C2
        rotations[2, 2, 0] = 0;
        rotations[2, 2, 1] = -1;
        // rotation C3
        rotations[2, 3, 0] = -1;
        rotations[2, 3, 1] = 0;

        // rotation D0
        rotations[3, 0, 0] = -1; // row
        rotations[3, 0, 1] = 2; // col
        // rotation D1
        rotations[3, 1, 0] = 2;
        rotations[3, 1, 1] = 1;
        // rotation D2
        rotations[3, 2, 0] = 1;
        rotations[3, 2, 1] = -2;
        // rotation D3
        rotations[3, 3, 0] = -2;
        rotations[3, 3, 1] = -1;
    }
}
