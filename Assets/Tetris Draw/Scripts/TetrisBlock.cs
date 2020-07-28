using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBlockHolder : MonoBehaviour
{
    Vector3 velocity = Vector3.zero;
    UIManager uIManager;
    LevelManager levelManager;
    float acc = +7f;
    public static float InitialAcceleration; 
    public static float JerkMultiplier;

    public GridTile CollidingTile;

    public bool isFree;

    int[] min_y;
    int absolute_min_y;


    void Awake()
    {
        min_y = new int[SpaceConversionUtility.ScreenWidthInBlocks];
        for (int i = 0; i < min_y[i]; i++) min_y[i] = int.MaxValue;
        for (int i = 0; i < transform.childCount; i++)
        {
            TetrisBlock tb = transform.GetChild(i).GetComponent<TetrisBlock>();
            min_y[tb.Coordx] = Mathf.Min(min_y[tb.Coordx], tb.localCoordy);
        }
        absolute_min_y = Mathf.Min(min_y);
        acc = InitialAcceleration;



    }
    void Start()
    {
        uIManager = FindObjectOfType<UIManager>();
        levelManager = FindObjectOfType<LevelManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFree || transform.position.y < -100) return;
        bool cont = true;
        Vector3 newpos = transform.position + (velocity * Time.deltaTime);
        float dist = float.MaxValue;
        TetrisBlock selected = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            TetrisBlock tb = transform.GetChild(i).GetComponent<TetrisBlock>();

            if ((tb.transform.localPosition + newpos - SpaceConversionUtility.UpDir).y < levelManager.TopPositionPerColumn[tb.Coordx].y)
            {
                float newdist = (tb.transform.localPosition + newpos - SpaceConversionUtility.UpDir).y - levelManager.TopPositionPerColumn[tb.Coordx].y;
                if (dist > newdist)
                {
                    selected = tb;
                    dist = newdist;
                    cont = false;
                }
            }
        }
        if (cont)
        {
            velocity.y -= acc * Time.deltaTime;
            acc = acc + acc * (JerkMultiplier * Time.deltaTime);
            transform.position = newpos;
        }
        else
        {
            SnapAndJoin(selected);
        }
    }

    void SnapAndJoin(TetrisBlock tetrisBlock)
    {
        //SNAP
        isFree = false;
        int yForTB = levelManager.TopGridIndexPerColumn[tetrisBlock.Coordx] + 1;
        Vector3 newPosforTB = SpaceConversionUtility.GridSpaceToWorldSpace(new Vector2(tetrisBlock.Coordx, yForTB));
        Vector3 disp = newPosforTB - tetrisBlock.transform.position;
        transform.position = transform.position + disp;


        //JOIN:
        int added_rows = 0;
        //HOW MANY ROWS TO ADD
        for (int i = transform.childCount - 1; i >= 0; i--)
        {

            TetrisBlock tb = transform.GetChild(i).GetComponent<TetrisBlock>();

            int y_difference = (tetrisBlock.localCoordy - tb.localCoordy); //upwards (+tetris)
            int newrow = levelManager.TopGridIndexPerColumn[tb.Coordx] + y_difference;
            int last_count = levelManager.Levels.Count;
            int j;
            for (j = 0; j <= newrow - last_count + 1; j++)
            {
                List<LevelBlock> lbs = new List<LevelBlock>();
                for (int k = 0; k < SpaceConversionUtility.ScreenWidthInBlocks; k++)
                {
                    lbs.Add(null);
                }
                levelManager.Levels.Insert(0, lbs);
            }
            added_rows = Mathf.Max(added_rows, j);
        }

        LevelBlock tetrisLevelBlock = tetrisBlock.gameObject.AddComponent<LevelBlock>();
        tetrisLevelBlock.Coordx = tetrisBlock.Coordx;
        int rowofTB = levelManager.Levels.Count - yForTB - 1;
        levelManager.Levels[rowofTB][tetrisBlock.Coordx] = tetrisLevelBlock;
        tetrisBlock.transform.SetParent(levelManager.LevelHolder, true);
        //tetrisBlock.GetComponentInChildren<MeshRenderer>().material.color = Color.green;

        //REGISTER TO LEVEL MANAGER
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            TetrisBlock tb = transform.GetChild(i).GetComponent<TetrisBlock>();
            if (tb == tetrisBlock) continue;
            LevelBlock lb = tb.gameObject.AddComponent<LevelBlock>();
            lb.Coordx = tb.Coordx;
            int y_difference = (tetrisBlock.localCoordy - tb.localCoordy); //upwards (+tetris)
      //      Debug.Log("tes:" + (rowofTB - (y_difference)));
            levelManager.Levels[rowofTB-(y_difference)][tb.Coordx] = lb;
            tb.transform.SetParent(levelManager.LevelHolder, true);
            Destroy(tb);
        }
        Destroy(tetrisBlock);
        Destroy(gameObject);

        levelManager.ReCalcTopPos();
        levelManager.CheckLevelCompletionDelayed();
    }

}

public class TetrisBlock : MonoBehaviour
{
    public int Coordx;
    public int localCoordy;
}
