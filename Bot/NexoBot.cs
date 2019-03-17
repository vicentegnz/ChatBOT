using System.Threading;
using System.Threading.Tasks;
using ChatBot.Services;
using ChatBOT.Core;
using ChatBOT.Dialogs;
using ChatBOT.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace ChatBOT.Bot
{
    public class NexoBot : IBot
    {
        //#region "Consts"

        private const string WelcomeText = "¿te puedo ayudar en algo?";
        //private const string LuisKey = "HelpService";
        //private const string QnaKey = "FrequentlyAskedQuestions";


        //private const string UNKNOWN_INTENT_LUIS = "None";


        //#endregion

        //#region "Properties"

        private readonly BotServices _services;
        private readonly ISpellCheckService _spellCheck;
        private readonly ISearchService _searchService;

        //#endregion

        #region "Constructor"
        public NexoBot(NexoBotAccessors nexoBotAccessors, BotServices services, ISpellCheckService spellCheck,ISearchService searchService)
        {
            var dialogState = nexoBotAccessors.DialogStateAccessor;
            Dialogs = new DialogSet(dialogState);
            Dialogs.Add(MainDialog.Instance);
            Dialogs.Add(new QuestionDialog(QuestionDialog.Id, services, spellCheck,searchService));
            Dialogs.Add(new ChoicePrompt("choicePrompt"));
            Dialogs.Add(new TextPrompt("textPrompt"));
            Dialogs.Add(new NumberPrompt<int>("numberPrompt"));

            NexoBotAccessors = nexoBotAccessors;
        }

        #endregion

        public NexoBotAccessors NexoBotAccessors { get; }
        private readonly DialogSet Dialogs;

        #region "Public Methods"

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {

            if(turnContext.Activity.Type == ActivityTypes.Message)
            { 
                await NexoBotAccessors.NexoBotStateStateAccessor.GetAsync(turnContext, () => new NexoBotState(), cancellationToken);

                turnContext.TurnState.Add("NexoBotAccessors", NexoBotAccessors);

                var dialogCtx = await Dialogs.CreateContextAsync(turnContext, cancellationToken);

                if(dialogCtx.ActiveDialog != null)
                    await dialogCtx.ContinueDialogAsync(cancellationToken);
                else
                    await dialogCtx.BeginDialogAsync(MainDialog.Id, cancellationToken);

                await NexoBotAccessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

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
