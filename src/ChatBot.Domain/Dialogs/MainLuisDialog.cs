using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Domain.Bot;
using ChatBot.Domain.Configuration;
using ChatBot.Domain.Core;
using ChatBot.Domain.Dialogs.Language;
using ChatBot.Domain.Dialogs.Question;
using ChatBot.Domain.Dialogs.Subject;
using ChatBot.Domain.Dialogs.Teacher;
using ChatBot.Domain.Dialogs.UnexFacilities;
using ChatBot.Domain.Services.IA;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace ChatBot.Domain.Dialogs
{
    
    public class MainLuisDialog : BaseDialog
    {
        #region "Properties"
        private readonly BotServices _services;
        private readonly Templates _lgEngine;
        #endregion

        public MainLuisDialog(
            BotServices botServices,
            ITeacherService teacherService,
            IOpenDataService openDataService,
            ISearchService searchService,
            ISpellCheckService spellCheckService,
            IUnexFacilitiesService unexFacilitiesService,
            string dialogId = null,
            IEnumerable<WaterfallStep> steps = null) : base(dialogId ?? nameof(MainLuisDialog))
        {
            string fullPath = Path.Combine(new string[]{ ".", ".", "Resources", "MainLuisDialog.lg" });
            _lgEngine = Templates.ParseFile(fullPath);

            _services = botServices ?? throw new ArgumentNullException(nameof(botServices));

            if (!_services.LuisServices.ContainsKey(LuisServiceConfiguration.LuisKey))
                throw new ArgumentException($"La configuración no es correcta. Por favor comprueba que existe en tu fichero '.bot' un servicio LUIS llamado '{LuisServiceConfiguration.LuisKey}'.");

            AddDialog(new LanguageNotValidDialog(nameof(LanguageNotValidDialog)));
            AddDialog(new GoodByeDialog(nameof(GoodByeDialog)));
            AddDialog(new HelpDialog(nameof(HelpDialog)));
            AddDialog(new GratitudeDialog(nameof(GratitudeDialog)));
            AddDialog(new NegationDialog(nameof(NegationDialog)));
            AddDialog(new QuestionDialog(nameof(QuestionDialog), botServices, spellCheckService, searchService));
            AddDialog(new SubjectDialog(nameof(SubjectDialog), openDataService));
            AddDialog(new TeacherDialog(nameof(TeacherDialog), teacherService));
            AddDialog(new UnexFacilitiesDialog(nameof(UnexFacilitiesDialog), unexFacilitiesService));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                            IntroStepAsync,
                            ActStepAsync,
                            FinalStepAsync,
            }));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string message;
            var recognizerResult = await _services.LuisServices[LuisServiceConfiguration.LuisKey].RecognizeAsync(stepContext.Context, cancellationToken);
            var topIntent = recognizerResult?.GetTopScoringIntent();
            var state = await(stepContext.Context.TurnState[nameof(NexoBotAccessors)] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
            
            if (topIntent != null && LuisServiceConfiguration.HelloIntent == topIntent.Value.intent)
            {
                message = state.Messages.Any() ?  _lgEngine.Evaluate("GreetingsAndAskAgain").ToString() : _lgEngine.Evaluate("MainGreeting").ToString();
            }
            else
            {

                if (!state.Messages.Any())
                {
                    await BeginDialogByIntent(stepContext, topIntent);
                }

                if (topIntent != null)
                    message = topIntent.Value.intent == LuisServiceConfiguration.OkIntent ? _lgEngine.Evaluate("ConfirmAskAgain").ToString() : _lgEngine.Evaluate("AskAgain").ToString();
                else
                    message = _lgEngine.Evaluate("AskAgain").ToString();
            }

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions {Prompt = MessageFactory.Text(message)}, cancellationToken);
        }


        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;

            var state = await(stepContext.Context.TurnState[nameof(NexoBotAccessors)] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
            var recognizerResult = await _services.LuisServices[LuisServiceConfiguration.LuisKey].RecognizeAsync(stepContext.Context, cancellationToken);
            var topIntent = recognizerResult?.GetTopScoringIntent();
            state.Messages.Add(stepContext.Result.ToString());

            if (topIntent != null)
            {
                return topIntent.Value.intent == LuisServiceConfiguration.OkIntent
                ? await stepContext.ReplaceDialogAsync(nameof(MainLuisDialog), null, cancellationToken)
                : await BeginDialogByIntent(stepContext, topIntent);
            }

            await stepContext.Context.SendActivityAsync(_lgEngine.Evaluate("SomethingWentWrong").ToString());
            return await stepContext.EndDialogAsync();

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync();
        }

    }
}
