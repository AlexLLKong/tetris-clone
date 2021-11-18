using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotTetriminoGroup : TetriminoGroup
{
    protected override void Awake()
    {
        base.Awake();
    }
    void Start()
    {
        maxRow = 1;
        minRow = 1;
        minCol = 5;
        maxCol = 5;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void InstantiateTetriminos()
    {
        Transform tetriTransform;
        Tetrimino tetri;
        tetriTransform = Instantiate(TetriminoPrefab, gameObject.transform);
        tetriTransform.localPosition = Vector3.zero;
        tetriminos.Add(tetriTransform.GetComponent<Tetrimino>());
        tetri = (Tetrimino)tetriminos[0];
        tetri.col = 5;
        tetri.row = 1;
        for (int i = 0; i < tetriminos.Count; i++)
        {
            tetriminos[i] = tetriTransform.GetComponent<Tetrimino>();
        }
    }
}
