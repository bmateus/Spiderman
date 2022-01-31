#if UNITY_WEBGL
using Moralis.WebGL.Platform;
using Moralis.WebGL.Web3Api.Models;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
#else
using Moralis.Platform;
using Moralis.Platform.Objects;
using System.Collections.Generic;
using System.Threading.Tasks;
#endif

using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Unity;
using MoralisWeb3ApiSdk;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts;
using TMPro;

public class LoginPopup : MonoBehaviour
{
    public string MoralisApplicationId;
    public string MoralisServerURI;
    public string ApplicationName;
    public string Version;

    public string ApplicationDescription;
    public string[] ApplicationIcons;
    public string ApplicationUrl;
    public string Web3RpcNodeUrl;

    public GameObject AuthenticationButton;
    public GameObject StartButton;
    public WalletConnect walletConnect;
    public UIPopup qrMenu;
    public GameObject title;
    public GameObject scoreView;
    public GameObject waiting;


    public GameObject scorePrefab;
    public Transform scoreRoot;


    async void Start()
    {
        HostManifestData hostManifestData = new HostManifestData()
        {
            Version = Version,
            Identifier = ApplicationName,
            Name = ApplicationName,
            ShortVersion = Version
        };

        ClientMeta clientMeta = new ClientMeta()
        {
            Name = ApplicationName,
            Description = ApplicationDescription,
            Icons = ApplicationIcons,
            URL = ApplicationUrl
        };

        await MoralisInterface.Initialize(MoralisApplicationId,
                                            MoralisServerURI,
                                            hostManifestData,
                                            clientMeta,
                                            Web3RpcNodeUrl ) ;

        if (MoralisInterface.IsLoggedIn())
        {
            Debug.Log("User is already logged in to Moralis.");

            // Transition to main game scene
            StartButton.SetActive(true);

        }
        else
        {
            AuthenticationButton.SetActive(true);
        }
    }

    /// <summary>
    /// Used to start the authentication process and then run the game. If the 
    /// user has a valid Moralis session thes user is redirected to the next 
    /// scene.
    /// </summary>
    public async void Login()
    {
        Debug.Log("PLAY");

        AuthenticationButton.SetActive(false);

        // If the user is still logged in just show game.
        if (MoralisInterface.IsLoggedIn())
        {
            Debug.Log("User is already logged in to Moralis.");

            // Transition to main game scene
            //SceneManager.LoadScene("Main");
            //hide title and show ScoreView
            title.SetActive(false);
            scoreView.SetActive(true);
        }
        // User is not logged in, depending on build target, begin wallect connection.
        else
        {
            Debug.Log("User is not logged in.");
            //mainMenu.SetActive(false);

            // The mobile solutions for iOS and Android will be different once we
            // smooth out the interaction with Wallet Connect. For now the duplicated 
            // code below is on purpose just to keep the iOS and Android authentication
            // processes separate.
#if UNITY_ANDROID
            // By pass noraml Wallet Connect for now.
            //androidMenu.SetActive(true);

            // Use Moralis Connect page for authentication as we work to make the Wallet 
            // Connect experience better.
            await LoginViaConnectionPage();
#elif UNITY_IOS
            // By pass noraml Wallet Connect for now.
            //iOsMenu.SetActive(true);

            // Use Moralis Connect page for authentication as we work to make the Wallet 
            // Connect experience better.
            await LoginViaConnectionPage();
#else
            title.SetActive(false);
            qrMenu.Show();
#endif
        }
    }

    /// <summary>
    /// Handles the Wallet Connect OnConnection event.
    /// When user grants wallet connection to application this 
    /// method is called. A request for signature is sent to wallet. 
    /// If users agrees to sign the message the signed message is 
    /// used to authenticate with Moralis.
    /// </summary>
    /// <param name="data">WCSessionData</param>
    public async void WalletConnectHandler(WCSessionData data)
    {
        title.SetActive(true);

        Debug.Log("Wallet connection received");

        if (!MoralisInterface.IsLoggedIn())
        {
            waiting.SetActive(true);

            // Extract wallet address from the Wallet Connect Session data object.
            string address = data.accounts[0].ToLower();

            Debug.Log($"Sending sign request for {address} ...");

            string response = await walletConnect.Session.EthPersonalSign(address, "Moralis Authentication");

            Debug.Log($"Signature {response} for {address} was returned.");

            // Create moralis auth data from message signing response.
            Dictionary<string, object> authData = new Dictionary<string, object> { { "id", address }, { "signature", response }, { "data", "Moralis Authentication" } };

            Debug.Log("Logging in user.");

            // Attempt to login user.
            MoralisUser user = await MoralisInterface.LogInAsync(authData);

            if (user != null)
            {
                Debug.Log($"User {user.username} logged in successfully. ");
                scoreView.SetActive(true);
            }
            else
            {
                Debug.Log("User login failed.");
                title.SetActive(true);
                AuthenticationButton.SetActive(true);
            }

            waiting.SetActive(false);
        }
        else
        {
            Debug.Log("User is already logged in to Moralis.");
        }
    }

