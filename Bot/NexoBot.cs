using System.Threading;
using System.Threading.Tasks;
using ChatBot.Services;
using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ChatBOT.Bot
{
    public class NexoBot : IBot
    {
        #region "Consts"

        private const string WelcomeText = "¿te puedo ayudar en algo?";
        private const string LuisKey = "HelpService";
        private const string QnaKey = "FrequentlyAskedQuestions";


        private const string UNKNOWN_INTENT_LUIS = "None";


        #endregion

        #region "Properties"

        private readonly BotServices _services;
        //private readonly ISpellCheckService _spellCheck;
        private readonly ISearchService _searchService;

        #endregion

        #region "Constructor"
        public NexoBot(BotServices services, ISpellCheckService spellCheck, ISearchService searchService)
        {
            _services = services ?? throw new System.ArgumentNullException(nameof(services));

            if (!_services.LuisServices.ContainsKey(LuisKey))
            {
                throw new System.ArgumentException($"La configuración no es correcta. Por favor comprueba que existe en tu fichero '.bot' un servicio LUIS llamado '{LuisKey}'.");
            }

            if (!_services.QnAServices.ContainsKey(QnaKey))
            {
                throw new System.ArgumentException($"La configuración no es correcta.Por favor comprueba que existe en tu fichero '.bot' un servicio Qna llamado '{QnaKey}'.");
            }

            _spellCheck = spellCheck;
            _searchService = searchService;
        }

        #endregion
        #region "Public Methods"

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {

            switch (turnContext.Activity.Type) {

                case ActivityTypes.Message:

                    //Spell Check 
                    //turnContext.Activity.Text = _spellCheck.GetSpellCheckFromMessage(turnContext.Activity.Text);
                
                    //LUIS
                    var recognizerResult = await _services.LuisServices[LuisKey].RecognizeAsync(turnContext, cancellationToken);
                    var topIntent = recognizerResult?.GetTopScoringIntent();
                    if (topIntent != null && topIntent.HasValue)
                    {
                        string message = string.Empty;
                        if (topIntent.Value.intent == UNKNOWN_INTENT_LUIS)
                        {
                            //Qna
                            var response = await _services.QnAServices[QnaKey].GetAnswersAsync(turnContext);

                            if (response != null && response.Length > 0)
                                message = response[0].Answer;
                            else
                            {
                                SearchResponseModel searchResponse = await _searchService.GetResultFromSearch(turnContext.Activity.Text);
                                if (searchResponse != null)
                                    message = $"{searchResponse.Description}\n{searchResponse.Url}";
                            }
                        }
                        else
                        {
                            // message = LEER DE CONFIGURACION
                        }

                        await turnContext.SendActivityAsync(message);

                    }

                        break;

                case ActivityTypes.ConversationUpdate:

                    if (turnContext.Activity.MembersAdded != null)
                        await SendWelcomeMessageAsync(turnContext, cancellationToken);

                    break;
            }
        }

        #endregion

        #region "Private Methods"

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        $"Hola {member.Name}, mi nombre es Nexo {WelcomeText}",
                        cancellationToken: cancellationToken);
                }
            }
        }
       
        #endregion
    }
}
