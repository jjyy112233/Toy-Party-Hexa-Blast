using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Board;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Board board;
    public static GameManager instance;

    public Block GetBlock(int x, int y) => board.GetBlock(x, y);
    public Block GetBlock(Vector3Int pos) => board.GetBlock(pos.x, pos.y);
    public Vector3Int GetNewBlockPos => board.GetNewBlockPos;
    public Block GetNewBlockFrame => board.GetNewBlockFrame.block;
    public int GetSpinTopBlockNumber
    {
        get
        {
            return spintTopNumber;
        }
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        board.Init();  
    }

    public bool CheckBlock(int id, int x, int y)
    {
        var block = board.GetBlock(x, y);
        if(block != null)
        {
            return block.ID == id;
        }
        return false;
    }
    public bool CheckBlock(int id, Vector3Int pos)
    {
        var block = board.GetBlock(pos.x, pos.y);
        if (block != null)
        {
            return block.ID == id;
        }
        return false;
    }
    public bool CheckBlock(Vector3Int pos)
    {
        var block = board.GetBlock(pos.x, pos.y);
        if (block != null)
        {
            return true;
        }
        return false;
    }
}
