using Cysharp.Threading.Tasks;
using Firebase.Database;
using System.Collections.Generic;
using UnityEngine;

public class LeaderBoardManager : MonoBehaviour
{
    private static LeaderBoardManager instance;
    public static LeaderBoardManager Instance => instance;

    //읽고 쓸때 요청 할때마다 데이터 베이스에 요청해서 가져오는 클래스다.
    private DatabaseReference leaderRef;

    private int cachedBestScore = 0;
    public int CachedBestScore => cachedBestScore;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //데이터 베이스 사용 될때까지 대기해야한다.
    private async UniTaskVoid Start()
    {
        await FirebaseInitializer.Instance.WaitForInitializationAsync(); //초기화 할때까지 대기

        leaderRef = FirebaseDatabase.DefaultInstance.RootReference.Child("leaderboard");

        Debug.Log("[leader] 초기화 완료");
    }

    public async UniTask<(bool success, string error)> SaveScoreAsync(int score)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (false, "로그인이 필요합니다.");
        }

        string uid = AuthManager.Instance.UserId;

        try
        {
            Debug.Log($"[leader] 점수 저장 시도: {score}");

            DatabaseReference historyRef = leaderRef.Child(uid).Child("history"); //해당 id의 레퍼런스 기록을 가져온다.
            DatabaseReference newHistoryRef = historyRef.Push(); //pushIdx 항목이 생기고, 작성한 내용이 이곳으로 저장된다.

            var leaderData = new Dictionary<string, object>();
            leaderData.Add("score", score);
            leaderData.Add("timestamp", ServerValue.Timestamp); //ServerValue : 플래그 같은 것이다. / 서버의 시간을 timestamp로 변환해서 반환하는 것이다.

            await newHistoryRef.UpdateChildrenAsync(leaderData).AsUniTask(); //성공하면 저장되고, 실패하면 예외처리로 넘어간다.

            bool shouldUpdateBestScore = false;
            if (cachedBestScore == 0)
            {
                var bestScoreSnapshot = await leaderRef.Child(uid).Child("bestScore").GetValueAsync().AsUniTask();

                if (!bestScoreSnapshot.Exists)
                {
                    shouldUpdateBestScore = true;
                }
                else if (score > cachedBestScore)
                {
                    shouldUpdateBestScore = true;
                }
            }
            else if (score > cachedBestScore)
            {
                shouldUpdateBestScore = true;
            }

            if (shouldUpdateBestScore)
            {
                //베스트 스코어 업데이트
                await UpdateBestScoreAsync(score);
            }

            Debug.Log($"점수 저장 성공: {score}");
            return (true, "[leader] 점수 저장 성공");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[leader] 점수 저장 실패! {ex.Message}");
            return (false, ex.Message);
        }
    }

    private async UniTask UpdateBestScoreAsync(int newBestScore)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return;
        }

        string uid = AuthManager.Instance.UserId;
        try
        {
            await leaderRef.Child(uid).Child("bestScore").SetValueAsync(newBestScore).AsUniTask();
            cachedBestScore = newBestScore;

            Debug.Log($"[leader] 최고 기록 갱신: {newBestScore}");
        }
        catch (System.Exception ex)
        {
            //LogErrorFormat : string.Format처럼 쓰는 디버그 에러 로그 함수다.
            Debug.LogErrorFormat("[leader] 최고 기록 로드 실패: {0}", ex.Message);
        }
    }

    public async UniTask<List<ScoreData>> LoadHistoryAsync(int limit = 10)
    {
        var list = new List<ScoreData>();

        if (!AuthManager.Instance.IsLoggedIn)
        {
            return list;
        }

        string uid = AuthManager.Instance.UserId;
        try
        {
            Debug.Log($"[leader] 히스토리 로드 시도");

            DatabaseReference historyRef = leaderRef.Child(uid).Child("history");

            historyRef.KeepSynced(true);
            await UniTask.Delay(100);

            //LimitToLast(limit) : 뒤에서부터 limit
            Query query = historyRef.OrderByChild("timestamp").LimitToLast(limit); //정렬해서 반환하는 linq와 닮은 문법

            DataSnapshot snapshot = await query.GetValueAsync().AsUniTask();
            if (snapshot.Exists)
            {
                foreach (DataSnapshot child in snapshot.Children) //children이기에 한 그룹마다 반환된다.
                {
                    string json = child.GetRawJsonValue();
                    ScoreData data = ScoreData.FromJson(json);
                    list.Add(data);
                }
            }

            Debug.Log($"[leader] 히스토리 로드 성공: {list.Count}개");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[leader] 히스토리 로드 실패: {ex.Message}");
        }

        return list;
    }
}
