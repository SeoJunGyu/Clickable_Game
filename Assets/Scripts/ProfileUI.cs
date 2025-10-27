using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    public Button changeBtn;
    public Button signOutBtn;
    public Button exitBtn;
    public Button profileButton;

    public GameObject ProfilePanel;
    public GameObject CreatePanel;
    public GameObject ChangeProfilePanel;
    public GameObject LogInPanel;

    public TextMeshProUGUI nicknameText;

    private void Start()
    {
        changeBtn.onClick.AddListener(() => OnChangeProfileClicked().Forget());
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

    public async UniTaskVoid OnChangeProfileClicked()
    {
        if (await ProfileManager.Instance.ProfileExistAsync())
        {
            ProfilePanel.SetActive(false);
            ChangeProfilePanel.SetActive(true);
        }
        else
        {
            ProfilePanel.SetActive(false);
            CreatePanel.SetActive(true);
        }
    }

    public void OnSignOutClicked()
    {
        AuthManager.Instance.SignOut();
        LogInPanel.SetActive(true);
        ProfilePanel.SetActive(false);
        profileButton.gameObject.SetActive(false);
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
            nicknameText.text = "(미설정)";
        }
    }
}
