using BillyCDK.DataAccess.Models;
using BillyCDK.DataAccess.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BillyCDK.DataAccess.Services;

public class CDKeyService : AbstractDataService<CDKey>, ICDKeyService
{
    private readonly IPlayerService playerService;

    protected override IMongoCollection<CDKey> Collection { get; set; }
    protected override string EntityName { get; set; } = "cdkey";

    public CDKeyService(
        IDBCollection collection,
        IPlayerService playerService
    )
    {
        Collection = collection.CDKeys;
        this.playerService = playerService;
    }

    public async Task<IList<CDKey>> GetByValues(IList<string> values, IDBViewOption? options = null)
    {
        string quotedValues = values.ToMarkedString();

        FilterDefinition<CDKey> searchCondition = JsonUtils.CreateCompactLiteral($@"{{
            ""value"": {{
                ""$in"": [ {quotedValues} ]
            }}
        }}");

        return await Get(searchCondition, options);
    }

    public async Task<InstanceMessage<CDKey>> AddCDKeys(IList<InputCDKey> newCDKeys)
    {
        var insertingCDKeys = (
            from cdkey in newCDKeys
            select new CDKey(
                ID: ObjectId.GenerateNewId().ToString(),
                Value: cdkey.Value,
                UpdatedDate: DateTime.Now,
                DeletedDate: null,
                Player: cdkey.Player,
                Game: cdkey.Game,
                IsActivated: cdkey.IsActivated ?? false,
                Price: cdkey.Price,
                Platform: cdkey.Platform ?? ""
            )
        ).ToList();
        return await Add(insertingCDKeys);
    }

    public async Task<InstanceMessage<CDKey>> Activate(string playerDBName, IList<string> values)
    {
        Player? player = (await playerService.Get(
            DBUtils.BuildSingleQueryCondition<Player>("dbname", playerDBName)
        )).FirstOrDefault();
        if (player is null)
        {
            return new InstanceMessage<CDKey>(
                Okay: 0,
                Message: $"Player {playerDBName} does not exist.",
                NumAffected: 0,
                Instances: null
            );
        }
        return await Activate(player, values);
    }

    public async Task<InstanceMessage<CDKey>> Activate(Player player, IList<string> values)
    {
        // check if cdkey list is empty
        if (values.Count <= 0)
        {
            return new InstanceMessage<CDKey>(
                Okay: 0,
                Message: $"No CDKeys to activate since the input list is empty.",
                NumAffected: 0,
                Instances: null
            );
        }

        Dictionary<string, string> cdkValueToErrorMessageMap = new();

        IList<CDKey> validCDKeys = await GetByValues(values, null);

        // check if cdk already activated + if player already owns the game
        validCDKeys = (
            from validCDKey in validCDKeys
            where (!validCDKey.IsActivated && !player.Games.Exists(ele => ele == validCDKey.Game))
            select validCDKey
        ).ToList();

        var validCDKeyValues = (
            from cdkey in validCDKeys
            select cdkey.Value
        );

        foreach (var value in values)
        {
            if (!validCDKeyValues.Contains(value))
            {
                cdkValueToErrorMessageMap.Add(value, $"This CDKey is invalid, has already been activated or the player has already owned the game.");
            }
        }

        // update CDKey isActivated field
        // mark the player who activates cdkey
        List<CDKey> successfullyActivatedCDKeys = new();

        foreach (var validCDKey in validCDKeys)
        {
            InstanceMessage<CDKey> cdkUpdateResult = await UpdateAsActivated(
                new Dictionary<string, string>
                {
                    [validCDKey.ID] = player.DBName
                },
                true
            );
            if (cdkUpdateResult.Okay == 1 && (cdkUpdateResult.NumAffected > 0))
            {
                successfullyActivatedCDKeys.Add(validCDKey);
            }
            else
            {
                cdkValueToErrorMessageMap.Add(validCDKey.Value, $"Failed to update the IsActivated field: {cdkUpdateResult.Message}");
            }
        }

        // mark the player who activates cdkey
        // TODO: bulkify
        foreach (var cdkey in successfullyActivatedCDKeys)
        {
            await UpdatePlayerByValue(cdkey.Value, player.DBName);
        }

        // add the game to player
        List<CDKey> successfullyAddedCDKeys = new();
        foreach (var cdKey in successfullyActivatedCDKeys)
        {
            InstanceMessage<Player> playerUpdateResult = await playerService.AddGames(
                player.DBName,
                new List<string> { cdKey.Game }
            );
            if (playerUpdateResult.Okay == 1)
            {
                successfullyAddedCDKeys.Add(cdKey);
            }
            else
            {
                cdkValueToErrorMessageMap.Add(cdKey.Value, $"Failed to add this game to player: {playerUpdateResult.Message}");
            }
        }

        // return sucessfully updated cdkeys list

        string resultMessage = "";
        if (cdkValueToErrorMessageMap.Count > 0)
        {
            resultMessage = "The following errors are detected: \n";
            foreach (var (key, value) in cdkValueToErrorMessageMap)
            {
                resultMessage += $"{key}: {value}\n";
            }
        }

        if (successfullyAddedCDKeys.Count <= 0)
        {
            return new InstanceMessage<CDKey>(
                Okay: 0,
                Message: $"No CDKeys have been activated. {resultMessage}",
                NumAffected: 0,
                Instances: null
            );
        }

        return new InstanceMessage<CDKey>(
            Okay: 1,
            Message: $"Activate CDKeys for Player with dbname = {player.DBName}: {successfullyAddedCDKeys.Count} success, {cdkValueToErrorMessageMap.Count} failure. {resultMessage}",
            NumAffected: successfullyAddedCDKeys.Count,
            Instances: successfullyAddedCDKeys
        );
    }

    public async Task<InstanceMessage<CDKey>> UpdateIsActivated(IList<string> ids, bool activate = true)
    {
        var objectIDValues = (
            from id in ids
            select ObjectId.Parse(id)
        );

        FilterDefinition<CDKey> searchCondition = Builders<CDKey>.Filter.In("_id", objectIDValues);

        UpdateDefinition<CDKey> updateToken = JsonUtils.CreateCompactLiteral($@"{{
            ""$set"": {{
                ""isActivated"": {activate.ToString().ToLower()}
            }}
        }}");

        return await Update(
            searchCondition,
            updateToken
        );
    }


    public async Task<InstanceMessage<CDKey>> UpdateAsActivated(IDictionary<string, string> cdkeyIDToPlayerNameMap, bool activate = true)
    {

        List<InstanceMessage<CDKey>> allMessages = new();

        foreach (var (key, value) in cdkeyIDToPlayerNameMap)
        {
            FilterDefinition<CDKey> searchCondition = DBUtils.BuildSingleQueryCondition<CDKey>("_id", key);
            UpdateDefinition<CDKey> updateToken = JsonUtils.CreateCompactLiteral($@"{{
                ""$set"": {{
                    ""isActivated"": {activate.ToString().ToLower()},
                    ""player"": ""{ value }""
                }}
            }}");
            InstanceMessage<CDKey> currentMessage = await Update(
                searchCondition,
                updateToken
            );
            allMessages.Add(currentMessage);
        }

        bool allOkay = true;
        long allnumAffected = 0;
        string allLocalMessages = "";
        List<CDKey> allLocalCDKeys = new();

        foreach (var message in allMessages)
        {
            allOkay = allOkay && (message.Okay == 1);
            allnumAffected += message.NumAffected;
            allLocalMessages += "|| " + message.Message;
            if (message.Instances is { })
            {
                allLocalCDKeys.AddRange(message.Instances);
            }
        }

        InstanceMessage<CDKey> entireMessage = new(
            Okay: (allOkay? (byte)1 : (byte)0),
            Message: allLocalMessages,
            NumAffected: allnumAffected,
            Instances: allLocalCDKeys
        );

        return entireMessage;
    }


    public async Task<InstanceMessage<CDKey>> UpdatePlayerByValue(string cdkValue, string newPlayerName)
    {

        FilterDefinition<CDKey> searchCondition = JsonUtils.CreateCompactLiteral($@"{{
            ""value"": ""{cdkValue}""
        }}");

        UpdateDefinition<CDKey> updateToken = JsonUtils.CreateCompactLiteral($@"{{
            ""$set"": {{
                ""player"": ""{newPlayerName}""
            }}
        }}");

        return await Update(
            searchCondition,
            updateToken
        );

    }

}
