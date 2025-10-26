using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeProfileUI : MonoBehaviour
{
    public TextMeshProUGUI nickname;
    public TMP_InputField nicknameInput;
    public Button changeBtn;
    public Button exitBtn;
    public GameObject changeProfilePanel;
    public GameObject profilePanel;

    private void Start()
    {
        changeBtn.onClick.AddListener(() => OnChangeProfileClicked().Forget());
        exitBtn.onClick.AddListener(() => OnExitClicked());
    }

    private void OnEnable()
    {
        LoadAndDisplayProfile().Forget();
    }

    public async UniTaskVoid OnChangeProfileClicked()
    {
        string nickname = nicknameInput.text;

        changeBtn.interactable = false;
        nicknameInput.interactable = false;

        try
        {
            var (success, error) = await ProfileManager.Instance.UpdateNicknameAsync(nickname);

            if (success)
            {
                Debug.Log("프로필 변경 성공!");

                profilePanel.SetActive(true);
                changeProfilePanel.SetActive(false);
            }
        }
        catch (System.Exception ex)
        {

        }
    }

    public void OnExitClicked()
    {
        profilePanel.SetActive(false);
    }

    private async UniTaskVoid LoadAndDisplayProfile()
    {
        var (profile, error) = await ProfileManager.Instance.LoadProfileAsync();

        if (profile != null)
        {
            nickname.text = profile.nickname;
        }
        else
        {
            Debug.LogError($"프로필 로드 실패: {error}");
        }
    }
}
