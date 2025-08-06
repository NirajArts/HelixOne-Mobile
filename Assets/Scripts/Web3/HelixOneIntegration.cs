using System;
using System.Threading.Tasks;
using UnityEngine;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Rpc.Models;
using SolGame;
using SolGame.Program;
using SolGame.Accounts;
using SolGame.Types;

/// <summary>
/// Easy-to-use integration for HelixOne Solana program interactions
/// Provides simplified functions for subscription management and progress tracking
/// </summary>
public class HelixOneIntegration : MonoBehaviour
{
    [Header("Program Configuration")]
    [Tooltip("The Solana program ID for the HelixOne game")]
    public string programId = "91juefdypagioebacMPA6w1jVqUv7fPRpqqm7D4pM2RW";
    
    [Tooltip("The game authority public key (auto-set when wallet connects)")]
    public string authorityPublicKey = "";
    
    [Tooltip("Connected wallet public key (auto-set when wallet connects)")]
    public string connectedWalletPublicKey = "";
    
    [Header("Debug Settings")]
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogs = true;
    
    // Static instance for easy access
    public static HelixOneIntegration Instance { get; private set; }
    
    // Solana client for program interactions
    private SolGameClient solGameClient;
    
    // Game state PDA address
    private PublicKey gameStatePDA;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Subscribe to Web3 wallet events
        Web3.OnLogin += OnWalletConnected;
        Web3.OnLogout += OnWalletDisconnected;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        Web3.OnLogin -= OnWalletConnected;
        Web3.OnLogout -= OnWalletDisconnected;
    }
    
    void Start()
    {
        // Don't initialize client here - wait for wallet connection
        LogDebug("HelixOneIntegration ready - waiting for wallet connection");
        
        // Restore saved wallet data if available
        RestoreSavedWalletData();
    }
    
    /// <summary>
    /// Called when wallet is connected - initialize client and set authority
    /// </summary>
    private void OnWalletConnected(Solana.Unity.Wallet.Account account)
    {
        // Set authority public key to current wallet
        authorityPublicKey = account.PublicKey.Key;
        connectedWalletPublicKey = account.PublicKey.Key;
        
        // Save to PlayerPrefs for persistence
        PlayerPrefs.SetString("HelixOne_AuthorityKey", authorityPublicKey);
        PlayerPrefs.SetString("HelixOne_WalletKey", connectedWalletPublicKey);
        PlayerPrefs.Save();
        
        LogDebug($"Wallet connected: {account.PublicKey.Key}");
        LogDebug($"Authority set to: {authorityPublicKey}");
        
        // Initialize client now that wallet is connected
        InitializeClient();
        
        // Initialize game state if needed
        InitializeGameStateIfNeeded();
    }
    
    /// <summary>
    /// Called when wallet is disconnected - cleanup client
    /// </summary>
    private void OnWalletDisconnected()
    {
        authorityPublicKey = "";
        connectedWalletPublicKey = "";
        solGameClient = null;
        gameStatePDA = null;
        
        // Clear PlayerPrefs
        PlayerPrefs.DeleteKey("HelixOne_AuthorityKey");
        PlayerPrefs.DeleteKey("HelixOne_WalletKey");
        PlayerPrefs.Save();
        
        LogDebug("Wallet disconnected - client cleaned up");
    }
    
    /// <summary>
    /// Restore saved wallet data from PlayerPrefs
    /// </summary>
    private void RestoreSavedWalletData()
    {
        string savedAuthority = PlayerPrefs.GetString("HelixOne_AuthorityKey", "");
        string savedWallet = PlayerPrefs.GetString("HelixOne_WalletKey", "");
        
        if (!string.IsNullOrEmpty(savedAuthority))
        {
            authorityPublicKey = savedAuthority;
            LogDebug($"Restored authority key: {authorityPublicKey}");
        }
        
        if (!string.IsNullOrEmpty(savedWallet))
        {
            connectedWalletPublicKey = savedWallet;
            LogDebug($"Restored wallet key: {connectedWalletPublicKey}");
        }
    }
    
    /// <summary>
    /// Initialize game state if it doesn't exist (called after wallet connection)
    /// </summary>
    private async void InitializeGameStateIfNeeded()
    {
        try
        {
            // Check if game state already exists
            var gameState = await GetGameState();
            
            if (gameState == null)
            {
                LogDebug("Game state not found - attempting to initialize");
                bool success = await InitializeGameState();
                
                if (success)
                {
                    LogDebug("Game state initialized successfully");
                }
                else
                {
                    LogDebug("Failed to initialize game state or not authorized");
                }
            }
            else
            {
                LogDebug($"Game state already exists - Total players: {gameState.TotalPlayers}");
            }
        }
        catch (Exception e)
        {
            LogDebug($"Error checking/initializing game state: {e.Message}");
        }
    }
    
    /// <summary>
    /// Initialize the Solana client
    /// </summary>
    private void InitializeClient()
    {
        try
        {
            if (Web3.Rpc != null)
            {
                solGameClient = new SolGameClient(Web3.Rpc, Web3.WsRpc, new PublicKey(programId));
                
                // Calculate game state PDA
                byte bump;
                var gameStatePDAResult = PublicKey.TryFindProgramAddress(
                    new[] { System.Text.Encoding.UTF8.GetBytes("game_state") },
                    new PublicKey(programId),
                    out gameStatePDA,
                    out bump
                );
                
                if (!gameStatePDAResult)
                {
                    Debug.LogError("Failed to find game state PDA");
                    return;
                }
                
                LogDebug("HelixOneIntegration initialized successfully");
            }
            else
            {
                LogDebug("Web3 RPC not available yet, will retry when needed");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"HelixOneIntegration initialization failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// Initialize the game state (admin function)
    /// </summary>
    public async Task<bool> InitializeGameState()
    {
        try
        {
            if (!ValidateConnection()) return false;
            
            var accounts = new InitializeAccounts
            {
                GameState = gameStatePDA,
                Authority = Web3.Account.PublicKey,
                SystemProgram = new PublicKey("11111111111111111111111111111111")
            };
            
            var instruction = SolGameProgram.Initialize(accounts, new PublicKey(programId));
            var transaction = new Transaction { FeePayer = Web3.Account.PublicKey };
            transaction.Add(instruction);
            
            var result = await Web3.Wallet.SignAndSendTransaction(transaction);
            
            LogDebug($"Initialize game state transaction: {result}");
            return result != null && result.WasSuccessful && !string.IsNullOrEmpty(result.Result);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize game state: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Subscribe to monthly plan
    /// </summary>
    public async Task<bool> SubscribeMonthly()
    {
        try
        {
            if (!ValidateConnection()) return false;
            
            var playerPDA = GetPlayerAccountPDA(Web3.Account.PublicKey);
            
            var accounts = new SubscribeMonthlyAccounts
            {
                GameState = gameStatePDA,
                PlayerAccount = playerPDA,
                Player = Web3.Account.PublicKey,
                Authority = new PublicKey(authorityPublicKey),
                SystemProgram = new PublicKey("11111111111111111111111111111111")
            };
            
            var instruction = SolGameProgram.SubscribeMonthly(accounts, new PublicKey(programId));
            var transaction = new Transaction { FeePayer = Web3.Account.PublicKey };
            transaction.Add(instruction);
            
            var result = await Web3.Wallet.SignAndSendTransaction(transaction);
            
            LogDebug($"Monthly subscription transaction: {result}");
            return result != null && result.WasSuccessful && !string.IsNullOrEmpty(result.Result);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to subscribe monthly: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Subscribe to yearly plan
    /// </summary>
    public async Task<bool> SubscribeYearly()
    {
        try
        {
            if (!ValidateConnection()) return false;
            
            var playerPDA = GetPlayerAccountPDA(Web3.Account.PublicKey);
            
            var accounts = new SubscribeYearlyAccounts
            {
                GameState = gameStatePDA,
                PlayerAccount = playerPDA,
                Player = Web3.Account.PublicKey,
                Authority = new PublicKey(authorityPublicKey),
                SystemProgram = new PublicKey("11111111111111111111111111111111")
            };
            
            var instruction = SolGameProgram.SubscribeYearly(accounts, new PublicKey(programId));
            var transaction = new Transaction { FeePayer = Web3.Account.PublicKey };
            transaction.Add(instruction);
            
            var result = await Web3.Wallet.SignAndSendTransaction(transaction);
            
            LogDebug($"Yearly subscription transaction: {result}");
            return result != null && result.WasSuccessful && !string.IsNullOrEmpty(result.Result);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to subscribe yearly: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Update player's game progress (0-100)
    /// </summary>
    /// <param name="progress">Progress percentage (0-100)</param>
    public async Task<bool> UpdateProgress(byte progress)
    {
        try
        {
            if (!ValidateConnection()) return false;
            
            if (progress > 100)
            {
                Debug.LogError("Progress cannot exceed 100%");
                return false;
            }
            
            var playerPDA = GetPlayerAccountPDA(Web3.Account.PublicKey);
            
            var accounts = new UpdateProgressAccounts
            {
                PlayerAccount = playerPDA,
                Player = Web3.Account.PublicKey
            };
            
            var instruction = SolGameProgram.UpdateProgress(accounts, progress, new PublicKey(programId));
            var transaction = new Transaction { FeePayer = Web3.Account.PublicKey };
            transaction.Add(instruction);
            
            var result = await Web3.Wallet.SignAndSendTransaction(transaction);
            
            LogDebug($"Update progress ({progress}%) transaction: {result}");
            return result != null && result.WasSuccessful && !string.IsNullOrEmpty(result.Result);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update progress: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Update subscription prices (admin function)
    /// </summary>
    /// <param name="monthlyPrice">New monthly price in lamports</param>
    /// <param name="yearlyPrice">New yearly price in lamports</param>
    public async Task<bool> UpdatePrices(ulong monthlyPrice, ulong yearlyPrice)
    {
        try
        {
            if (!ValidateConnection()) return false;
            
            var accounts = new UpdatePricesAccounts
            {
                GameState = gameStatePDA,
                Authority = Web3.Account.PublicKey
            };
            
            var instruction = SolGameProgram.UpdatePrices(accounts, monthlyPrice, yearlyPrice, new PublicKey(programId));
            var transaction = new Transaction { FeePayer = Web3.Account.PublicKey };
            transaction.Add(instruction);
            
            var result = await Web3.Wallet.SignAndSendTransaction(transaction);
            
            LogDebug($"Update prices transaction: {result}");
            return result != null && result.WasSuccessful && !string.IsNullOrEmpty(result.Result);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update prices: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Get the current game state
    /// </summary>
    public async Task<GameState> GetGameState()
    {
        try
        {
            if (solGameClient == null) InitializeClient();
            if (solGameClient == null) return null;
            
            var result = await solGameClient.GetGameStateAsync(gameStatePDA.Key);
            
            if (result?.ParsedResult != null)
            {
                LogDebug($"Game state retrieved: Total players: {result.ParsedResult.TotalPlayers}");
                return result.ParsedResult;
            }
            
            LogDebug("Game state not found or not initialized");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get game state: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get player account data
    /// </summary>
    /// <param name="playerPublicKey">Player's public key (optional, uses current wallet if null)</param>
    public async Task<PlayerAccount> GetPlayerAccount(PublicKey playerPublicKey = null)
    {
        try
        {
            if (solGameClient == null) InitializeClient();
            if (solGameClient == null) return null;
            
            playerPublicKey ??= Web3.Account?.PublicKey;
            if (playerPublicKey == null)
            {
                Debug.LogError("No player public key provided and wallet not connected");
                return null;
            }
            
            var playerPDA = GetPlayerAccountPDA(playerPublicKey);
            var result = await solGameClient.GetPlayerAccountAsync(playerPDA.Key);
            
            if (result?.ParsedResult != null)
            {
                LogDebug($"Player account retrieved: Progress: {result.ParsedResult.GameProgress}%, NFT: {result.ParsedResult.HasCompletionNft}");
                return result.ParsedResult;
            }
            
            LogDebug("Player account not found");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get player account: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Check if player has an active subscription
    /// </summary>
    public async Task<bool> HasActiveSubscription()
    {
        try
        {
            var playerAccount = await GetPlayerAccount();
            if (playerAccount == null) return false;
            
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            bool isActive = currentTimestamp < playerAccount.SubscriptionEnd;
            
            LogDebug($"Subscription status: {(isActive ? "Active" : "Expired")}");
            return isActive;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to check subscription status: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Get player's current game progress (0-100)
    /// </summary>
    public async Task<byte> GetPlayerProgress()
    {
        try
        {
            var playerAccount = await GetPlayerAccount();
            if (playerAccount == null) return 0;
            
            LogDebug($"Player progress: {playerAccount.GameProgress}%");
            return playerAccount.GameProgress;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get player progress: {e.Message}");
            return 0;
        }
    }
    
    /// <summary>
    /// Check if player has completion NFT
    /// </summary>
    public async Task<bool> HasCompletionNFT()
    {
        try
        {
            var playerAccount = await GetPlayerAccount();
            if (playerAccount == null) return false;
            
            LogDebug($"Completion NFT status: {playerAccount.HasCompletionNft}");
            return playerAccount.HasCompletionNft;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to check NFT status: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Calculate player account PDA
    /// </summary>
    private PublicKey GetPlayerAccountPDA(PublicKey playerPublicKey)
    {
        PublicKey playerPDA;
        byte bump;
        var success = PublicKey.TryFindProgramAddress(
            new[] { 
                System.Text.Encoding.UTF8.GetBytes("player"), 
                playerPublicKey.KeyBytes 
            },
            new PublicKey(programId),
            out playerPDA,
            out bump
        );
        
        if (!success)
        {
            Debug.LogError("Failed to find player account PDA");
            return null;
        }
        
        return playerPDA;
    }
    
    /// <summary>
    /// Validate wallet connection and client initialization
    /// </summary>
    private bool ValidateConnection()
    {
        if (Web3.Account == null)
        {
            Debug.LogError("Wallet not connected. Please connect wallet first.");
            return false;
        }
        
        if (solGameClient == null)
        {
            InitializeClient();
            if (solGameClient == null)
            {
                Debug.LogError("Failed to initialize Solana client");
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Utility function for debug logging
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[HelixOneIntegration] {message}");
        }
    }
    
    #region Public Utility Functions
    
    /// <summary>
    /// Convert SOL to lamports
    /// </summary>
    public static ulong SolToLamports(double sol)
    {
        return (ulong)(sol * 1_000_000_000);
    }
    
    /// <summary>
    /// Convert lamports to SOL
    /// </summary>
    public static double LamportsToSol(ulong lamports)
    {
        return (double)lamports / 1_000_000_000;
    }
    
    /// <summary>
    /// Get subscription type as string
    /// </summary>
    public static string GetSubscriptionTypeString(SubscriptionType subscriptionType)
    {
        return subscriptionType switch
        {
            SubscriptionType.Monthly => "Monthly",
            SubscriptionType.Yearly => "Yearly",
            _ => "Unknown"
        };
    }
    
    #endregion
}
