using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Solana.Unity.SDK;

public class WalletManager : MonoBehaviour
{
    public string publicKey;

    [Header("UI Components")]
    public GameObject walletLoginPanel;
    public GameObject connectedPanel;

    void Start()
    {
        // Subscribe to Web3 wallet events
        Web3.OnLogin += OnWalletConnected;
        Web3.OnLogout += OnWalletDisconnected;
        
        // Check if wallet is already connected
        CheckWalletConnection();
    }
 
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        Web3.OnLogin -= OnWalletConnected;
        Web3.OnLogout -= OnWalletDisconnected;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// Connect wallet using Phantom wallet adapter (for Android)
    /// </summary>
    public async void ConnectWallet()
    {
        try
        {
            var account = await Web3.Instance.LoginWalletAdapter();
            if (account != null)
            {
                Debug.Log("Wallet connected successfully!");
            }
            else
            {
                Debug.LogError("Failed to connect wallet");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Wallet connection error: {e.Message}");
        }
    }
    
    /// <summary>
    /// Disconnect wallet
    /// </summary>
    public void DisconnectWallet()
    {
        Web3.Instance.Logout();
    }
    
    /// <summary>
    /// Called when wallet is connected
    /// </summary>
    private void OnWalletConnected(Solana.Unity.Wallet.Account account)
    {
        // Save public key
        publicKey = account.PublicKey.Key;
        
        // Save to PlayerPrefs
        PlayerPrefs.SetString("WalletPublicKey", publicKey);
        PlayerPrefs.Save();
        
        // Update UI
        if (walletLoginPanel != null)
            walletLoginPanel.SetActive(false);
            
        if (connectedPanel != null)
            connectedPanel.SetActive(true);
        
        Debug.Log($"Wallet connected! Public Key: {publicKey}");
    }
    
    /// <summary>
    /// Called when wallet is disconnected
    /// </summary>
    private void OnWalletDisconnected()
    {
        // Clear public key
        publicKey = "";
        
        // Clear PlayerPrefs
        PlayerPrefs.DeleteKey("WalletPublicKey");
        PlayerPrefs.Save();
        
        // Update UI
        if (connectedPanel != null)
            connectedPanel.SetActive(false);
            
        if (walletLoginPanel != null)
            walletLoginPanel.SetActive(true);
        
        Debug.Log("Wallet disconnected!");
    }
    
    /// <summary>
    /// Check if wallet is already connected on startup
    /// </summary>
    private void CheckWalletConnection()
    {
        // Check if Web3 wallet is already connected
        if (Web3.Account != null)
        {
            OnWalletConnected(Web3.Account);
        }
        else
        {
            // Load saved public key from PlayerPrefs
            string savedPublicKey = PlayerPrefs.GetString("WalletPublicKey", "");
            if (!string.IsNullOrEmpty(savedPublicKey))
            {
                publicKey = savedPublicKey;
                Debug.Log($"Loaded saved public key: {publicKey}");
            }
            
            // Show login panel by default
            if (walletLoginPanel != null)
                walletLoginPanel.SetActive(true);
                
            if (connectedPanel != null)
                connectedPanel.SetActive(false);
        }
    }
}
