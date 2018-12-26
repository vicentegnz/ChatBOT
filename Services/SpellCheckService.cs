using System;
using System.Linq;
using ChatBOT.Core;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck;

namespace ChatBOT.Services
{
    public class SpellCheckService : ISpellCheckService
    {
        private const string MODE_SPELLCHECK = "spell";
        private const string LANGUAGE_SPELLCHECK = "es-ES";

        private readonly SpellCheckClient spellCheckClient;

        public SpellCheckService()
        {
            spellCheckClient = new SpellCheckClient(new ApiKeyServiceClientCredentials("5363528dc54e4611a206afcf2dcad502"));
        }

        public string GetSpellCheckFromMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            try
            {
                var result = spellCheckClient.SpellCheckerWithHttpMessagesAsync(text: message, mode: MODE_SPELLCHECK, acceptLanguage: LANGUAGE_SPELLCHECK).Result; 

                if (result?.Body.FlaggedTokens?.Count > 0)
                {
                    var firstspellCheckResult = result.Body.FlaggedTokens.FirstOrDefault();

                    if (firstspellCheckResult != null)
                    {
                        var suggestions = firstspellCheckResult.Suggestions;
                        if (suggestions?.Count > 0)
                        {
                            return suggestions.FirstOrDefault().Suggestion;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return message;
            }


            return message;
        }


    }
}
