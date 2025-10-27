using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordUI : MonoBehaviour
{
    public GameObject prefab;
    public GameObject StartPanel;

    public ScrollRect scrollRect;
    private RectTransform content;

    public TextMeshProUGUI recordText;

    public Button reloadButton;
    public Button closeButton;

    private int bestScore;

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => AuthManager.Instance != null && AuthManager.Instance.IsInitialized);

        reloadButton.onClick.AddListener(() => OnReloadHistoryClicked().Forget());
        closeButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });

        content = scrollRect.content;
    }

    private void OnEnable()
    {
        if(AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
        {
            return;
        }

        OnReloadHistoryClicked().Forget();
    }

    private async UniTaskVoid OnReloadHistoryClicked()
    {
        ResetList();

        var list = await ScoreManager.Instance.LoadHistoryAsync();

        foreach(var data in list)
        {
            var score = data.score;
            var contentObj = Instantiate(prefab, content);
            contentObj.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("{0}점 - {1}", score, data.GetDateString());
        }


        recordText.text = $"최고 기록: {ScoreManager.Instance.CachedBestScore}";
    }

    private void ResetList()
    {
        if(content == null)
        {
            return;
        }

        for(int i = 0; i < content.childCount; i++)
        {
            var child = content.GetChild(i).gameObject;
            Destroy(child);
        }
    }
}
