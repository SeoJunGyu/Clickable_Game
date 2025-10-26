using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    public Button changeBtn;
    public Button signOutBtn;
    public Button exitBtn;

    public GameObject ProfilePanel;
    public GameObject ChangeProfilePanel;
    public GameObject LogInPanel;

    public TextMeshProUGUI nicknameText;

    private void Start()
    {
        changeBtn.onClick.AddListener(() => OnChangeProfileClicked());
        signOutBtn.onClick.AddListener(() => OnSignOutClicked());
        exitBtn.onClick.AddListener(() => OnExitClicked());
    }

    private void OnEnable()
    {
        LoadAndDisplayProfile().Forget();
    }

    private void OnDestroy()
    {
        changeBtn.onClick.RemoveAllListeners();
        signOutBtn.onClick.RemoveAllListeners();
        exitBtn.onClick.RemoveAllListeners();
    }

    public void OnChangeProfileClicked()
    {
        ProfilePanel.SetActive(false);
        ChangeProfilePanel.SetActive(true);
    }

    public void OnSignOutClicked()
    {
        AuthManager.Instance.SignOut();
        LogInPanel.SetActive(true);
        ProfilePanel.SetActive(false);
    }

    public void OnExitClicked()
    {
        ProfilePanel.SetActive(false);
    }

    private async UniTaskVoid LoadAndDisplayProfile()
    {
        var (profile, error) = await ProfileManager.Instance.LoadProfileAsync();

        if (profile != null)
        {
            nicknameText.text = profile.nickname;
        }
        else
        {
            Debug.LogError($"프로필 로드 실패: {error}");
        }
    }
}
