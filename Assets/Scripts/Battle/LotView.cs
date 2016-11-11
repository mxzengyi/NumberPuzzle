using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using ScoreCfg;

public class LotView : MonoBehaviour {

    public RectTransform NumRoot;

    public RectTransform DirectionRoot;

    public RectTransform ScoreRoot;

    public Text CurScore;

    public RectTransform TutorialRoot;

    public RectTransform CostPageRoot;

    public RectTransform CostRoot;

    public RectTransform CostNotEnoughRoot;

    public RectTransform HightLight;


    public ParticleSystem LuckySixEffect;

    public ParticleSystem GreatEffect;

    public Button[] Nums;

    public Button[] Directions;

    public RectTransform[] ScoreItems;


    private Text costText;

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


        if (PlayerInfo.Instance().CurAtt.FirstLogin)
        {
            TutorialRoot.gameObject.SetActive(true);
            PlayerInfo.Instance().CurAtt.FirstLogin = false;
        }

        LuckySixEffect.Clear();
        LuckySixEffect.Pause();
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
        if (Num.Equals("?"))
        {
            Nums[index].targetGraphic.color=Color.white;
        }
        else
        {
            Nums[index].targetGraphic.color = Color.green;
        }
        Text text = Nums[index].transform.FindChild("Text").GetComponent<Text>();
        text.text = Num;
    }


    public void ShowScore(int index, int Sum, int Score)
    {
        ScoreItems[index].transform.FindChild("Sum").GetComponent<Text>().text = Sum.ToString();
        ScoreItems[index].transform.FindChild("Score").GetComponent<Text>().text = Score.ToString();
    }


    public void HightLightScore(int index)
    {
        HightLight.gameObject.SetActive(true);
        HightLight.position = ScoreItems[index].transform.position;
        HightLight.localPosition = HightLight.localPosition + new Vector3(25, 0, 0);
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


    public void ShowCostPage(int cost)
    {
        CostPageRoot.gameObject.SetActive(true);
        CostRoot.gameObject.SetActive(true);
        if (costText==null)
        {
            costText = CostRoot.transform.FindChild("rules").GetComponent<Text>();
        }
        costText.text = "Next Game Turn Will Cost " + cost + " Scores.";

        CostNotEnoughRoot.gameObject.SetActive(false);
    }

    public void ShowCostNotEnough()
    {
        CostPageRoot.gameObject.SetActive(true);
        CostRoot.gameObject.SetActive(false);
        CostNotEnoughRoot.gameObject.SetActive(true);
    }


    public void HideCostPage()
    {
        CostPageRoot.gameObject.SetActive(false);
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

        if (Input.GetKey(KeyCode.A))
        {
            PlayerInfo.Instance().CurAtt.Date="-1";
        }
    }



    public void ShowLuckySixEffect()
    {
        LuckySixEffect.gameObject.SetActive(true);
        LuckySixEffect.Clear();
        LuckySixEffect.Play();
    }


    public void HideLuckySixEffect()
    {
        LuckySixEffect.Stop();
    }

    public void ShowGreatEffect()
    {
        GreatEffect.gameObject.SetActive(true);
        GreatEffect.Clear();
        GreatEffect.Play();
    }


    public void HideGreatEffect()
    {
        GreatEffect.Stop();
    }
}
