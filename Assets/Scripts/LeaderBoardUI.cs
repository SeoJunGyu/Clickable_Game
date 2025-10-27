using Cysharp.Threading.Tasks;
using Firebase.Database;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardUI : MonoBehaviour
{
    public GameObject prefab;

    public ScrollRect scrollRect;
    private RectTransform content;

    public TextMeshProUGUI bestText;

    public Button reloadButton;
    public Button closeButton;

    public Toggle toggle;
    private bool isRealTimeMode = false;
    private CancellationTokenSource realtimeCts;

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => AuthManager.Instance != null && AuthManager.Instance.IsInitialized);

        reloadButton.onClick.AddListener(() => OnReloadHistoryClicked().Forget());
        closeButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });

        content = scrollRect.content;

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDisable()
    {
        StopRealTimeMode();
    }

    private void OnEnable()
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
        {
            return;
        }

        OnReloadHistoryClicked().Forget();
    }

    private async UniTaskVoid OnReloadHistoryClicked()
    {
        ResetList();

        var rankings = await LeaderBoardManager.Instance.LoadTopRankingsAsync();

        foreach (var (rank, userName, score) in rankings)
        {
            CreateLeaderBoardItem(rank, userName, score);
        }

        var (myRank, myName, myScore) = await LeaderBoardManager.Instance.GetMyRankingAsync();

        if(myRank > 0)
        {
            bestText.text = $"내 순위: {myRank}위 / {myName} / {myScore}점";
        }
        else
        {
            bestText.text = "아직 기록이 없습니다.";
        }
    }

    private void CreateLeaderBoardItem(int rank, string userName, int score)
    {
        GameObject item = Instantiate(prefab, content);
        TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();

        if(text != null)
        {
            text.text = $"{rank}위 / {userName} / {score}점";
        }
    }

    private void ResetList()
    {
        if (content == null)
        {
            return;
        }

        for (int i = 0; i < content.childCount; i++)
        {
            var child = content.GetChild(i).gameObject;
            Destroy(child);
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            StartRealTimeMode();
        }
        else
        {
            StopRealTimeMode();
        }
    }

    private void StartRealTimeMode()
    {
        if (isRealTimeMode)
        {
            return;
        }

        isRealTimeMode = true;
        realtimeCts = new CancellationTokenSource();

        Debug.Log("[LeaderBoardUI] 실시간 모드 시작");

        DatabaseReference leaderboardRef = FirebaseDatabase.DefaultInstance.GetReference("leaderboard");
        leaderboardRef.ValueChanged += OnLeaderboardChanged;
    }

    private void StopRealTimeMode()
    {
        if (!isRealTimeMode)
        {
            return;
        }

        isRealTimeMode = false;
        realtimeCts?.Cancel();
        realtimeCts?.Dispose();
        realtimeCts = null;

        Debug.Log("[LeaderBoardUI] 실시간 모드 중지");

        DatabaseReference leaderboardRef = FirebaseDatabase.DefaultInstance.GetReference("leaderboard");
        leaderboardRef.ValueChanged -= OnLeaderboardChanged;
    }

    private void OnLeaderboardChanged(object sender, ValueChangedEventArgs args)
    {
        if (!isRealTimeMode)
        {
            return;
        }

        Debug.Log("[LeaderBoardUI] 리더보드 데이터 변경");

        OnReloadHistoryClicked().Forget();
    }

    private void OnDestroy()
    {
        StopRealTimeMode();
    }
}
