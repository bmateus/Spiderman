using MoralisWeb3ApiSdk;
using Nethereum.Hex.HexTypes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static GameManager instance_;

    public static GameManager Instance => instance_;

    private void Awake()
    {
        instance_ = this;
    }

    [SerializeField]
    Animator panelAnimator;

    [SerializeField]
    TMP_Text panelText;


    [SerializeField]
    UIPopup GameOverPopup;

    [SerializeField]
    UIPopup WinPopup;

    [SerializeField]
    TMP_Text WinPopupTime;

    [SerializeField]
    GameObject WinPopupMint;



    public void ShowTutorial(string text)
    {
        panelText.text = text;
        panelAnimator.Play("PanelOpen");
    }

    public void ShowGameOver()
    {
        GameOverPopup.gameObject.SetActive(true);
        GameOverPopup.Show();
    }

    public async void Win()
    {
        var game = FindObjectOfType<Spiderman>();
        game.SetGameOver();

        //show a waiting screen here

        // Post score and 
        // Show Minting Dialog
        SaveScore();

        //see if there are mints remaining
        // might be better to query stuff like this from the graph

        var getTokenSupply = MoralisInterface.EvmContractFunctionInstance(ContractData.REWARD_KEY, "polygon", "tokenSupply");
        var tokenSupply = await getTokenSupply.CallAsync<int>();
        Debug.Log($"response: {tokenSupply}");

        if (tokenSupply < 500)
        {
            //if so, show minting popup
            WinPopupMint.SetActive(true);
        }
        else
        {
            //else, just show congrats
            WinPopupMint.SetActive(false);
        }

        WinPopupTime.text = $"Time: {game.TimeElapsed:0.00}s";

        WinPopup.gameObject.SetActive(true);
        WinPopup.Show();

    }

    async void SaveScore()
    {
        var game = FindObjectOfType<Spiderman>();
        var user = await MoralisInterface.GetUserAsync();
        var addr = user.authData["moralisEth"]["id"].ToString();
        var ens = MoralisInterface.GetClient().Web3Api.Resolve.ResolveAddress(addr);
        var score = MoralisInterface.GetClient().Create<SpideyScores>();
        score.Name = !string.IsNullOrEmpty(ens.Name) ? ens.Name : addr;
        score.Seconds = game.TimeElapsed;
        await score.SaveAsync();
        Debug.Log("SavedScore");
    }

    public void Restart()
    {
        SceneManager.LoadScene("Main");
    }

    public void Quit()
    {
        SceneManager.LoadScene("Startup");
    }

    public async void Mint()
    {
        var user = await MoralisInterface.GetUserAsync();
        string addr = user.authData["moralisEth"]["id"].ToString();
        var mintFunction = MoralisInterface.EvmContractFunctionInstance(ContractData.REWARD_KEY, "polygon", "mint");
        var receipt = await MoralisInterface.SendTransactionAndWaitForReceiptAsync(
            ContractData.REWARD_KEY, "polygon", "mint", addr, new HexBigInteger(0), new HexBigInteger(0), null);
        Debug.Log($"Receipt: {receipt}");
        SceneManager.LoadScene("Startup");
    }

}
