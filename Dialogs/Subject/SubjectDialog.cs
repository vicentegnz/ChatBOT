

using System;
using System.Collections.Generic;
using System.Linq;
using ChatBOT.Bot;
using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace ChatBOT.Dialogs
{
    public sealed class SubjectDialog : WaterfallDialog
    {
        public SubjectDialog(string dialogId, IOpenDataService openDataService,IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {

            AddStep(async (stepContext, cancellationToken) =>
            {
                List<StudyCenterModel> centers = openDataService.GetStudyCenters();

                await stepContext.Context.SendActivityAsync($"La universidad de extremadura tiene muchos centros, podrías indicarme el nombre del centro del que necesitas esta información.");
                List<string> centersName = centers.Select(x => x.Name).ToList();

                return await stepContext.PromptAsync("choicePrompt", new PromptOptions
                {
                    Prompt = stepContext.Context.Activity.CreateReply($"Los centros que tengo disponibles son los siguiente {Environment.NewLine} ¿De las siguientes opciones que te gustaría consultar?"),
                    Choices = ChoiceFactory.ToChoices(centersName),
                    RetryPrompt = stepContext.Context.Activity.CreateReply("Por favor, escriba una de las siguientes centros para que te pueda ayudar en tu consulta.")
                });
          
            });

            AddStep(async (stepContext, cancellationToken) =>
            {
                var response = (stepContext.Result as FoundChoice)?.Value;

                var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);

                List<StudyCenterModel> centers = openDataService.GetStudyCenters();
                state.StudyCenterModel = centers.FirstOrDefault(x => x.Name.ToLower().Contains(response.ToLower()));

                await stepContext.Context.SendActivityAsync($"Aquí tienes la página web donde encontraras mucha información de este centro. {state.StudyCenterModel.Url}");
                var degrees = state.StudyCenterModel.Degrees.Select(x => x.Name.ToLower()).ToList();

                return await stepContext.PromptAsync("choicePrompt",
                new PromptOptions
                {
                    Prompt = stepContext.Context.Activity.CreateReply($"Para el centro {state.StudyCenterModel.Name.ToLower()}, tengo disponibles estos grados: "),
                    Choices = ChoiceFactory.ToChoices(degrees),
                    RetryPrompt = stepContext.Context.Activity.CreateReply("Por favor, escriba una de las siguientes grados para que te pueda ayudar en tu consulta.")
                });
            });

            AddStep(async (stepContext, cancellationToken) =>
            {
                var response = (stepContext.Result as FoundChoice)?.Value;
                var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
                state.DegreeCenterModel = state.StudyCenterModel.Degrees.FirstOrDefault(x => x.Name.ToLower().Contains(response.ToLower()));

                await stepContext.Context.SendActivityAsync($"Aquí tienes la página web donde encontraras mucha información de este grado. {state.DegreeCenterModel.Url}");
                var subjects = state.DegreeCenterModel.Subjects.Select(x => x.Name).ToList();

                return await stepContext.PromptAsync("choicePrompt",
                new PromptOptions
                {
                    Prompt = stepContext.Context.Activity.CreateReply($"Para este grado {state.DegreeCenterModel.Name.ToLower()}, tengo disponibles las siguientes asignaturas: "),
                    Choices = ChoiceFactory.ToChoices(subjects),
                    RetryPrompt = stepContext.Context.Activity.CreateReply("Por favor, escriba una de las siguientes asignaturas para que te pueda ayudar en tu consulta.")
                });
            });

            AddStep(async (stepContext, cancellationToken) =>
            {
                var response = (stepContext.Result as FoundChoice)?.Value;
                var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
                state.SubjectModel = state.DegreeCenterModel.Subjects.FirstOrDefault(x => x.Name.ToLower().Contains(response.ToLower()));

                await stepContext.Context.SendActivityAsync($"Aquí tienes la página web donde encontraras mucha información de esta asignatura. {state.SubjectModel.InfoUrl}");

                return await stepContext.ReplaceDialogAsync(MainLuisDialog.Id, cancellationToken);

            });


        }

        public new static string Id => "Subjectdialog";
    }
}
