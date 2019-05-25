using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Services;
using ChatBOT.Core;
using ChatBOT.Dialogs;
using ChatBOT.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace ChatBOT.Bot
{
    public class NexoBot<T> : ActivityHandler where T : Dialog
    {
        #region "Properties"

        public NexoBotAccessors _nexoBotAccessors { get; }

        //private readonly DialogSet _dialogs;


        protected readonly Dialog _dialog;
        protected readonly BotState _conversationState;
        protected readonly BotState _userState;
        protected readonly ILogger _logger;

        #endregion


        #region "Constructor"
        public NexoBot(ConversationState conversationState, UserState userState, T dialog, ILogger<NexoBot<T>> logger)
        {

            _conversationState = conversationState;
            _userState = userState;
            _dialog = dialog;
            _logger = logger;
            _nexoBotAccessors = new NexoBotAccessors(conversationState)
            {
                DialogStateAccessor = _conversationState.CreateProperty<DialogState>(NexoBotAccessors.DialogStateAccessorName),
                NexoBotStateStateAccessor = _conversationState.CreateProperty<NexoBotState>(NexoBotAccessors.NexoBotStateAccesorName),
            };
            //var dialogState = nexoBotAccessors.DialogStateAccessor;
            //_dialogs = new DialogSet(dialogState);
            //_dialogs.Add(new MainLuisDialog(MainLuisDialog.Id, services));
            //_dialogs.Add(new QuestionDialog(QuestionDialog.Id, services, spellCheck, searchService));
            //_dialogs.Add(new TeacherDialog(TeacherDialog.Id, teacherService));
            //_dialogs.Add(new SubjectDialog(SubjectDialog.Id, openDataService.FirstOrDefault(x => x.GetType() == typeof(OpenDataCacheService))));


            ////SOLO VISUALIZAN TEXTO
            //_dialogs.Add(new HelpDialog(HelpDialog.Id));
            //_dialogs.Add(new LanguageNotValidDialog(LanguageNotValidDialog.Id));
            //_dialogs.Add(new GratitudeDialog(GratitudeDialog.Id));
            //_dialogs.Add(new GoodByeDialog(GoodByeDialog.Id));
            //_dialogs.Add(new NegationDialog(NegationDialog.Id));

            //ChoicePrompt choicePrompt = new ChoicePrompt(nameof(ChoicePrompt));
            //choicePrompt.ChoiceOptions = new ChoiceFactoryOptions { IncludeNumbers = false };
            //choicePrompt.RecognizerOptions = new FindChoicesOptions { AllowPartialMatches = true };
            //_dialogs.Add(choicePrompt);
            //_dialogs.Add(new TextPrompt(nameof(TextPrompt)));
            //_dialogs.Add(new NumberPrompt<int>(nameof(NumberPrompt<int>)));

        }

        #endregion

        #region "Public Methods"

        //public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        //{

        //        if (turnContext.Activity.Type == ActivityTypes.Message)
        //        {
        //            await _nexoBotAccessors.NexoBotStateStateAccessor.GetAsync(turnContext, () => new NexoBotState(), cancellationToken);

        //            turnContext.TurnState.Add("NexoBotAccessors", _nexoBotAccessors);

        //            DialogContext dialogCtx = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
        //            DialogTurnResult results = await dialogCtx.ContinueDialogAsync(cancellationToken);

        //            switch (results.Status)
        //            {
        //                case DialogTurnStatus.Cancelled:
        //                case DialogTurnStatus.Empty:
        //                    // If there is no active dialog, we should clear the user info and start a new dialog.

        //                    await _nexoBotAccessors.NexoBotStateStateAccessor.SetAsync(turnContext, new NexoBotState(), cancellationToken);
        //                    await _nexoBotAccessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        //                    await dialogCtx.BeginDialogAsync(MainLuisDialog.Id, null, cancellationToken);
        //                    break;

        //                case DialogTurnStatus.Complete:
        //                    // If we just finished the dialog, capture and display the results.
        //                    NexoBotState userInfo = results.Result as NexoBotState;
        //                    await _nexoBotAccessors.NexoBotStateStateAccessor.SetAsync(turnContext, userInfo, cancellationToken);
        //                    await _nexoBotAccessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        //                    await dialogCtx.BeginDialogAsync(MainLuisDialog.Id, null, cancellationToken);

        //                    break;
        //                case DialogTurnStatus.Waiting:
        //                    // If there is an active dialog, we don't need to do anything here.
        //                    break;
        //            }

        //            await _nexoBotAccessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

        //        }
        //}

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _nexoBotAccessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

        }


        #endregion

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");

            await _nexoBotAccessors.NexoBotStateStateAccessor.GetAsync(turnContext, () => new NexoBotState(), cancellationToken);
            turnContext.TurnState.Add(nameof(NexoBotAccessors), _nexoBotAccessors);

            await _nexoBotAccessors.NexoBotStateStateAccessor.SetAsync(turnContext, new NexoBotState(), cancellationToken);
            await _nexoBotAccessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _dialog.Run(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

    }
}
