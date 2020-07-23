﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Start is called before the first frame update
    int level = 0;
    public GameObject BlockPrefab;
    public float newLevelTimeInSeconds;
    public float HoleProbability;
    public float IncreaseInVerticalProbability;
    public float IncreaseInHorizontalProbability;
    float lastTime = -1f;
    public Transform LevelHolder;
    public Vector3[] TopPositionPerColumn;
    public int[] TopGridIndexPerColumn;

    UIManager uIManager;
    public List<List<LevelBlock>> Levels;

    private void Awake()
    {
        uIManager = FindObjectOfType<UIManager>();
        GameObject LevelHolderGO = new GameObject("LevelHolder");
        LevelHolder = LevelHolderGO.transform;
        prev = new List<int>(0);
        Levels = new List<List<LevelBlock>>(SpaceConversionUtility.ScreenHeightInBlocks);
    }

    LevelBlock GenerateBlock(Vector2Int Coords)
    {
        GameObject newBlock = (Instantiate(BlockPrefab, SpaceConversionUtility.GridSpaceToWorldSpace(Coords), Quaternion.identity));
        newBlock.transform.SetParent(LevelHolder, true);

        LevelBlock lb = newBlock.AddComponent<LevelBlock>();
        lb.Coordx = Coords.x;
        TopPositionPerColumn = new Vector3[SpaceConversionUtility.ScreenWidthInBlocks];
        TopGridIndexPerColumn = new int[SpaceConversionUtility.ScreenWidthInBlocks];
        return lb;
    }


    //REMEMBER TO OPTIMIZE?
    public void CheckLevelCompletion()
    {
        int destroyed = 0;
        for (int i = Levels.Count - 1; i >= 0; i--)
        {
            for(int m = 0 ; m < destroyed; m++)
            for(int k = 0; k < Levels[i].Count; k++)
            {
                if(Levels[i][k]!=null)                
                Levels[i][k].transform.position -= SpaceConversionUtility.UpDir;
            }

            if (!Levels[i].Contains(null))
            {destroyed++;
                for(int j = 0; j < Levels[i].Count; j++)
                {
                    Destroy(Levels[i][j].gameObject);
                }
            Levels.RemoveAt(i);
            level--;
            }
        }

    }

    public override string ToString()
    {
        List<string> a = new List<string>();
        a.Add("\n");
        for(int i = 0; i < Levels.Count; i++)
        {
            for(int j = 0; j < Levels[i].Count ;j++)
            {
                a.Add((Levels[i][j]==null) ? "N" : "D");
            }
            a.Add("\n");
        }
        return string.Concat(a.ToArray());

    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying || Time.frameCount < 10) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < TopPositionPerColumn.Length; i++)
        {
            Gizmos.DrawWireCube(TopPositionPerColumn[i], 1.1f * SpaceConversionUtility.BlockWidth * Vector3.one);
        }
    }

    List<int> prev;
    public List<LevelBlock> CreateNewLevel()
    {
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
    //    Debug.Log(this);
    }

    public void AddLevel()
    {
       Levels.Add(CreateNewLevel());
        ReCalcTopPos();
    }
    private void Update()
    {
        if (Time.time - lastTime >= newLevelTimeInSeconds)
        {
            lastTime = Time.time;
            AddLevel();
        }
    }
}