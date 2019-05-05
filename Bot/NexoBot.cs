using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Services;
using ChatBOT.Core;
using ChatBOT.Dialogs;
using ChatBOT.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace ChatBOT.Bot
{
    public class NexoBot : IBot
    {
        #region "Properties"

        public NexoBotAccessors _nexoBotAccessors { get; }
        private readonly DialogSet _dialogs;
        
        #endregion


        #region "Constructor"
        public NexoBot(NexoBotAccessors nexoBotAccessors, BotServices services, 
            ISpellCheckService spellCheck,
            ISearchService searchService, 
            ITeacherService teacherService)
        {
            var dialogState = nexoBotAccessors.DialogStateAccessor;
            _dialogs = new DialogSet(dialogState);
            _dialogs.Add(new MainLuisDialog(MainLuisDialog.Id, services));
            _dialogs.Add(new QuestionDialog(QuestionDialog.Id, services, spellCheck,searchService));
            _dialogs.Add(new TeacherDialog(TeacherDialog.Id, teacherService));

            //SOLO VISUALIZAN TEXTO
            _dialogs.Add(new HelloDialog(HelloDialog.Id));
            _dialogs.Add(new HelpDialog(HelpDialog.Id));
            _dialogs.Add(new LanguageNotValidDialog(LanguageNotValidDialog.Id));
            _dialogs.Add(new GratitudeDialog(GratitudeDialog.Id));
            _dialogs.Add(new GoodByeDialog(GoodByeDialog.Id));
            _dialogs.Add(new NegationDialog(NegationDialog.Id));

            _dialogs.Add(new ChoicePrompt("choicePrompt"));
            _dialogs.Add(new TextPrompt("textPrompt"));
            _dialogs.Add(new NumberPrompt<int>("numberPrompt"));

            _nexoBotAccessors = nexoBotAccessors;
        }

        #endregion

        #region "Public Methods"

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(turnContext.Activity.Type == ActivityTypes.Message)
            { 
                await _nexoBotAccessors.NexoBotStateStateAccessor.GetAsync(turnContext, () => new NexoBotState(), cancellationToken);

                turnContext.TurnState.Add("NexoBotAccessors", _nexoBotAccessors);

                DialogContext dialogCtx = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                DialogTurnResult results = await dialogCtx.ContinueDialogAsync(cancellationToken);

                switch (results.Status)
                {
                    case DialogTurnStatus.Cancelled:
                    case DialogTurnStatus.Empty:
                        // If there is no active dialog, we should clear the user info and start a new dialog.

                        await _nexoBotAccessors.NexoBotStateStateAccessor.SetAsync(turnContext, new NexoBotState(), cancellationToken);
                        await _nexoBotAccessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                        await dialogCtx.BeginDialogAsync(MainLuisDialog.Id, cancellationToken);
                        break;

                    case DialogTurnStatus.Complete:
                        // If we just finished the dialog, capture and display the results.
                        NexoBotState userInfo = results.Result as NexoBotState;
                        await _nexoBotAccessors.NexoBotStateStateAccessor.SetAsync(turnContext, userInfo, cancellationToken);
                        await _nexoBotAccessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                        await dialogCtx.BeginDialogAsync(MainLuisDialog.Id, cancellationToken);

                        break;
                    case DialogTurnStatus.Waiting:
                        // If there is an active dialog, we don't need to do anything here.
                        break;
                }

                await _nexoBotAccessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

            }
            //else
            //{
            //    if(turnContext.Activity.Type == ActivityTypes.ConversationUpdate && turnContext.Activity.MembersAdded != null)
            //    {
            //         await SendWelcomeMessageAsync(turnContext, cancellationToken);
            //    }
            //}
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
                        $"Hola {member.Name}, soy Nexo 🤖 un asistente virtual de la Unex.",
                        cancellationToken: cancellationToken);
                }
            }
        }

        #endregion



    }
}
