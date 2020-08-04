using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Start is called before the first frame update
    public int Stage = 0;
    public float Seed;
    int level = 0;
    public GameObject BlockPrefab;
    public float newLevelTimeInSeconds;
    public int StartingLevelsCount;
    float lastTime = -1f;
    [HideInInspector] public Transform LevelHolder;
    [HideInInspector] public Vector3[] TopPositionPerColumn;
    [HideInInspector] public int[] TopGridIndexPerColumn;

    public int MaxLevelsToGenerateInTotal;
    public int LoseLevels;

    public bool Tutorial;
    public GameObject[] TutorialObjectsEnabledPerLevel;

    UIManager uIManager;
    public List<List<LevelBlock>> Levels;

    int created = 0;
    GameManager gameManager;
    GridManager gridManager;


    [Header("Probabilities")]
    public float HoleProbability;
    public float IncreaseInVerticalProbability;
    public float IncreaseInHorizontalProbability;
    private void Awake()
    {
        Stage = PlayerPrefs.GetInt("Stage", Stage);
        if (Stage < 5 && SceneManager.GetActiveScene().buildIndex != 1) SceneManager.LoadScene(1);
        gridManager = FindObjectOfType<GridManager>();
        uIManager = FindObjectOfType<UIManager>();
        GameObject LevelHolderGO = new GameObject("LevelHolder");
        LevelHolder = LevelHolderGO.transform;
        gameManager = FindObjectOfType<GameManager>();
        uIManager.SetStage(Stage.ToString());
    }

    public void Initialize()
    {
        if (Tutorial)
        {
            foreach (var a in TutorialObjectsEnabledPerLevel)
            {
             a.SetActive(false);   
            }
            if (TutorialObjectsEnabledPerLevel != null && Stage < TutorialObjectsEnabledPerLevel.Length) 
            {
                TutorialObjectsEnabledPerLevel[Stage].SetActive(true);
            }
           /* else
            {
                for (int T = 0; T <= 3; T++)
                {
                    TutorialObjectsEnabledPerLevel[T].SetActive(false);
                }
            }*/
            if (Stage >= 5) { SceneManager.LoadScene(2); }
            created = 0;
            level = 0;
            for (int i = LevelHolder.childCount - 1; i >= 0; i--)
            {
                Destroy(LevelHolder.GetChild(i).gameObject);
            }
            Levels = new List<List<LevelBlock>>(4);
            MakeTutorial();
            MakeTutorial();
            if (Stage == 2) { MakeTutorial(); MakeTutorial(); }
            if (Stage == 4) { MakeTutorial(); MaxLevelsToGenerateInTotal = 8; LoseLevels = 6; uIManager.PositionLoseBar(); }
            Analytics.LogLevelStarted(Stage);
            gameManager.isPlaying = true;
            lastTime = Time.time;
            return;
        }

        Random.InitState((int)(Stage * Seed + Mathf.Pow(Stage, 2f) + Seed));

        if (Stage > 7)
        {
            MaxLevelsToGenerateInTotal = Mathf.Min(6 + Stage, 35);
            StartingLevelsCount = 5;
        }

        if (Stage >= 9 && Stage < 16)
        {
            gridManager.EnableAll();
            gridManager.DisabledTileCount = 1;
            gridManager.DisableRandomly();
        }

        if (Stage >= 16)
        {
            gridManager.EnableAll();
            gridManager.DisabledTileCount = 2;
            gridManager.DisableRandomly();
        }

        if (Stage >= 20)
        {
            newLevelTimeInSeconds = 4.5f;
        }


        //I'm bored and working at 6AM, so here's an easter-egg crazy level.
        if (Stage == 50)
        {
            uIManager.SetStage("!!!");
            LoseLevels = 8;
            uIManager.PositionLoseBar();
            StartingLevelsCount = 4;
            newLevelTimeInSeconds = 2.9f;
            gridManager.EnableAll();
            gridManager.DisabledTileCount = 3;
            gridManager.DisableRandomly();
        }

        prev = new List<int>(0);
        for (int i = LevelHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(LevelHolder.GetChild(i).gameObject);
        }
        Levels = new List<List<LevelBlock>>(SpaceConversionUtility.ScreenHeightInBlocks);
        created = 0;
        for (int i = 0; i < StartingLevelsCount; i++)
        {
            AddLevel();
            lastTime = Time.time;
        }
        Analytics.LogLevelStarted(Stage);
        gameManager.isPlaying = true;
    }

    LevelBlock GenerateBlock(Vector2Int Coords)
    {
        GameObject newBlock = (Instantiate(BlockPrefab, SpaceConversionUtility.GridSpaceToWorldSpace(Coords), Quaternion.identity));
        newBlock.transform.SetParent(LevelHolder, true);
        newBlock.GetComponentInChildren<BlockRandom>().Init();

        LevelBlock lb = newBlock.AddComponent<LevelBlock>();
        lb.Coordx = Coords.x;
        TopPositionPerColumn = new Vector3[SpaceConversionUtility.ScreenWidthInBlocks];
        TopGridIndexPerColumn = new int[SpaceConversionUtility.ScreenWidthInBlocks];
        return lb;
    }

    bool WillCheckForCompletion = false;
    public void CheckLevelCompletionDelayed(float delay)
    {
        if (WillCheckForCompletion) return;
        WillCheckForCompletion = true;
        Invoke("CheckLevelCompletion", delay);
    }

    //REMEMBER TO OPTIMIZE?
    public void CheckLevelCompletion()
    {
        int og_levelCount = Levels.Count;
        WillCheckForCompletion = false;
        if (!gameManager.isPlaying) return;
        int destroyed = 0;
        for (int i = Levels.Count - 1; i >= 0; i--)
        {
            int this_destroyed = 0;
            bool remove = false;


            if (!Levels[i].Contains(null))
            {
                this_destroyed++;
                for (int j = 0; j < Levels[i].Count; j++)
                {
                    LevelBlock tb = Levels[i][j];
                    tb.SetForDestructionDelayed(2f);
                    tb.transform.SetParent(null);
                    tb.PlayParticles();
                    //Destroy(tb.gameObject);
                }
                remove = true;
                level--;
            }
            if (!remove)
                for (int m = 0; m < destroyed; m++)
                    for (int k = 0; k < Levels[i].Count; k++)
                    {
                        if (Levels[i][k] != null)
                            Levels[i][k].transform.position -= SpaceConversionUtility.UpDir;
                    }
            destroyed += this_destroyed;
            if (remove) uIManager.AddExplositionFX(og_levelCount - i).SetActive(true);
            if (remove) Levels.RemoveAt(i);
        }
        if (destroyed > 0)
        {
            uIManager.ShakeItUp();
        }
        if (Levels.Count == 0 || LevelHolder.childCount == 0)
        {
            gameManager.isPlaying = false;
            Analytics.LogLevelSucceeded();
            PlayerPrefs.SetInt("Stage", Stage + 1);
            uIManager.Win();
        }
        ReCalcTopPos();

        if (Levels.Count > LoseLevels)
        {
            gameManager.isPlaying = false;
            Analytics.LogLevelFailed();
            uIManager.Lose();
        }

    }

    public override string ToString()
    {
        List<string> a = new List<string>();
        a.Add("\n");
        for (int i = 0; i < Levels.Count; i++)
        {
            for (int j = 0; j < Levels[i].Count; j++)
            {
                a.Add((Levels[i][j] == null) ? "N" : "D");
            }
            a.Add("\n");
        }
        return string.Concat(a.ToArray());

    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || Time.frameCount < 10) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < TopPositionPerColumn.Length; i++)
        {
            Gizmos.DrawWireCube(TopPositionPerColumn[i], 1.1f * SpaceConversionUtility.BlockWidth * Vector3.one);
        }
    }

    List<int> prev;
    public List<LevelBlock> CreateNewLevel()
    {
        created++;
        level++;
        List<LevelBlock> ThisLevel = new List<LevelBlock>(uIManager.ScreenWidthInBlocks);
        LevelHolder.position += SpaceConversionUtility.UpDir;
        List<int> this_round = new List<int>((int)(HoleProbability * uIManager.ScreenWidthInBlocks) + 1);
        int og_childindex = LevelHolder.childCount - 1;
        for (int i = 0; i < uIManager.ScreenWidthInBlocks; i++)
        {
            float rand = Random.Range(0f, 1f);
            if (prev.Contains(i)) rand -= IncreaseInVerticalProbability;
            if (this_round.Contains(i - 1)) rand -= IncreaseInHorizontalProbability;
            if (rand > HoleProbability)
            {
                ThisLevel.Add(GenerateBlock(new Vector2Int(i, 0)));
            }
            else
            {
                ThisLevel.Add(null);
                this_round.Add(i);
            }
        }
        if (this_round.Count == 0)
        {
            int prev_index;
            if (prev.Count > 0)
            {
                prev_index = prev[Random.Range(0, prev.Count)];
            }
            else
            {
                prev_index = Random.Range(0, uIManager.ScreenWidthInBlocks);
            }
            GameObject obj = ThisLevel[prev_index].gameObject;
            Destroy(obj);
            ThisLevel[prev_index] = null;
            this_round.Add(prev_index);
        }
        prev = this_round;
        return ThisLevel;
    }

    public List<LevelBlock> CreateNewTutorialLevel()
    {
        created++;
        level++;
        List<LevelBlock> ThisLevel = new List<LevelBlock>(uIManager.ScreenWidthInBlocks);
        LevelHolder.position += SpaceConversionUtility.UpDir;
        for (int i = 0; i < uIManager.ScreenWidthInBlocks; i++)
        {
            if (Stage == 0)
            {
                if (i == 0 || i == 1)
                {
                    ThisLevel.Add(null);
                    continue;
                }
            }
            else if (Stage == 1)
            {
                if (i == 3 || i == 4)
                {
                    ThisLevel.Add(null);
                    continue;
                }
            }
            else if (Stage == 2)
            {
                if (i == 2)
                {
                    ThisLevel.Add(null);
                    continue;
                }
            }
            else if (Stage == 3)
            {
                if (created == 1)
                {
                    if (i == 1)
                    {
                        ThisLevel.Add(null);
                        continue;
                    }
                }
                if (i == 2 || i == 3)
                {
                    ThisLevel.Add(null);
                    continue;
                }
            }
            else if (Stage == 4)
            {
                if (i == 1 || i == 2)
                {
                    ThisLevel.Add(null);
                    continue;
                }
            }

            ThisLevel.Add(GenerateBlock(new Vector2Int(i, 0)));
        }

        return ThisLevel;
    }

    public void ReCalcTopPos()
    {
        //I DON'T CARE LET'S RECALC EACH TIME
        for (int column = 0; column < SpaceConversionUtility.ScreenWidthInBlocks; column++)
        {
            for (int row = 0; row < Levels.Count; row++)
            {
                if (Levels[row][column] != null)
                {
                    int ind = Levels.Count - row - 1;
                    TopPositionPerColumn[column] = SpaceConversionUtility.GridSpaceToWorldSpace(new Vector2(column, ind));
                    TopGridIndexPerColumn[column] = ind;
                    break;
                }
                else if (row == 0)
                {
                    TopPositionPerColumn[column] = SpaceConversionUtility.GridSpaceToWorldSpace(new Vector2(column, -1));
                    TopGridIndexPerColumn[column] = -1;
                }

            }
        }
        for (int i = Levels.Count - 1; i >= 0; i--)
        {
            if (Levels[i].Count == 0) Levels.RemoveAt(i);
        }
        //    Debug.Log(this);
    }

    public void AddLevel()
    {
        Levels.Add(CreateNewLevel());
        ReCalcTopPos();
        uIManager.MoveSpawnerOnly();
        CheckLevelCompletionDelayed(gameManager.LoseCheckDelayAfterLevelAddition);
    }

    public void MakeTutorial()
    {
        Levels.Add(CreateNewTutorialLevel());
        ReCalcTopPos();
        uIManager.MoveSpawnerOnly();
        CheckLevelCompletionDelayed(gameManager.LoseCheckDelayAfterLevelAddition);
    }

    public void Restart()
    {
        uIManager.GameScreen();
        Initialize();
    }

    public void Next()
    {
        Stage++;
        uIManager.SetStage(Stage.ToString());
        uIManager.GameScreen();
        gridManager.ShuffleTileTextures();
        Initialize();
    }

    private void Update()
    {
        if (created < MaxLevelsToGenerateInTotal && gameManager.isPlaying)
            if (Time.time - lastTime >= newLevelTimeInSeconds)
            {
                lastTime = Time.time;
                if (!Tutorial) AddLevel();
                else MakeTutorial();
            }
        //        Debug.Log(Levels.Count);
    }
}
