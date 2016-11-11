using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LotViewController : MonoBehaviour {

    public LotView View;

    public IAPController IAPUI;

    private PlayerInfo _playerInfo;

    private LotPlayer _model;

    IEnumerator Start()
    {
        yield return MainConfigManager.GetInstance().Initialize();
        _playerInfo = PlayerInfo.Instance();
        _model = LotPlayer.Instance();
        View.Initionlize();
        View.InitByData(_playerInfo, _model);
        int cost = _model.GetCurrentTurnGameCost(false);
        if (cost != 0)
        {
            View.ShowCostPage(cost);
        }
        else
        {
            _model.GetCurrentTurnGameCost(true);
        }
    }

    public void OnNumClick(Button btn)
    {
        if (!_model.MeetMaxNum())
        {
            int index = View.GetNumIndex(btn);
            int num = _model.ShowNumAtIndex(index);
            View.ShowNum(index, num.ToString());
        }

        //if (_model.MeetMaxNum())
        //{
        //    View.ShowMessage("已经点击" + _model.MaxShowNum+"次，现在可以点击方向按钮获得积分了！");
        //}
    }

    public void OnDirectionClick(Button btn)
    {
        if (_model.MeetMaxNum())
        {
            SumDirection direction = View.GetSumDirection(btn);
            int sum = _model.GetSum(direction);
            int score = _model.GetScore(sum);

            if (sum == 6)
            {
                View.ShowMessage("<color=#D1FF00>Congratulation!Luck 6! </color>"); 
                View.ShowLuckySixEffect();
            }
            else
            {
                if (score > 1000)
                {
                    View.ShowMessage("<color=#D1FF00>Great!Greedy is Good!</color>");
                    View.ShowGreatEffect();
                }
                else
                {
                    View.ShowMessage("<color=#D1FF00>What a pity!Come on!</color>");
                }
                
            }


            View.SetCurScore(_playerInfo.CurAtt.Gold + score);
            View.HightLightScore(sum-6);

            //show all
            for (int i = 0; i < LotPlayer.Count; i++)
            {
                int num = _model.ShowNumAtIndex(i);
                View.ShowNum(i, num.ToString());
            }

            StartCoroutine(TimeHelper.DelayExecuteMethod(()=>
            {
                _model.Refresh();
                View.InitByData(_playerInfo, _model);
                View.ShowCostPage(_model.GetCurrentTurnGameCost(false));
                View.HideLuckySixEffect();
                View.HideGreatEffect();
            },4.0f));
        }
    }


    public void SetCurScore(int score)
    {
        View.SetCurScore(score);
    }


    public void OnTutorialClick(GameObject root)
    {
        root.SetActive(false);
    }


    public void OnCostPageClick()
    {
        if (_playerInfo.CurAtt.Gold >= _model.GetCurrentTurnGameCost(false))
        {
            View.SetCurScore(_playerInfo.CurAtt.Gold - _model.GetCurrentTurnGameCost(true));
            View.HideCostPage();
        }
        else
        {
            View.ShowCostNotEnough();
        }
    }


    public void ShowIAP()
    {
        IAPUI.Show();
    }

    public void OnCostNotEnoughClick()
    {
        if (_playerInfo.CurAtt.Gold >= _model.GetCurrentTurnGameCost(false))
        {
            View.SetCurScore(_playerInfo.CurAtt.Gold - _model.GetCurrentTurnGameCost(true));
            View.HideCostPage();
        }
        else
        {
            ShowIAP();
        }
    }
}
