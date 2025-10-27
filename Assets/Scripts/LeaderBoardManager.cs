using Cysharp.Threading.Tasks;
using Firebase.Database;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderBoardManager : MonoBehaviour
{
    private static LeaderBoardManager instance;
    public static LeaderBoardManager Instance => instance;

    //읽고 쓸때 요청 할때마다 데이터 베이스에 요청해서 가져오는 클래스다.
    private DatabaseReference leaderboardRef;

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

        leaderboardRef = FirebaseDatabase.DefaultInstance.RootReference.Child("leaderboard");

        Debug.Log("[leader] 초기화 완료");
    }

    public async UniTask<(bool success, string error)> UpdateLeaderBoardAsync(int score, string userName)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (false, "로그인이 필요합니다.");
        }

        string uid = AuthManager.Instance.UserId;

        try
        {
            Debug.Log($"[leader] 업데이트 시도: {userName} - {score}");

            DataSnapshot snapshot = await leaderboardRef.Child(uid).GetValueAsync().AsUniTask();

            if (snapshot.Exists)
            {
                var data = LeaderBoardData.FromJson(snapshot.GetRawJsonValue());
                if (score <= data.score)
                {
                    return (true, "기존 점수가 더 높습니다.");
                }
            }

            var leaderboardData = new Dictionary<string, object>
            {
                { "userName", userName },
                { "score", score }
            };

            await leaderboardRef.Child(uid).UpdateChildrenAsync(leaderboardData).AsUniTask();

            Debug.Log($"[LeaderBoard] 리더보드 업데이트 성공");
            return (true, "[leader] 업데이트 성공");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[leader] 점수 업데이트 실패: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async UniTask<List<(int rank, string userName, int score)>> LoadTopRankingsAsync(int limit = 10)
    {
        var result = new List<(int rank, string userName, int score)>();

        try
        {
            Debug.Log($"[LeaderBoard] 상위 랭킹 로드 시도 (limit: {limit})");

            leaderboardRef.KeepSynced(true);
            await UniTask.Delay(100);

            Query query = leaderboardRef.OrderByChild("score").LimitToLast(limit);
            DataSnapshot snapshot = await query.GetValueAsync().AsUniTask();

            if (snapshot.Exists)
            {
                var tempList = new List<LeaderBoardData>();

                foreach (DataSnapshot child in snapshot.Children)
                {
                    try
                    {
                        string json = child.GetRawJsonValue();
                        LeaderBoardData data = LeaderBoardData.FromJson(json);
                        tempList.Add(data);
                    }
                    catch (System.Exception innerEx)
                    {
                        Debug.LogError($"[LeaderBoard] 로드 실패: {innerEx.Message}");
                    }
                }

                // score 내림차순 정렬
                tempList = tempList.OrderByDescending(x => x.score).ToList();

                // 순위 추가
                for (int i = 0; i < tempList.Count; i++)
                {
                    result.Add((i + 1, tempList[i].userName, tempList[i].score));
                }
            }

            Debug.Log($"[LeaderBoard] 상위 랭킹 로드 성공: {result.Count}개");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LeaderBoard] 상위 랭킹 로드 실패: {ex.Message}");
        }

        return result;
    }

    public async UniTask<(int rank, string userName, int score)> GetMyRankingAsync()
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (-1, "", 0);
        }

        string uid = AuthManager.Instance.UserId;

        try
        {
            Debug.Log($"[LeaderBoard] 내 순위 조회 시도");

            // 내 점수 가져오기
            DataSnapshot mySnapshot = await leaderboardRef.Child(uid).GetValueAsync().AsUniTask();

            if (!mySnapshot.Exists)
            {
                Debug.Log("[LeaderBoard] 리더보드에 기록 없음");
                return (-1, "", 0);
            }

            LeaderBoardData myData = LeaderBoardData.FromJson(mySnapshot.GetRawJsonValue());

            // 전체 리더보드 가져오기
            DataSnapshot allSnapshot = await leaderboardRef.GetValueAsync().AsUniTask();

            if (!allSnapshot.Exists)
            {
                return (-1, "", 0);
            }

            var allPlayers = new List<(string userId, LeaderBoardData data)>();

            foreach (DataSnapshot child in allSnapshot.Children)
            {
                try
                {
                    string json = child.GetRawJsonValue();
                    LeaderBoardData data = LeaderBoardData.FromJson(json);
                    allPlayers.Add((child.Key, data));
                }
                catch (System.Exception innerEx)
                {
                    Debug.LogError($"[LeaderBoard] 파싱 실패: {innerEx.Message}");
                }
            }

            // score 내림차순 정렬
            allPlayers = allPlayers.OrderByDescending(x => x.data.score).ToList();

            // 내 순위 찾기
            int ranking = allPlayers.FindIndex(x => x.userId == uid) + 1;

            Debug.Log($"[LeaderBoard] 내 순위: {ranking}위 - {myData.userName} - {myData.score}점");
            return (ranking, myData.userName, myData.score);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LeaderBoard] 내 순위 조회 실패: {ex.Message}");
            return (-1, "", 0);
        }
    }
}
