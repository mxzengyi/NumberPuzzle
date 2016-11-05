using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using ScoreCfg;

public class LotView : MonoBehaviour {

    public RectTransform NumRoot;

    public RectTransform DirectionRoot;

    public RectTransform ScoreRoot;

    public Text CurScore;


    public Button[] Nums;

    public Button[] Directions;

    public RectTransform[] ScoreItems;

    public Text MessageBox;

    public void Initionlize()
    {
        List<Button> btns = new List<Button>();

        int count = NumRoot.childCount;
        for (int i = 0; i < count; i++)
        {
            btns.Add(NumRoot.GetChild(i).GetComponent<Button>());
        }
        Nums = btns.ToArray();

        btns.Clear();
        count = DirectionRoot.childCount;
        for (int i = 0; i < count; i++)
        {
            btns.Add(DirectionRoot.GetChild(i).GetComponent<Button>());
        }
        Directions = btns.ToArray();

        List<RectTransform> trans = new List<RectTransform>();
        count = ScoreRoot.childCount;
        for (int i = 0; i < count; i++)
        {
            trans.Add(ScoreRoot.GetChild(i) as RectTransform);
        }
        ScoreItems = trans.ToArray();


        CurScore.text = PlayerInfo.Instance().CurAtt.Gold.ToString();
    }


    public void InitByData(PlayerInfo info, LotPlayer curPlayer)
    {
        int count = curPlayer.data.Count;
        for (int i = 0; i < count; i++)
        {
            if (curPlayer.data[i].Value)
            {
                ShowNum(i, curPlayer.data[i].Key.ToString());
            }
            else
            {
                ShowNum(i, "?");
            }
        }

        int index = 0;
        foreach (TScoreCfg item in curPlayer.sumScores)
        {
            ShowScore(index,item.Sum,item.Score);
            index++;
        }
    }

    public int GetNumIndex(Button btn)
    {
        int count = Nums.Length;
        for (int i = 0; i < count; i++)
        {
            if (Nums[i] == btn)
            {
                return i;
            }
        }
        return -1;
    }


    public SumDirection GetSumDirection(Button btn)
    {
        int count = Directions.Length;
        for (int i = 0; i < count; i++)
        {
            if (Directions[i] == btn)
            {
                return (SumDirection)i;
            }
        }
        return SumDirection.Col1;
    }

    public void ShowNum(int index, string Num)
    {
        Text text = Nums[index].transform.FindChild("Text").GetComponent<Text>();
        text.text = Num;
    }


    public void ShowScore(int index, int Sum, int Score)
    {
        ScoreItems[index].transform.FindChild("Sum").GetComponent<Text>().text = Sum.ToString();
        ScoreItems[index].transform.FindChild("Score").GetComponent<Text>().text = Score.ToString();
    }


    public void ShowMessage(string content)
    {
        GameObject message = Instantiate(MessageBox.gameObject) as GameObject;

        message.GetComponent<Text>().text = content;

        message.transform.parent = MessageBox.transform.parent;
        message.transform.localPosition = MessageBox.transform.localPosition;
        message.transform.localRotation = MessageBox.transform.localRotation;
        message.transform.localScale = MessageBox.transform.localScale;

        message.SetActive(true);
    }

    private bool ScoreChange = false;
    private int LastScore = 0;
    private float Duration = 0.5f;
    private float ScoreChangeProcess = 0;
    public void SetCurScore(int score)
    {
        if (PlayerInfo.Instance().CurAtt.Gold != score)
        {
            LastScore = PlayerInfo.Instance().CurAtt.Gold;
            PlayerInfo.Instance().CurAtt.Gold = score;
            ScoreChange = true;
            ScoreChangeProcess = 0;
        }
    }


    void Update()
    {
        if (ScoreChange)
        {
            int distance = PlayerInfo.Instance().CurAtt.Gold - LastScore;

            ScoreChangeProcess += (Time.deltaTime/Duration);

            if (ScoreChangeProcess>=1)
            {
                CurScore.text = PlayerInfo.Instance().CurAtt.Gold.ToString();
                ScoreChange = false;
            }
            else
            {
                CurScore.text = ((int)(ScoreChangeProcess*distance) + LastScore).ToString();
            }

        }


        if (Input.GetKey(KeyCode.F))
        {
            SetCurScore(1000);
        }
    }
}
