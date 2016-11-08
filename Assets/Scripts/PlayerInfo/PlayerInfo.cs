using UnityEngine;
using System.Collections;

public class PlayerInfo {

    public playerAtt CurAtt;

	// Use this for initialization
    public PlayerInfo()
    {

        CurAtt = new playerAtt();

	}
	
	public static PlayerInfo Instance()
    {
        return SingletonObject<PlayerInfo>.GetInstance();
    }
}


public class playerAtt
{

    private int _initGold = 1000;
    public bool FirstLogin
    {
        get
        {
            return !PlayerPrefs.HasKey("FirstLogin");
        }
        set
        {
            PlayerPrefs.SetInt("FirstLogin",1);
        }
    }
    public int Gold
    {
        get
        {
            return PlayerPrefs.GetInt("Gold", _initGold);
        }
        set
        {
            PlayerPrefs.SetInt("Gold", value);
        }
    }

    public string Date
    {
        get
        {
            return PlayerPrefs.GetString("Date", "-1");
        }
        set
        {
            PlayerPrefs.SetString("Date", value);
        }
    }


    public int Level
    {
        get
        {
            return PlayerPrefs.GetInt("Level", 0);
        }
        set
        {
            PlayerPrefs.SetInt("Level", value);
        }
    }


    public void Save()
    {
        PlayerPrefs.Save();
    }

}