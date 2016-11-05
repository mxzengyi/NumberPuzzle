using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LotViewController : MonoBehaviour {

    public LotView View;

    private PlayerInfo _playerInfo;

    private LotPlayer _model;

    IEnumerator Start()
    {
        yield return MainConfigManager.GetInstance().Initialize();
        _playerInfo = PlayerInfo.Instance();
        _model = LotPlayer.Instance();
        View.Initionlize();
        View.InitByData(_playerInfo, _model);
    }

    public void OnNumClick(Button btn)
    {
        if (!_model.MeetMaxNum())
        {
            int index = View.GetNumIndex(btn);
            int num = _model.ShowNumAtIndex(index);
            View.ShowNum(index, num.ToString());
        }

        if (_model.MeetMaxNum())
        {
            View.ShowMessage("已经点击" + _model.MaxShowNum+"次，现在可以点击方向按钮获得积分了！");
        }
    }

    public void OnDirectionClick(Button btn)
    {
        if (_model.MeetMaxNum())
        {
            SumDirection direction = View.GetSumDirection(btn);
            int sum = _model.GetSum(direction);
            int score = _model.GetScore(sum);

            View.ShowMessage("Sum="+sum+",可获得" + score + "点积分！");


            View.SetCurScore(_playerInfo.CurAtt.Gold + score);

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
            },4.0f));
        }
    }
}
