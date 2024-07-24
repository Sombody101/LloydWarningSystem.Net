using DSharpPlus;
using Spectre.Console;

namespace LloydWarningSystem.Net.Services;

public class RandomMinecraftSender
{
    private readonly DiscordClient _client;
    private Random _random = new();

    public RandomMinecraftSender(DiscordClient client)
    {
        _client = client;
    }

    public async Task StartSendingMessages()
    {
        while (true)
        {
            var timeDelay = _random.Next(1, 200);
            Logging.Log($"Waiting {timeDelay} minutes before next Minecraft message.");

            await Task.Delay(TimeSpan.FromMinutes(timeDelay));

            try
            {
                var channel = await _client.GetChannelAsync(1265458513140912289);

                if (channel is null)
                {
                    Logging.LogError("Channel not found.");
                    continue;
                }

                var message = $"MINECRAFT\n@everyone\nMINECRAFT";
                await channel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Logging.LogError("Error sending scheduled message.");
                AnsiConsole.WriteException(ex);
            }
        }
    }
}
