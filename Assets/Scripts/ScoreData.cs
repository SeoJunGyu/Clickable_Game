using System;
using UnityEngine;

//타임 스탬프를 찍어 json으로 만드는 클래스다.
public class ScoreData
{
    public int score;
    public long timestamp;

    public ScoreData()
    {

    }

    public ScoreData(int score, long timestamp)
    {
        this.score = score;
        this.timestamp = timestamp;
    }

    public DateTime GetDataTime()
    {
        //DateTimeOffset -> 타임 스탬프에 찍힌 지역의 시간으로 반환되는 함수다.
        return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
    }

    public string GetDateString()
    {
        //yyyy-mm-dd HH:mm:ss -> 이런 형식으로 반환된다.
        return GetDataTime().ToString("yyyy-mm-dd HH:mm:ss");
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
