﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] Transform CameraPos1,CameraPos2;
    public static BoardManager Instance { get; set; }
    private bool[,] allowedMoves { get; set; }

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;

    private Quaternion whiteOrientation = Quaternion.Euler(0, 270, 0);
    private Quaternion blackOrientation = Quaternion.Euler(0, 90, 0);

    public Chessman[,] Chessmans { get; set; }
    private Chessman selectedChessman;

    public bool isWhiteTurn = false;

    private Material previousMat;
    public Material selectedMat;

    public int[] EnPassantMove { set; get; }

    System.Action OnTurnChange;
    // Use this for initialization
    void Start()
    {
        Instance = this;
        SpawnAllChessmans();
        EnPassantMove = new int[2] { -1, -1 };
        OnTurnChange += TurnChange;
        OnTurnChange?.Invoke();
    }
    public bool CheckForAllowedMove(int x, int y, int posx, int posy,Chessman type=null)
    {
        var pureX = Mathf.Abs(x - posx);
        var pureY = Mathf.Abs(y - posy);
        int singleIn,singleOut;
        if (pureX < pureY)
        {
            singleIn = pureY - pureX;
            singleOut = pureX;
        }
        else
        {
            singleIn = pureX-pureY;
            singleOut = pureY;
        }
        if (type != null)
        {
            if (type.isWhite == isWhiteTurn)
            {
                if (type is Knight)
                {
                    singleIn++;
                }
            }
        }
        return allowedMoves[x, y] &&
            (singleIn+singleOut == DiceBehaviour.Instance.Dice1 || singleIn + singleOut == DiceBehaviour.Instance.Dice2);
    }
    bool CheckForSelectionPossibility()
    {
        bool found = false;
        foreach(var chessman in Chessmans)
        {
            if (chessman == null) continue;
            if (chessman.isWhite != isWhiteTurn) continue;
            if (CheckForChessmanMovePossibility(chessman.CurrentX,chessman.CurrentY))
            {
                BoardHighlights.Instance.HighLightAllowedChessman();
                found = true;
            }
        }
        return found;
    }
    bool CheckForChessmanMovePossibility(int x,int y)
    {
        bool hasAtLeastOneMove = false;
        allowedMoves = Chessmans[x, y].PossibleMoves();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (CheckForAllowedMove(i, j, x, y, Chessmans[x, y]))
                {
                    hasAtLeastOneMove = true;
                    i = 8;
                    break;
                }
            }
        }
        return hasAtLeastOneMove;
    }
    // Update is called once per frame
    void Update()
    {
        if (DiceBehaviour.Instance.Locked) return;
        UpdateSelection();

        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedChessman == null)
                {
                    // Select the chessman
                    SelectChessman(selectionX, selectionY);
                }
                else
                {
                    // Move the chessman
                    MoveChessman(selectionX, selectionY);
                }
            }
        }

        if (Input.GetKey("escape"))
            Application.Quit();
    }

    private void SelectChessman(int x, int y)
    {
        if (Chessmans[x, y] == null) return;

        if (Chessmans[x, y].isWhite != isWhiteTurn) return;

        bool hasAtLeastOneMove = false;

        allowedMoves = Chessmans[x, y].PossibleMoves();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (CheckForAllowedMove(i,j,x,y, Chessmans[x, y]))
                {
                    hasAtLeastOneMove = true;
                    i = 8;
                    break;
                }
            }
        }

        if (!hasAtLeastOneMove)
        {
            Debug.LogError("No allowed move");
            return;
        }

        selectedChessman = Chessmans[x, y];
        previousMat = selectedChessman.GetComponent<MeshRenderer>().material;
        selectedMat.mainTexture = previousMat.mainTexture;
        selectedChessman.GetComponent<MeshRenderer>().material = selectedMat;

        BoardHighlights.Instance.HighLightAllowedMoves(allowedMoves,x,y);
    }

    private void MoveChessman(int x, int y)
    {
        if (CheckForAllowedMove(x,y,selectedChessman.CurrentX,selectedChessman.CurrentY, selectedChessman))
        {
            Chessman c = Chessmans[x, y];

            if (c != null && c.isWhite != isWhiteTurn)
            {
                // Capture a piece

                if (c.GetType() == typeof(King))
                {
                    // End the game
                    EndGame();
                    return;
                }

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            if (x == EnPassantMove[0] && y == EnPassantMove[1])
            {
                if (isWhiteTurn)
                    c = Chessmans[x, y - 1];
                else
                    c = Chessmans[x, y + 1];

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            EnPassantMove[0] = -1;
            EnPassantMove[1] = -1;
            if (selectedChessman.GetType() == typeof(Pawn))
            {
                if(y == 7) // White Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(1, x, y, true);
                    selectedChessman = Chessmans[x, y];
                }
                else if (y == 0) // Black Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(7, x, y, false);
                    selectedChessman = Chessmans[x, y];
                }
                EnPassantMove[0] = x;
                if (selectedChessman.CurrentY == 1 && y == 3)
                    EnPassantMove[1] = y - 1;
                else if (selectedChessman.CurrentY == 6 && y == 4)
                    EnPassantMove[1] = y + 1;
            }

            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;
            OnTurnChange?.Invoke();
        }

        selectedChessman.GetComponent<MeshRenderer>().material = previousMat;

        BoardHighlights.Instance.HideHighlights();
        selectedChessman = null;
    }
    public async void TurnChange()
    {
        await Task.Delay(1000);
        isWhiteTurn = !isWhiteTurn;
        if (isWhiteTurn)
        {
            Camera.main.transform.position = CameraPos1.position;
            Camera.main.transform.rotation = CameraPos1.rotation;
        }
        else
        {
            Camera.main.transform.position = CameraPos2.position;
            Camera.main.transform.rotation = CameraPos2.rotation;
        }
        var res=await DiceBehaviour.Instance.MakeRandomDice();
        if (res)
        {
            if (!CheckForSelectionPossibility())
            {
                await Task.Delay(2000);
                OnTurnChange?.Invoke();
            }
        }
    }
    private void UpdateSelection()
    {
        if (!Camera.main) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("ChessPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void SpawnChessman(int index, int x, int y, bool isWhite)
    {
        Vector3 position = GetTileCenter(x, y);
        GameObject go;

        if (isWhite)
        {
            go = Instantiate(chessmanPrefabs[index], position, whiteOrientation) as GameObject;
        }
        else
        {
            go = Instantiate(chessmanPrefabs[index], position, blackOrientation) as GameObject;
        }

        go.transform.SetParent(transform);
        Chessmans[x, y] = go.GetComponent<Chessman>();
        Chessmans[x, y].SetPosition(x, y);
        activeChessman.Add(go);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;

        return origin;
    }

    private void SpawnAllChessmans()
    {
        activeChessman = new List<GameObject>();
        Chessmans = new Chessman[8, 8];

        /////// White ///////

        // King
        SpawnChessman(0, 3, 0, true);

        // Queen
        SpawnChessman(1, 4, 0, true);

        // Rooks
        SpawnChessman(2, 0, 0, true);
        SpawnChessman(2, 7, 0, true);

        // Bishops
        SpawnChessman(3, 2, 0, true);
        SpawnChessman(3, 5, 0, true);

        // Knights
        SpawnChessman(4, 1, 0, true);
        SpawnChessman(4, 6, 0, true);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i, 1, true);
        }


        /////// Black ///////

        // King
        SpawnChessman(6, 4, 7, false);

        // Queen
        SpawnChessman(7, 3, 7, false);

        // Rooks
        SpawnChessman(8, 0, 7, false);
        SpawnChessman(8, 7, 7, false);

        // Bishops
        SpawnChessman(9, 2, 7, false);
        SpawnChessman(9, 5, 7, false);

        // Knights
        SpawnChessman(10, 1, 7, false);
        SpawnChessman(10, 6, 7, false);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(11, i, 6, false);
        }
    }

    private void EndGame()
    {
        if (isWhiteTurn)
            Debug.Log("White wins");
        else
            Debug.Log("Black wins");

        foreach (GameObject go in activeChessman)
        {
            Destroy(go);
        }

        isWhiteTurn = true;
        BoardHighlights.Instance.HideHighlights();
        SpawnAllChessmans();
    }
}


