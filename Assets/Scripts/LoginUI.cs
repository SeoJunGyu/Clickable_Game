using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Firebase.Auth;
using Cysharp.Threading.Tasks.Triggers;

public class LoginUI : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject profilePanel;
    public GameObject createProfilePanel;

    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    public Button loginButton;
    public Button signupButton;
    public Button anonymousButton;
    public Button profileButton;

    public TextMeshProUGUI errorText;
    public TextMeshProUGUI profileText;

    private async UniTaskVoid Start()
    {
        SetButtonsInteractable(false);

        await UniTask.WaitUntil(() => AuthManager.Instance != null && AuthManager.Instance.IsInitialized);

        SetButtonsInteractable(true);

        profileButton.gameObject.SetActive(false);

        loginButton.onClick.AddListener(() => OnLoginClicked().Forget());
        signupButton.onClick.AddListener(() => OnSignupClicked().Forget());
        anonymousButton.onClick.AddListener(() => OnAnonymousClicked().Forget());
        profileButton.onClick.AddListener(() => OnProfileButtonClicked().Forget());

        UpdateUI().Forget();
    }

    public async UniTaskVoid UpdateUI()
    {
        //매니저 없거나 초기화 안되어있다면 리턴
        if(AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
        {
            return;
        }

        bool isLoggedIn = AuthManager.Instance.IsLoggedIn;
        loginPanel.SetActive(!isLoggedIn);

        if (isLoggedIn)
        {
            bool isProfileExist = await ProfileManager.Instance.ProfileExistAsync();
            if (isProfileExist)
            {
                var result = await ProfileManager.Instance.LoadProfileAsync();
                profileText.text = result.profile.nickname;
            }
            else
            {
                string userId = AuthManager.Instance.UserId;
                profileText.text = userId;
            }

            //gameObject.SetActive(false);
        }
        else
        {
            profileButton.gameObject.SetActive(false);
        }

        errorText.text = string.Empty;
        profileButton.gameObject.SetActive(true);
    }

    public async UniTask OnLoginClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        SetButtonsInteractable(false);

        var (success, error) = await AuthManager.Instance.SighInWithEmailAsync(email, password);

        if (success)
        {

        }
        else
        {
            ShowError(error);
        }

        SetButtonsInteractable(true);

        UpdateUI().Forget();
    }

    public async UniTask OnSignupClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        SetButtonsInteractable(false);

        var (success, error) = await AuthManager.Instance.CreateUserWithEmailAsync(email, password);
        if (success)
        {

        }
        else
        {
            ShowError(error);
        }

        SetButtonsInteractable(true);

        UpdateUI().Forget();
    }

    public async UniTask OnAnonymousClicked()
    {
        SetButtonsInteractable(false);

        var (success, error) = await AuthManager.Instance.SingInAnonymouslyAsync();
        if (success)
        {
            
        }
        else
        {
            ShowError(error);
        }

        SetButtonsInteractable(true);

        UpdateUI().Forget();
    }

    public void ShowError(string error)
    {
        errorText.text = $"{error}";
    }

    private void SetButtonsInteractable(bool interactable)
    {
        loginButton.interactable = interactable;
        signupButton.interactable = interactable;
        anonymousButton.interactable = interactable;
    }

    private async UniTaskVoid OnProfileButtonClicked()
    {
        profilePanel.SetActive(true);
    }
}