    /// <summary>
    /// Closeout connections and quit the application.
    /// </summary>
    public async void Quit()
    {
        Debug.Log("QUIT");

        // Disconnect wallet subscription.
        await walletConnect.Session.Disconnect();
        // CLear out the session so it is re-establish on sign-in.
        walletConnect.CLearSession();
        // Logout the Moralis User.
        await MoralisInterface.LogOutAsync();
        // Close out the application.
        Application.Quit();
    }

    /// <summary>
    /// Display Moralis connector login page
    /// </summary>
#if UNITY_WEBGL
    private async UniTask LoginViaConnectionPage()
    {
        // Use Moralis Connect page for authentication as we work to make the Wallet 
        // Connect experience better.
        MoralisUser user = await MobileLogin.LogIn(MoralisServerURI, MoralisApplicationId);

        if (user != null)
        {
            // User is not null so login was successful, show first game scene.
            SceneManager.LoadScene("Main");
            AuthenticationButton.SetActive(false);
        }
        else
        {
            AuthenticationButton.SetActive(true);
        }
    }
#else
    private async Task LoginViaConnectionPage()
    {
        // Use Moralis Connect page for authentication as we work to make the Wallet 
        // Connect experience better.
        MoralisUser user = await MobileLogin.LogIn(MoralisServerURI, MoralisApplicationId);

        if (user != null)
        {
            // User is not null so login was successful, show first game scene.
            SceneManager.LoadScene("Main");
            AuthenticationButton.SetActive(false);
        }
        else
        {
            AuthenticationButton.SetActive(true);
        }
    }
#endif

    [SerializeField]
    TextAsset rewardAbi;

    public void WalletConnectSessionEstablished(WalletConnectUnitySession session)
    {
        InitializeWeb3();
    }

    void InitializeWeb3()
    {
        Debug.Log("Setting Up Web3");
        MoralisInterface.SetupWeb3();
        MoralisInterface.InsertContractInstance(ContractData.REWARD_KEY, 
            rewardAbi.text, "polygon", ContractData.REWARD_CONTRACT);
    }

    public void ShowScoreView()
    {
        title.SetActive(false);
        StartButton.SetActive(false);
        scoreView.SetActive(true);

        //refresh high scores from Moralis
        RefreshScores();
    }


    async void RefreshScores()
    {
        //destroy the children
        for(int i=0; i < scoreRoot.childCount; ++i )
        {
            Destroy(scoreRoot.GetChild(i).gameObject);
        }

        try
        {
            var query = MoralisInterface.GetClient().Query<SpideyScores>()
                .OrderBy("Seconds")
                .Limit(10);
            var results = new List<SpideyScores>(await query.FindAsync());

            Debug.Log($"Query Results:{results}");

            if (results.Count == 0)
            {
                results.Add(new SpideyScores() { Name = "Stan Lee", Seconds = 120.4f });
                results.Add(new SpideyScores() { Name = "Peter Parker", Seconds = 136.4f });
                results.Add(new SpideyScores() { Name = "Mary Jane Watson", Seconds = 270.1f });
                results.Add(new SpideyScores() { Name = "Norman Osborn", Seconds = 536.4f });
            }


            int count = 0;
            foreach (var entry in results)
            {
                var obj = Instantiate(scorePrefab, scoreRoot);
                var tmp = obj.GetComponentInChildren<TMP_Text>();
                count++;
                tmp.text = $"{count}) {entry.Name} - {entry.Seconds}s";
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }


    public void Play()
    {
        SceneManager.LoadScene("Main");
    }


}

