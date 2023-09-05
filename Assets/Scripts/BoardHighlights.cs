using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHighlights : MonoBehaviour
{

    public static BoardHighlights Instance { set; get; }
    [SerializeField] GameObject chessPieceHighlightPrefab;
    public GameObject highlightPrefab;
    private List<GameObject> highlights;
    

    private void Start()
    {
        Instance = this;
        highlights = new List<GameObject>();
    }

    private GameObject GetHighLightObject(bool chessPiece=false)
    {
        if(chessPiece)
        {
            var go2 = Instantiate(chessPieceHighlightPrefab);
            highlights.Add(go2);
            return go2;
        }
        GameObject go = highlights.Find(g => !g.activeSelf);

        if (go == null)
        {
            go = Instantiate(highlightPrefab);
            highlights.Add(go);
        }

        return go;
    }

    public void HighLightAllowedMoves(bool[,] moves,int x,int y)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (BoardManager.Instance.CheckForAllowedMove(i, j, x, y, BoardManager.Instance.Chessmans[x,y]))
                {
                    GameObject go = GetHighLightObject();
                    go.SetActive(true);
                    go.transform.position = new Vector3(i + 0.5f, 0.0001f, j + 0.5f);
                }
            }

        }
    }
    public void HighLightAllowedChessman()
    {
        foreach (var chessman in BoardManager.Instance.Chessmans)
        {
            if (chessman == null) continue;
            if (chessman.isWhite != BoardManager.Instance.isWhiteTurn) continue;
            var possibleMoves = chessman.PossibleMoves();
            for (int i=0;i<8;i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (!possibleMoves[i, j]) continue;
                    if (BoardManager.Instance.CheckForAllowedMove(i, j, chessman.CurrentX, chessman.CurrentY, chessman))
                    {
                        GameObject go = GetHighLightObject(true);
                        go.SetActive(true);
                        go.transform.position = new Vector3(chessman.CurrentX + 0.5f, 0.0001f, chessman.CurrentY + 0.5f);
                    }
                }
            }
        }
    }

    public void HideHighlights()
    {
        foreach (GameObject go in highlights)
            go.SetActive(false);
    }
}
