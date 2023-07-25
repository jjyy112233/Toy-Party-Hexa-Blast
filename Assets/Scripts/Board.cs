using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int blockCount = 6;
    public const int spintTopNumber = 6;

    public BlockFrame blockFrame;
    private string dataPath = "data";
    Camera cam;

    private Block firstBlock;
    private Block secondBlock;
    private bool isMoveBlock = false;
    private float swapDuration = 0.5f;
    private float downDuration = 0.5f;

    public enum NextBlock
    {
        NONE = -1, UP, DOWN, LEFT_UP, LEFT_DOWN, RIGHT_UP, RIGHT_DOWN, COUNT
    }
    Dictionary<int, Dictionary<int, BlockFrame>> boardBlocks = new Dictionary<int, Dictionary<int, BlockFrame>>();
    BlockFrame newBlockFrame = null;
    public BlockFrame GetNewBlockFrame
    {
        get
        {
            return newBlockFrame;
        }
    }

    public Vector3Int GetNewBlockPos
    {
        get
        {
            return newBlockFrame.block.Pos;
        }
    }

    private void Awake()
    {
        cam = Camera.main;
    }
    public void Init()
    {
        TextAsset textFile = Resources.Load(dataPath) as TextAsset;
        StringReader stringReader = new StringReader(textFile.text);

        while (true)
        {
            string line = stringReader.ReadLine();

            if (line == null)
                break;

            var data = line.Split(',');
            var x = int.Parse(data[0]);
            var y = int.Parse(data[1]);
            var t = int.Parse(data[2]);
            BlockFrame nowBlock = Instantiate(blockFrame, transform);


            nowBlock.Setting(t, x, y);
            if (!boardBlocks.ContainsKey(x))
            {
                boardBlocks[x] = new Dictionary<int, BlockFrame>();
            }

            if (newBlockFrame == null)
            {
                newBlockFrame = nowBlock;
                newBlockFrame.frameImage.color = Color.clear;
                newBlockFrame.block.blockImage.color = Color.clear;
                newBlockFrame.block.FirstBlockInit();
            }
            else
                boardBlocks[x][y] = nowBlock;

        }
    }
    void Update()
    {
        ListenInput();
    }
    private void ListenInput()
    {
        if (isMoveBlock)
            return;

        if (Input.touchCount > 0)
        {
            // 터치 입력 시,
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(touch.position), transform.forward, 1000f);
                if (hit)
                {
                    var block = hit.transform.GetComponent<Block>();
                    if (block != null)
                    {
                        firstBlock = block;
                        secondBlock = null;
                    }
                }
            }
            if (touch.phase == TouchPhase.Moved)
            {
                RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(touch.position), transform.forward, 1000f);
                if (hit)
                {
                    var block = hit.transform.GetComponent<Block>();
                    if (block != null && block != firstBlock && firstBlock != null && secondBlock == null)
                    {
                        secondBlock = block;
                        Debug.Log("move");
                        StartCoroutine(MoveBlock());
                    }
                }
            }
        }
    }
    private IEnumerator MoveBlock()
    {
        isMoveBlock = true;

        var firstPos = firstBlock.transform.position;
        var secondPos = secondBlock.transform.position;

        yield return StartCoroutine(SwapBlock(firstBlock, secondPos, secondBlock, firstPos));

        List<Block> breakBlockList = new List<Block>();
        var check1 = firstBlock.CheckBreak(breakBlockList);
        var check2 = secondBlock.CheckBreak(breakBlockList);

        if (!check1 && !check2)
            yield return StartCoroutine(SwapBlock(firstBlock, secondPos, secondBlock, firstPos));
        else
        {
            if (check1)
                breakBlockList.Add(firstBlock);
            if (check2)
                breakBlockList.Add(secondBlock);

            breakBlockList = breakBlockList.Distinct().ToList();
            yield return StartCoroutine(DownBlcok(breakBlockList));


        }

        isMoveBlock = false;

        firstBlock = null;
        secondBlock = null;
    }

    private IEnumerator SwapBlock(Block block1, Vector3 nextPos1, Block block2, Vector3 nextPos2)
    {
        var runTime = 0.0f;

        while (runTime < swapDuration)
        {
            runTime += Time.deltaTime;

            block1.transform.position = Vector3.Lerp(block1.transform.position, nextPos1, runTime / swapDuration);
            block2.transform.position = Vector3.Lerp(block2.transform.position, nextPos2, runTime / swapDuration);

            yield return null;
        }
        block1.transform.position = nextPos2;
        block2.transform.position = nextPos1;

        var tempID = block1.ID;
        block1.Setting(block2.ID);
        block2.Setting(tempID);
    }

    struct MoveBlockData
    {
        public Block upBlock;
        public Block downBlock;

        public int upBlockId;
        public int downBlockId;
        public Vector3 upPos;
        public Vector3 downPos;


        public MoveBlockData(Block UpB, Block DownB)
        {
            upBlock = UpB;
            downBlock = DownB;
            upBlockId = UpB.ID;
            downBlockId = DownB.ID;
            upPos = UpB.transform.position;
            downPos = DownB.transform.position;
        }
    }

    private IEnumerator DownBlcok(List<Block> breakBlockList)
    {
        foreach (var block in breakBlockList)
        {
            var joinBlocks = GetJoinBlockList(block, GameManager.instance.GetSpinTopBlockNumber);

            foreach (var b in joinBlocks)
            {
                b.TouchBlock();
            }

            block.SetEmpty();
        }

        List<Block> moveBlocks = new List<Block>();
        while (true)
        {
            List<MoveBlockData> moveBlockDatas = new List<MoveBlockData>();
            Dictionary<Block, Block> blockMap = new Dictionary<Block, Block>();
            foreach (var blocks in boardBlocks.Values)
            {
                foreach (var b in blocks.Values)
                {
                    if (b.block.IsEmpty)
                    {
                        GetUpToDownBlocks(b.block, ref blockMap); // Key : 위에 있는 블럭, Value : 아래에 있는 블럭

                        foreach (var block in blockMap)
                        {
                            if (block.Value != null)
                            {
                                moveBlockDatas.Add(new MoveBlockData(block.Key, block.Value));
                                if(!moveBlocks.Contains(block.Value))
                                    moveBlocks.Add(block.Value);
                            }
                        }
                    }
                }
            }

            if (moveBlockDatas.Count == 0)
            {
                breakBlockList = new List<Block>();
                foreach (var b in moveBlocks)
                {
                    if(b.CheckBreak(breakBlockList))
                        breakBlockList.Add(b);
                }

                moveBlocks.Clear();

                foreach (var block in breakBlockList)
                {
                    block.SetEmpty();
                }
                if (breakBlockList.Count == 0)
                    break;
                else
                    continue;
            }


            var runTime = 0.0f;

            foreach (var b in moveBlockDatas)
            {
                if (!GameManager.instance.CheckBlock(b.upBlock.Up))
                {
                    b.upBlock.SetEmpty();
                }
                b.downBlock.transform.position = b.upPos;
                b.downBlock.Setting(b.upBlockId);
            }

            while (runTime < downDuration)
            {
                runTime += Time.deltaTime;

                foreach (var b in moveBlockDatas)
                {
                    b.downBlock.transform.position = Vector3.Lerp(b.upPos, b.downPos, runTime / downDuration);
                }

                yield return null;
            }

            GameManager.instance.GetNewBlockFrame.FirstBlockInit();
            yield return null;
        }
    }
    public Block GetBlock(int x, int y)
    {
        if (boardBlocks.ContainsKey(x))
        {
            if (boardBlocks[x].ContainsKey(y))
                return boardBlocks[x][y].block;
        }

        return null;
    }

    public void GetUpToDownBlocks(Block checkBlock, ref Dictionary<Block, Block> blocks) // Key : 위에 있는 블럭, Value : 아래에 있는 블럭
    {
        while (checkBlock != null)
        {
            Vector3Int[] dirs;

            if (checkBlock.Pos.x < 0)
                dirs = new Vector3Int[2] { checkBlock.LEFT_UP, checkBlock.RIGHT_UP };
            else if (checkBlock.Pos.x > 0)
                dirs = new Vector3Int[2] { checkBlock.RIGHT_UP, checkBlock.LEFT_UP };
            else
            {
                int r = Random.Range(0, 2);
                dirs = r == 0 ? new Vector3Int[2] { checkBlock.LEFT_UP, checkBlock.RIGHT_UP } : new Vector3Int[2] { checkBlock.RIGHT_UP, checkBlock.LEFT_UP };
            }

            if (checkBlock.Up == GameManager.instance.GetNewBlockPos) // 가장 위에 있는 블럭이면
            {
                blocks[GameManager.instance.GetNewBlockFrame] = checkBlock; // 따로 저장한 값으로 설정
                GameManager.instance.GetNewBlockFrame.IsUse = true;
                break;
            }
            else if (GameManager.instance.CheckBlock(checkBlock.Up)) //위에 블럭이 있다면
            {
                var nextBlock = GameManager.instance.GetBlock(checkBlock.Up);
                if (nextBlock.IsEmpty || nextBlock.IsUse) //위에 블럭이 빈 블럭이거나 사용중인 블럭이라면
                {
                    checkBlock.SetEmpty(); // 아래 블럭은 내려올 블럭이 없으니 빈 블럭으로 설정
                    break;
                }

                blocks[nextBlock] = checkBlock; //위에 있는 블럭을 키로 아래있는 블럭을 값으로
                nextBlock.IsUse = true; //위에 있는 블럭은 사용하는 블럭으로 설정
                checkBlock = nextBlock; //다음 블럭
                continue;
            }
            else if (GameManager.instance.CheckBlock(dirs[0])) //위에 블럭이 없고 좌측 상단에 블럭이 있다면
            {
                var nextBlock = GameManager.instance.GetBlock(dirs[0]); //빈 블럭이 아닐 경우
                if (nextBlock.IsEmpty || nextBlock.IsUse)
                {
                    checkBlock.SetEmpty();
                    break;
                }

                blocks[nextBlock] = checkBlock;
                nextBlock.IsUse = true; //위에 있는 블럭은 사용하는 블럭으로 설정
                checkBlock = nextBlock;
                continue;
            }
            else if (GameManager.instance.CheckBlock(dirs[1])) //위에 블럭이 없고 좌측 상단에 블럭이 있다면
            {
                var nextBlock = GameManager.instance.GetBlock(dirs[1]); 
                if (nextBlock.IsEmpty || nextBlock.IsUse)
                {
                    checkBlock.SetEmpty();
                    break;
                }

                blocks[nextBlock] = checkBlock;
                nextBlock.IsUse = true; //위에 있는 블럭은 사용하는 블럭으로 설정
                checkBlock = nextBlock;
                continue;
            }

            checkBlock = null;
        }
    }

    public static bool SpecialBlock(int id)
    {
        switch (id)
        {
            case spintTopNumber:
                return true;
            default:
                return false;
        }
    }
    public List<Block> GetJoinBlockList(Block block, int findId)
    {
        var blockList = new List<Block>();

        if (block.ID == findId)
            return blockList;

        var UpBlock = GameManager.instance.GetBlock(block.Up);
        if (UpBlock != null && UpBlock.ID == findId) blockList.Add(UpBlock);

        var DownBlock = GameManager.instance.GetBlock(block.DOWN);
        if (DownBlock != null && DownBlock.ID == findId) blockList.Add(DownBlock);

        var LeftDownBlock = GameManager.instance.GetBlock(block.LEFT_DOWN);
        if (LeftDownBlock != null && LeftDownBlock.ID == findId) blockList.Add(LeftDownBlock);

        var LeftUpBlock = GameManager.instance.GetBlock(block.LEFT_UP);
        if (LeftUpBlock != null && LeftUpBlock.ID == findId) blockList.Add(LeftUpBlock);

        var RightDownBlock = GameManager.instance.GetBlock(block.RIGHT_DOWN);
        if (RightDownBlock != null && RightDownBlock.ID == findId) blockList.Add(RightDownBlock);

        var RightUpBlock = GameManager.instance.GetBlock(block.RIGHT_UP);
        if (RightUpBlock != null && RightUpBlock.ID == findId) blockList.Add(RightUpBlock);

        return blockList;
    }

}
