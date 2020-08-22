using System;
using System.IO;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace ChatBot.Domain.Core.Errors
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        private readonly Templates _lgEngine;

        public AdapterWithErrorHandler(ICredentialProvider credentialProvider, ILogger<BotFrameworkHttpAdapter> logger, ConversationState conversationState = null)
            : base(credentialProvider)
        {
            // combine path for cross platform support
            string[] paths = { ".","Core" ,"Errors","AdapterWithErrorHandler.lg" };
            string fullPath = Path.Combine(paths);
            _lgEngine = Templates.ParseFile(fullPath);

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync(_lgEngine.Evaluate("SomethingWentWrong").ToString());

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };
        }
    }
}
