

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatBOT.Bot;
using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace ChatBOT.Dialogs
{
    public sealed class SubjectDialog : ComponentDialog
    {
        private readonly IOpenDataService _openDataService;

        public SubjectDialog(string dialogId, IOpenDataService openDataService, IEnumerable<WaterfallStep> steps = null) : base(dialogId)
        {
            _openDataService = openDataService;

            ChoicePrompt choicePrompt = new ChoicePrompt(nameof(ChoicePrompt));
            choicePrompt.ChoiceOptions = new ChoiceFactoryOptions { IncludeNumbers = false };
            choicePrompt.RecognizerOptions = new FindChoicesOptions { AllowPartialMatches = true };
            AddDialog(choicePrompt);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
              {
                                        QuestionCenterStepAsync,
                                        QuestionDegreeStepAsync,
                                        QuestionSubjectStepAsync,
                                        EndQuestionStepAsync,

              }));

            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> QuestionCenterStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            List<StudyCenterModel> centers = _openDataService.GetStudyCenters();

            await stepContext.Context.SendActivityAsync($"La universidad de extremadura tiene muchos centros, podrías indicarme el nombre del centro del que necesitas esta información.");
            List<string> centersName = centers.Select(x => x.Name).ToList();

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = stepContext.Context.Activity.CreateReply($"Los centros que tengo disponibles son los siguiente {Environment.NewLine} ¿De las siguientes opciones que te gustaría consultar?"),
                Choices = ChoiceFactory.ToChoices(centersName),
                RetryPrompt = stepContext.Context.Activity.CreateReply("Por favor, escriba una de las siguientes centros para que te pueda ayudar en tu consulta.")
            });
        }

        private async Task<DialogTurnResult> QuestionDegreeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var response = (stepContext.Result as FoundChoice)?.Value;

            var state = await (stepContext.Context.TurnState[nameof(NexoBotAccessors)] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);

            List<StudyCenterModel> centers = _openDataService.GetStudyCenters();
            state.StudyCenterModel = centers.FirstOrDefault(x => x.Name.ToLower().Contains(response.ToLower()));

            await stepContext.Context.SendActivityAsync($"Aquí tienes la página web donde encontraras mucha información de este centro. {state.StudyCenterModel.Url}");
            var degrees = state.StudyCenterModel.Degrees.Select(x => x.Name.ToLower()).ToList();

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
            new PromptOptions
            {
                Prompt = stepContext.Context.Activity.CreateReply($"Para el centro {state.StudyCenterModel.Name.ToLower()}, tengo disponibles estos grados: "),
                Choices = ChoiceFactory.ToChoices(degrees),
                RetryPrompt = stepContext.Context.Activity.CreateReply("Por favor, escriba una de las siguientes grados para que te pueda ayudar en tu consulta.")
            });
        }

        private async Task<DialogTurnResult> QuestionSubjectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var response = (stepContext.Result as FoundChoice)?.Value;
            var state = await (stepContext.Context.TurnState[nameof(NexoBotAccessors)] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
            state.DegreeCenterModel = state.StudyCenterModel.Degrees.FirstOrDefault(x => x.Name.ToLower().Contains(response.ToLower()));

            await stepContext.Context.SendActivityAsync($"Aquí tienes la página web donde encontraras mucha información de este grado. {state.DegreeCenterModel.Url}");
            var subjects = state.DegreeCenterModel.Subjects.Select(x => x.Name).ToList();

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
            new PromptOptions
            {
                Prompt = stepContext.Context.Activity.CreateReply($"Para este grado {state.DegreeCenterModel.Name.ToLower()}, tengo disponibles las siguientes asignaturas: "),
                Choices = ChoiceFactory.ToChoices(subjects),
                RetryPrompt = stepContext.Context.Activity.CreateReply("Por favor, escriba una de las siguientes asignaturas para que te pueda ayudar en tu consulta.")
            });
        }

        private async Task<DialogTurnResult> EndQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var response = (stepContext.Result as FoundChoice)?.Value;
            var state = await (stepContext.Context.TurnState[nameof(NexoBotAccessors)] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
            state.SubjectModel = state.DegreeCenterModel.Subjects.FirstOrDefault(x => x.Name.ToLower().Contains(response.ToLower()));

            await stepContext.Context.SendActivityAsync($"Aquí tienes la página web donde encontraras mucha información de esta asignatura. {state.SubjectModel.InfoUrl}");

            return await stepContext.ReplaceDialogAsync(nameof(MainLuisDialog), null, cancellationToken);

        }

    }
}
