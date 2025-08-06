using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Solana.Unity;
using Solana.Unity.Programs.Abstract;
using Solana.Unity.Programs.Utilities;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Wallet;
using SolGame;
using SolGame.Program;
using SolGame.Errors;
using SolGame.Accounts;
using SolGame.Types;

namespace SolGame
{
    namespace Accounts
    {
        public partial class GameState
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 8684738851132956304UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{144, 94, 208, 172, 248, 99, 134, 120};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "R9aG661U96X";
            public PublicKey Authority { get; set; }

            public ulong TotalPlayers { get; set; }

            public ulong MonthlyPrice { get; set; }

            public ulong YearlyPrice { get; set; }

            public static GameState Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                GameState result = new GameState();
                result.Authority = _data.GetPubKey(offset);
                offset += 32;
                result.TotalPlayers = _data.GetU64(offset);
                offset += 8;
                result.MonthlyPrice = _data.GetU64(offset);
                offset += 8;
                result.YearlyPrice = _data.GetU64(offset);
                offset += 8;
                return result;
            }
        }

        public partial class PlayerAccount
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 17019182578430687456UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{224, 184, 224, 50, 98, 72, 48, 236};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "eb62BHK8YZR";
            public PublicKey Player { get; set; }

            public SubscriptionType SubscriptionType { get; set; }

            public long SubscriptionStart { get; set; }

            public long SubscriptionEnd { get; set; }

            public byte GameProgress { get; set; }

            public bool HasCompletionNft { get; set; }

            public static PlayerAccount Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                PlayerAccount result = new PlayerAccount();
                result.Player = _data.GetPubKey(offset);
                offset += 32;
                result.SubscriptionType = (SubscriptionType)_data.GetU8(offset);
                offset += 1;
                result.SubscriptionStart = _data.GetS64(offset);
                offset += 8;
                result.SubscriptionEnd = _data.GetS64(offset);
                offset += 8;
                result.GameProgress = _data.GetU8(offset);
                offset += 1;
                result.HasCompletionNft = _data.GetBool(offset);
                offset += 1;
                return result;
            }
        }
    }

    namespace Errors
    {
        public enum SolGameErrorKind : uint
        {
            SubscriptionExpired = 6000U,
            InvalidProgress = 6001U,
            GameNotCompleted = 6002U,
            NftAlreadyMinted = 6003U
        }
    }

    namespace Types
    {
        public enum SubscriptionType : byte
        {
            Monthly,
            Yearly
        }
    }

    public partial class SolGameClient : TransactionalBaseClient<SolGameErrorKind>
    {
        public SolGameClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId = null) : base(rpcClient, streamingRpcClient, programId ?? new PublicKey(SolGameProgram.ID))
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<GameState>>> GetGameStatesAsync(string programAddress = SolGameProgram.ID, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = GameState.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<GameState>>(res);
            List<GameState> resultingAccounts = new List<GameState>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => GameState.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<GameState>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<PlayerAccount>>> GetPlayerAccountsAsync(string programAddress = SolGameProgram.ID, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = PlayerAccount.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<PlayerAccount>>(res);
            List<PlayerAccount> resultingAccounts = new List<PlayerAccount>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => PlayerAccount.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<PlayerAccount>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<GameState>> GetGameStateAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<GameState>(res);
            var resultingAccount = GameState.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<GameState>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<PlayerAccount>> GetPlayerAccountAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<PlayerAccount>(res);
            var resultingAccount = PlayerAccount.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<PlayerAccount>(res, resultingAccount);
        }

        public async Task<SubscriptionState> SubscribeGameStateAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, GameState> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                GameState parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = GameState.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribePlayerAccountAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, PlayerAccount> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                PlayerAccount parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = PlayerAccount.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        protected override Dictionary<uint, ProgramError<SolGameErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<SolGameErrorKind>>{{6000U, new ProgramError<SolGameErrorKind>(SolGameErrorKind.SubscriptionExpired, "Subscription has expired")}, {6001U, new ProgramError<SolGameErrorKind>(SolGameErrorKind.InvalidProgress, "Invalid progress value. Must be between 0 and 100")}, {6002U, new ProgramError<SolGameErrorKind>(SolGameErrorKind.GameNotCompleted, "Game must be 100% completed to mint NFT")}, {6003U, new ProgramError<SolGameErrorKind>(SolGameErrorKind.NftAlreadyMinted, "Completion NFT already minted for this player")}, };
        }
    }

    namespace Program
    {
        public class InitializeAccounts
        {
            public PublicKey GameState { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey SystemProgram { get; set; } = new PublicKey("11111111111111111111111111111111");
        }

        public class SubscribeMonthlyAccounts
        {
            public PublicKey GameState { get; set; }

            public PublicKey PlayerAccount { get; set; }

            public PublicKey Player { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey SystemProgram { get; set; } = new PublicKey("11111111111111111111111111111111");
        }

        public class SubscribeYearlyAccounts
        {
            public PublicKey GameState { get; set; }

            public PublicKey PlayerAccount { get; set; }

            public PublicKey Player { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey SystemProgram { get; set; } = new PublicKey("11111111111111111111111111111111");
        }

        public class UpdatePricesAccounts
        {
            public PublicKey GameState { get; set; }

            public PublicKey Authority { get; set; }
        }

        public class UpdateProgressAccounts
        {
            public PublicKey PlayerAccount { get; set; }

            public PublicKey Player { get; set; }
        }

        public static class SolGameProgram
        {
            public const string ID = "91juefdypagioebacMPA6w1jVqUv7fPRpqqm7D4pM2RW";
            public static Solana.Unity.Rpc.Models.TransactionInstruction Initialize(InitializeAccounts accounts, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.GameState, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Authority, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(17121445590508351407UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction SubscribeMonthly(SubscribeMonthlyAccounts accounts, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.GameState, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.PlayerAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Player, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Authority, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(9664417972857443539UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction SubscribeYearly(SubscribeYearlyAccounts accounts, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.GameState, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.PlayerAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Player, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Authority, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(9543263563044721969UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction UpdatePrices(UpdatePricesAccounts accounts, ulong new_monthly_price, ulong new_yearly_price, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.GameState, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(11534310640515195198UL, offset);
                offset += 8;
                _data.WriteU64(new_monthly_price, offset);
                offset += 8;
                _data.WriteU64(new_yearly_price, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction UpdateProgress(UpdateProgressAccounts accounts, byte new_progress, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.PlayerAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Player, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(8004477753423179655UL, offset);
                offset += 8;
                _data.WriteU8(new_progress, offset);
                offset += 1;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }
        }
    }
}