using UnityEngine;
using System.Collections.Generic;
using ScoreCfg;

public enum SumDirection
{
    Row1=0,
    Row2,
    Row3,
    LeftUp2RightDown,
    Col1,
    Col2,
    Col3,
    LeftDown2RightUp,
}

public class LotPlayer {


    public const int Count = 9;

    public List<KeyValuePair<int, bool>> data = new List<KeyValuePair<int, bool>>();

    public List<TScoreCfg> sumScores;

    public int InitShowNum = 1;

    public int MaxShowNum = 4;

    public int CurShowNum;


    public LotPlayer()
    {
        CurShowNum = 0;
        sumScores = ScoreConfig.GetInst().GetConfigs();
        InitLotData();
        InitShow();
    }


    public static LotPlayer Instance()
    {
        return SingletonObject<LotPlayer>.GetInstance();
    }


    public void InitLotData()
    {
        data.Clear();
        data.Add(new KeyValuePair<int,bool>(1,false));
        for (int i = 1; i < Count; i++)
        {
            int index = Random.Range(0,i+1);
            data.Insert(index, new KeyValuePair<int, bool>(i+1, false));
        }
    }

    public void InitShow()
    {
        for (int i = 0; i < InitShowNum; i++)
        {
            while(true)
            {
                int index = Random.Range(0, 9);
                if(!data[index].Value)
                {
                    data[index] = new KeyValuePair<int, bool>(data[index].Key,true);
                    CurShowNum++;
                    break;
                }
            }
        }
    }


    public void Refresh()
    {
        CurShowNum = 0;
        InitLotData();
        InitShow();
    }


    public int ShowNumAtIndex(int index)
    {
        if (!data[index].Value)
        {
            data[index] = new KeyValuePair<int, bool>(data[index].Key, true);
            CurShowNum++;
        }
        return data[index].Key;
    }

    public bool MeetMaxNum()
    {
        return CurShowNum >= MaxShowNum;
    }


    public int GetSum(SumDirection direction)
    {
        int sum = 0;
        int index1=0;
        int index2=1;
        int index3=2;
        switch (direction)
        {
            case SumDirection.Row1:
                break;
            case SumDirection.Row2:
                index1 = 3;
                index2 = 4;
                index3 = 5;
                break;
            case SumDirection.Row3:
                index1 = 6;
                index2 = 7;
                index3 = 8;
                break;
            case SumDirection.LeftUp2RightDown:
                index1 = 0;
                index2 = 4;
                index3 = 8;
                break;
            case SumDirection.Col1:
                index1 = 0;
                index2 = 3;
                index3 = 6;
                break;
            case SumDirection.Col2:
                index1 = 1;
                index2 = 4;
                index3 = 7;
                break;
            case SumDirection.Col3:
                index1 = 2;
                index2 = 5;
                index3 = 8;
                break;
            case SumDirection.LeftDown2RightUp:
                index1 = 2;
                index2 = 4;
                index3 = 6;
                break;
            default:
                break;
        }
        sum = data[index1].Key + data[index2].Key + data[index3].Key;
        return sum;
    }


    public int GetScore(int sum)
    {
        foreach (var item in sumScores)
        {
            if (item.Sum==sum)
            {
                return item.Score;
            }
        }
        return -1;
    }
}
