using System;
using System.Security;
using UnityEngine;

[Serializable]
public class UserProfile
{
    public string nickname;
    public string email;
    public long createdAt; //긴 이름

    public UserProfile()
    {

    }

    public UserProfile(string nickname, string email)
    {
        this.nickname = nickname;
        this.email = email;
        this.createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); //UtcNow : 런던시간 / ToUnixTimeSeconds : 유니티 시간으로 변환 함수
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static UserProfile FromJson(string json)
    {
        return JsonUtility.FromJson<UserProfile>(json);
    }
}
