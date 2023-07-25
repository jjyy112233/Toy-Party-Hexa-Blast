using UnityEngine;

public class BlockFrame : MonoBehaviour
{
    public SpriteRenderer frameImage;
    public Block block;
    int ID;

    public void Setting(int id, int x, int y)
    {
        transform.position = new Vector3(x * 0.59f, y * -0.33f);
        block.Setting(id,x,y);
        ID = id;
    }
}
