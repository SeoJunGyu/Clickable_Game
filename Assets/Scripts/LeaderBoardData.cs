using System;
using UnityEngine;

public class LeaderBoardData
{
    public int score;
    public string nickName;

    public LeaderBoardData()
    {

    }

    public LeaderBoardData(int score, string nickName)
    {
        this.score = score;
        this.nickName = nickName;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static ScoreData FromJson(string data)
    {
        return JsonUtility.FromJson<ScoreData>(data);
    }
}
