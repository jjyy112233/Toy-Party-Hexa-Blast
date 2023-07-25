using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Block : MonoBehaviour
{
    public SpriteRenderer blockImage;
  
    private int id;
    private Vector3Int pos = new Vector3Int();
    public Vector3Int Pos
    {
        get
        {
            return pos;
        }
        set
        {
            pos = value;
        }
    }
    public Vector3Int Up => new Vector3Int(Pos.x, Pos.y - 2);
    public Vector3Int DOWN => new Vector3Int(Pos.x, Pos.y + 2);
    public Vector3Int LEFT_UP => new Vector3Int(Pos.x - 1, Pos.y - 1);
    public Vector3Int LEFT_DOWN => new Vector3Int(Pos.x - 1, Pos.y + 1);
    public Vector3Int RIGHT_UP => new Vector3Int(Pos.x + 1, Pos.y - 1);
    public Vector3Int RIGHT_DOWN => new Vector3Int(Pos.x + 1, Pos.y + 1);

    public int ID
    {
        get { return id; }
        set { id = value; }
    }

    bool isEmpty;
    public bool IsEmpty
    {
        get
        {
            return isEmpty;
        }
        set
        {
            isEmpty = value;
        }
    }

    bool isUse;
    public bool IsUse
    {
        get
        {
            return isUse;
        }
        set
        {
            isUse = value;
        }

    }
    bool isMove;
    public bool IsMove
    {
        get
        {
            return isMove;
        }
        set
        {
            isMove = value;
        }
    }

    public void FirstBlockInit()
    {
        blockImage.color = Color.clear;
        id = Random.Range(0, Board.blockCount);
    }

    public void Setting(int id, int x, int y)
    {
        blockImage.sprite = Resources.Load<Sprite>($"blocks/{id.ToString()}");
        ID = id;
        Pos = new Vector3Int(x, y, 0);
        IsEmpty = false;
        blockImage.color = Color.white;
        IsUse = false;
    }
    public void Setting(int id)
    {
        blockImage.sprite = Resources.Load<Sprite>($"blocks/{id.ToString()}");
        ID = id;
        IsEmpty = false;
        blockImage.color = Color.white;
        IsUse = false;
    }

    public bool CheckBreak(List<Block> breakBlockList)
    {
        if (Board.SpecialBlock(ID))
        {
            return false;
        }

        int up_down;
        List<Block> breakBlockList_up_down = new List<Block>();
        int leftUp_rightDown;
        List<Block> breakBlockList_leftUp_rightDown = new List<Block>();
        int leftDown_rightUp;
        List<Block> breakBlockList_leftDown_rightUp = new List<Block>();

        up_down = CheckNext(Board.NextBlock.UP, breakBlockList_up_down) + CheckNext(Board.NextBlock.DOWN, breakBlockList_up_down);
        leftUp_rightDown = CheckNext(Board.NextBlock.LEFT_UP, breakBlockList_leftUp_rightDown)
            + CheckNext(Board.NextBlock.RIGHT_DOWN, breakBlockList_leftUp_rightDown);
        leftDown_rightUp = CheckNext(Board.NextBlock.LEFT_DOWN, breakBlockList_leftDown_rightUp) 
            + CheckNext(Board.NextBlock.RIGHT_UP, breakBlockList_leftDown_rightUp);

        if (up_down + 1 >= 3)
            breakBlockList.AddRange(breakBlockList_up_down);
        if (leftUp_rightDown + 1 >= 3)
            breakBlockList.AddRange(breakBlockList_leftUp_rightDown);
        if (leftDown_rightUp + 1 >= 3)
            breakBlockList.AddRange(breakBlockList_leftDown_rightUp);

        return (up_down + 1 >= 3) || (leftUp_rightDown + 1 >= 3) || (leftDown_rightUp + 1 >= 3);
    }

    public int CheckNext(Board.NextBlock nextBlock, List<Block> breakBlockList, int count = 0)
    {
        var nextPos = Pos;
        switch (nextBlock)
        {
            case Board.NextBlock.UP:
                nextPos = Up;
                break;
            case Board.NextBlock.DOWN:
                nextPos = DOWN;
                break;
            case Board.NextBlock.LEFT_UP:
                nextPos = LEFT_UP;
                break;
            case Board.NextBlock.LEFT_DOWN:
                nextPos = LEFT_DOWN;
                break;
            case Board.NextBlock.RIGHT_UP:
                nextPos = RIGHT_UP;
                break;
            case Board.NextBlock.RIGHT_DOWN:
                nextPos = RIGHT_DOWN;
                break;
        }

        if (GameManager.instance.CheckBlock(ID, nextPos))
        {
            count++;
            var _nextBlock = GameManager.instance.GetBlock(nextPos);
            if (_nextBlock != null)
            {
                count = _nextBlock.CheckNext(nextBlock, breakBlockList, count);
                breakBlockList.Add(_nextBlock);
            }
        }
        return count;
    }

    public void SetEmpty()
    {
        blockImage.color = Color.clear;
        IsEmpty = true;
    }

    [SerializeField]
    int touchCount = 0;
    public virtual void TouchBlock()
    {
        if (ID != GameManager.instance.GetSpinTopBlockNumber)
            return;

        if (IsEmpty)
            return;

        touchCount++;

        if (touchCount >= 3)
        {
            SetEmpty();
        }
    }
}
