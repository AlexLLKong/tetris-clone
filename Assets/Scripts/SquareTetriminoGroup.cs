using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareTetriminoGroup : TetriminoGroup
{
    protected override void Awake()
    {
        tetriTransforms = new List<Transform>();
        base.Awake();
    }
    void Start()
    {
        maxRow = 2;
        minRow = 1;
        minCol = 5;
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
        // upper left block
        tempTetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tempTetriTransform.localPosition = Vector3.zero;
        tetriTransforms.Add(tempTetriTransform);
        tetri = tempTetriTransform.GetComponent<Tetrimino>();
        tetri.col = 5;
        tetri.row = 1;
        tetriminos.Add(tetri);

        // upper right block
        tempTetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tempTetriTransform.localPosition = new Vector3(0.64f,0,0);
        tetriTransforms.Add(tempTetriTransform);
        tetri = tempTetriTransform.GetComponent<Tetrimino>();
        tetri.col = 6;
        tetri.row = 1;
        tetriminos.Add(tetri);

        // lower left block
        tempTetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tempTetriTransform.localPosition = new Vector3(0, -0.64f, 0);
        tetriTransforms.Add(tempTetriTransform);
        tetri = tempTetriTransform.GetComponent<Tetrimino>();
        tetri.col = 5;
        tetri.row = 2;
        tetriminos.Add(tetri);

        // lower right block
        tempTetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tempTetriTransform.localPosition = new Vector3(0.64f, -0.64f, 0);
        tetriTransforms.Add(tempTetriTransform);
        tetri = tempTetriTransform.GetComponent<Tetrimino>();
        tetri.col = 6;
        tetri.row = 2;
        tetriminos.Add(tetri);
        
    }
}
