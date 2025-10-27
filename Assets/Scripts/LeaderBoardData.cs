using System;
using UnityEngine;

public class LeaderBoardData
{
    public int score;
    public string userName;

    public LeaderBoardData()
    {

    }

    public LeaderBoardData(int score, string nickName)
    {
        this.score = score;
        this.userName = nickName;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static LeaderBoardData FromJson(string data)
    {
        return JsonUtility.FromJson<LeaderBoardData>(data);
    }
}
