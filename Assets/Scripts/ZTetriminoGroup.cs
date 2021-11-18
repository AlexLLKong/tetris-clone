using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class ZTetriminoGroup : TetriminoGroup
{
    protected override void Awake()
    {
        tetriTransforms = new List<Transform>();
        base.Awake();
        assignRotations();
    }
    void Start()
    {
        maxRow = 2;
        minRow = 1;
        minCol = 4;
        maxCol = 6;
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
        // upper left block - A
        tempTetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tempTetriTransform.localPosition = new Vector3(-0.64f, 0, 0);
        tetriTransforms.Add(tempTetriTransform);
        tetri = tempTetriTransform.GetComponent<Tetrimino>();
        tetri.col = 4;
        tetri.row = 1;
        tetri.rotLet = 'A';
        tetriminos.Add(tetri);

        // upper middle block - B
        tempTetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tempTetriTransform.localPosition = Vector3.zero;
        tetriTransforms.Add(tempTetriTransform);
        tetri = tempTetriTransform.GetComponent<Tetrimino>();
        tetri.col = 5;
        tetri.row = 1;
        tetri.rotLet = 'B';
        tetriminos.Add(tetri);
        
        // lower right block - C
        tempTetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tempTetriTransform.localPosition = new Vector3(0.64f, -0.64f, 0);
        tetriTransforms.Add(tempTetriTransform);
        tetri = tempTetriTransform.GetComponent<Tetrimino>();
        tetri.col = 6;
        tetri.row = 2;
        tetri.rotLet = 'C';
        tetriminos.Add(tetri);

        // lower middle block - X
        tempTetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tempTetriTransform.localPosition = new Vector3(0, -0.64f, 0);
        tetriTransforms.Add(tempTetriTransform);
        tetri = tempTetriTransform.GetComponent<Tetrimino>();
        tetri.col = 5;
        tetri.row = 2;
        tetri.rotLet = 'X';
        tetriminos.Add(tetri);

    }
    private void assignRotations()
    {
        // rotation A0
        rotations[0, 0, 0] = 0; // row
        rotations[0, 0, 1] = 2; // col
        // rotation A1
        rotations[0, 1, 0] = 2;
        rotations[0, 1, 1] = 0;
        // rotation A2
        rotations[0, 2, 0] = 0;
        rotations[0, 2, 1] = -2;
        // rotation A3
        rotations[0, 3, 0] = -2;
        rotations[0, 3, 1] = 0;

        // rotation B0
        rotations[1, 0, 0] = 1; // row
        rotations[1, 0, 1] = 1; // col
        // rotation B1
        rotations[1, 1, 0] = 1;
        rotations[1, 1, 1] = -1;
        // rotation B2
        rotations[1, 2, 0] = -1;
        rotations[1, 2, 1] = -1;
        // rotation B3
        rotations[1, 3, 0] = -1;
        rotations[1, 3, 1] = 1;

        // rotation C0
        rotations[2, 0, 0] = 1; // row
        rotations[2, 0, 1] = -1; // col
        // rotation C1
        rotations[2, 1, 0] = -1;
        rotations[2, 1, 1] = -1;
        // rotation C2
        rotations[2, 2, 0] = -1;
        rotations[2, 2, 1] = 1;
        // rotation C3
        rotations[2, 3, 0] = 1;
        rotations[2, 3, 1] = 1;

        // rotation X0
        rotations[3, 0, 0] = 0; // row
        rotations[3, 0, 1] = 0; // col
        // rotation X1
        rotations[3, 1, 0] = 0;
        rotations[3, 1, 1] = 0;
        // rotation X2
        rotations[3, 2, 0] = 0;
        rotations[3, 2, 1] = 0;
        // rotation X3
        rotations[3, 3, 0] = 0;
        rotations[3, 3, 1] = 0;
    }
}
