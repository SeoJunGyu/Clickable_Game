using UnityEngine;
using Firebase.Database;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance;
    public static ScoreManager Instance => instance;

    //읽고 쓸때 요청 할때마다 데이터 베이스에 요청해서 가져오는 클래스다.
    private DatabaseReference scoreRef;

    private int cachedBestScore = 0;
    public int CachedBestScore => cachedBestScore;

    private void Awake()
    {
        if(instance == null)
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

        scoreRef = FirebaseDatabase.DefaultInstance.RootReference.Child("scores");

        Debug.Log("[Score] 초기화 완료");
        await LoadBestScoreAsync(); //최고점수가 로딩 될때까지 대기
    }

    private async UniTask<int> LoadBestScoreAsync()
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return 0;
        }

        string uid = AuthManager.Instance.UserId;

        //항상 파이어 베이스에서 뭔가를 읽고 쓸때는 try catch로 해야한다.
        try
        {
            //DataSnapshot : 파이어베이스에서 가져오는 데이터의 집합 클래스
            //GetValueAsync() : 해당 이름의 값을 가져오는 함수
            DataSnapshot snapshot = await scoreRef.Child(uid).Child("bestScore").GetValueAsync();
            if (!snapshot.Exists)
            {
                //스냅샷은 오브젝트 형이기에 바로 값을 가져올 수 있지만, 항상 string으로 바꾸고 출력할 데이터형으로 다시 parsing 해라
                cachedBestScore = int.Parse(snapshot.Value.ToString());
                Debug.Log($"[Score] 최고 기록 로드: {cachedBestScore}");
            }
            else
            {
                cachedBestScore = 0;
                Debug.Log("[Score] 최고 기록 없음");
            }
        }
        catch(System.Exception ex)
        {
            Debug.LogError($"[Score] 최고 기록 로드 실패: {ex.Message}");
        }

        return 0;
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
            Debug.Log($"[Score] 점수 저장 시도: {score}");

            DatabaseReference historyRef = scoreRef.Child(uid).Child("history"); //해당 id의 레퍼런스 기록을 가져온다.
            DatabaseReference newHistoryRef = historyRef.Push(); //pushIdx 항목이 생기고, 작성한 내용이 이곳으로 저장된다.

            var scoreData = new Dictionary<string, object>();
            scoreData.Add("score", score);
            scoreData.Add("timestamp", ServerValue.Timestamp); //ServerValue : 플래그 같은 것이다. / 서버의 시간을 timestamp로 변환해서 반환하는 것이다.

            await newHistoryRef.UpdateChildrenAsync(scoreData).AsUniTask(); //성공하면 저장되고, 실패하면 예외처리로 넘어간다.

            bool shouldUpdateBestScore = false;
            if(cachedBestScore == 0)
            {
                var bestScoreSnapshot = await scoreRef.Child(uid).Child("bestScore").GetValueAsync().AsUniTask();

                if (!bestScoreSnapshot.Exists)
                {
                    shouldUpdateBestScore = true;
                }
                else if(score > cachedBestScore)
                {
                    shouldUpdateBestScore = true;
                }
            }
            else if(score > cachedBestScore)
            {
                shouldUpdateBestScore = true;
            }

            if (shouldUpdateBestScore)
            {
                //베스트 스코어 업데이트
                await UpdateBestScoreAsync(score);
            }

            Debug.Log($"점수 저장 성공: {score}");
            return (true, "[Score] 점수 저장 성공");
        }
        catch(System.Exception ex)
        {
            Debug.LogError($"[Score] 점수 저장 실패! {ex.Message}");
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
            await scoreRef.Child(uid).Child("bestScore").SetValueAsync(newBestScore).AsUniTask();
            cachedBestScore = newBestScore;

            Debug.Log($"[Score] 최고 기록 갱신: {newBestScore}");
        }
        catch(System.Exception ex)
        {
            //LogErrorFormat : string.Format처럼 쓰는 디버그 에러 로그 함수다.
            Debug.LogErrorFormat("[Score] 최고 기록 로드 실패: {0}", ex.Message);
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
            Debug.Log($"[Score] 히스토리 로드 시도");

            DatabaseReference historyRef = scoreRef.Child(uid).Child("history");

            historyRef.KeepSynced(true);
            await UniTask.Delay(100);

            //LimitToLast(limit) : 뒤에서부터 limit
            Query query = historyRef.OrderByChild("timestamp").LimitToLast(limit); //정렬해서 반환하는 linq와 닮은 문법

            DataSnapshot snapshot = await query.GetValueAsync().AsUniTask();
            if (snapshot.Exists)
            {
                foreach(DataSnapshot child in snapshot.Children) //children이기에 한 그룹마다 반환된다.
                {
                    string json = child.GetRawJsonValue();
                    ScoreData data = ScoreData.FromJson(json);
                    list.Add(data);
                }
            }

            Debug.Log($"[Score] 히스토리 로드 성공: {list.Count}개");
        }
        catch(System.Exception ex)
        {
            Debug.LogError($"[Score] 히스토리 로드 실패: {ex.Message}");
        }

        list.Reverse();

        return list;
    }
}
