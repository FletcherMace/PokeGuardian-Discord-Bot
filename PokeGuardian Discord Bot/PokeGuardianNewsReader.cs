using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using DSharpPlus;

namespace PokeGuardian_Discord_Bot
{
    internal class PokeGuardianNewsReader
    {
        private static async Task Main(string[] args)
        {
            try
            {
                var token = ConfigurationManager.AppSettings.Get("Token");
                var channelId = ConfigurationManager.AppSettings.Get("ChannelId");
                var xmlUrlString = ConfigurationManager.AppSettings.Get("XmlUrl");
                var lastPostUrl = ConfigurationManager.AppSettings.Get("LastUrl");
                var currentPostUrl = "";
                var nodeCounter = 0;

                var discordClient = new DiscordClient(new DiscordConfiguration
                {
                    Token = token,
                    TokenType = TokenType.Bot
                });

                await discordClient.ConnectAsync();

                var reader = new XmlTextReader(xmlUrlString);
                while (reader.Read())
                {
                    if (nodeCounter != 149)
                    {
                        nodeCounter++;
                    }
                    else
                    {
                        currentPostUrl = reader.Value;
                        break;
                    }
                }

                if (currentPostUrl != lastPostUrl)
                {
                    await PostNewLink(discordClient, channelId, currentPostUrl);

                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                    config.AppSettings.Settings["LastUrl"].Value = currentPostUrl;
                    config.Save(ConfigurationSaveMode.Modified);
                }

            }
            catch
            {
                // ignored
            }
        }


        private static async Task PostNewLink(DiscordClient client, string channelId, string postUrl)
        {
            var splitTitle = postUrl.Split("_");
            var postTitle = splitTitle[1].Replace("-", " ");
            postTitle = Regex.Replace(postTitle, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
            var channel = await client.GetChannelAsync(Convert.ToUInt64(channelId));
            await channel.SendMessageAsync(postTitle + ": \n" + postUrl);
        }
    }
}
