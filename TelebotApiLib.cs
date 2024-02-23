using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace telebotApiLib;

public sealed class TelegramAPIBot: IPolling
{
    private string _token;

    private int _offset;

    private HttpClient _httpClient;

    //private string _tokenUrlPart;

    //private readonly string _apiUrlPart = "https://api.telegram.org/";

    public TelegramAPIBot(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);

        _token = token;

        _offset = 0;

        _httpClient = new HttpClient();
    }

    public async Task PollingAsync(int timeout)
    {
        await LongPollingAsync(timeout, 100, []);
    }

    public async Task PollingAsync(byte limit)
    {
        await LongPollingAsync(0, limit, []);
    }

    public async Task PollingAsync(string[] allowedUpdates)
    {
        await LongPollingAsync(0, 0, allowedUpdates);
    }

    public async Task PollingAsync(int timeout, byte limit, string[] allowedUpdates)
    {
        await LongPollingAsync(timeout, limit, allowedUpdates);
    }

    private async Task LongPollingAsync(int timeout, byte limit, string[] allowedUpdates)
    {
        Console.WriteLine("Start polling!");

        while (true)
        {
            await GetUpdates(timeout, limit, allowedUpdates);
        }
    }

    private async Task GetUpdates(int timeout, byte limit, string[] allowedUpdates)
    {
        string allowedUpdatesAsString = string.Join(",", allowedUpdates);
        string url = $"https://api.telegram.org/" +
                     $"bot{_token}/" +
                     $"getUpdates?" +
                     $"timeout={timeout}" +
                     $"&limit={limit}" +
                     $"&offset={_offset}" +
                     $"&allowed_updates={allowedUpdatesAsString}";

        try
        {
            string response = await _httpClient.GetStringAsync(url);
            UpdateResponse? updateResponse = JsonSerializer.Deserialize<UpdateResponse>(response);

            Console.WriteLine(response);

            foreach (Update update in updateResponse.result)
            {
                _offset = update.UpdateId + 1;
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }
}

class UpdateResponse
{
    public Update[] result { get; set; }
}

class Update
{
    [JsonPropertyName("update_id")]
    public int UpdateId { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; }
}

class Message
{
    [JsonPropertyName("message_id")]
    public int MessageId { get; set; }

    [JsonPropertyName("from")]
    public User From { get; set; }

    [JsonPropertyName("chat")]
    public Chat Chat { get; set; }

    [JsonPropertyName("date")]
    public int Date { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("entities")]
    public Entity[] Entities { get; set; }
}

class User
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string LastName { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }
}

class Chat
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string LastName { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }
}

class Entity
{
    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("length")]
    public int Length { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}

class GetUpdates
{
    [JsonPropertyName("timeout")]
    public int Timeout { get; set; }

    //[JsonPropertyName("offset")]
    //public int Offset { get; set; }

    [JsonPropertyName("limit")]
    public byte Limit { get; set; }

    [JsonPropertyName("allowed_updates")]
    public string[] AllowedUpdates { get; set; }
}

public interface IPolling
{
    public Task PollingAsync(int timeout);
    public Task PollingAsync(byte limit);
    public Task PollingAsync(string[] allowedUpdates);
    public Task PollingAsync(int timeout, byte limit, string[] allowedUpdates);    
}
