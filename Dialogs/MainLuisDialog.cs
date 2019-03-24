using System;
using System.Collections.Generic;
using System.Linq;
using ChatBot.Services;
using ChatBOT.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace ChatBOT.Dialogs
{
    
    public class MainLuisDialog : WaterfallDialog
    {
        #region "Consts"
        private const string LuisKey = "HelpService";

        private const string SCHEDULE_INTENT_LUIS = "Horario";
        private const string SUBJECT_INTENT_LUIS = "Asignatura";
        private const string TEACHER_INTENT_LUIS = "Profesor";
        private const string LANGUAGE_INTENT_LUIS = "LenguajeNoAdecuado";
        private const string GREETINS_INTENT_LUIS = "Agradecimientos";
        private const string HELP_INTENT_LUIS = "Ayuda";
        private const string UNKNOWN_INTENT_LUIS = "None";
        private const string GOODBYE_INTENT_LUIS = "Despedida";
        
        #endregion

        #region "Properties"
        private readonly BotServices _services;
        #endregion

        public MainLuisDialog(string dialogId, BotServices botServices, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {

            _services = botServices ?? throw new ArgumentNullException(nameof(botServices));

            if (!_services.LuisServices.ContainsKey(LuisKey))
                throw new ArgumentException($"La configuración no es correcta. Por favor comprueba que existe en tu fichero '.bot' un servicio LUIS llamado '{LuisKey}'.");

            AddStep(async (stepContext, cancellationToken) =>
            {

                var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
                var message = string.Empty;

                if (state.Messages.Any())
                {
                    message = state.Messages.LastOrDefault() == "si" ? $"Perfecto, pues dime en que más te puedo ayudar." : $"¿Necesitas algo más?";
                }
                else
                {
                    message = $"¿En que te puedo ayudar? En el caso de que no sepas de que información dispongo pideme ayuda y te explicaré que cosas puedo hacer.";
                }

                return await stepContext.PromptAsync("textPrompt",
                    new PromptOptions
                    {
                        Prompt = stepContext.Context.Activity.CreateReply(message),
                        RetryPrompt = stepContext.Context.Activity.CreateReply("¿No he entendido tu pregunta, podrías repetirla de otra forma?")
                    });
            });


            AddStep(async (stepContext, cancellationToken) =>
            {
                var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
                var recognizerResult = await _services.LuisServices[LuisKey].RecognizeAsync(stepContext.Context, cancellationToken);
                var topIntent = recognizerResult?.GetTopScoringIntent();
                state.Messages.Add(stepContext.Result.ToString());

                if (topIntent != null)
                {
                    switch (topIntent.Value.intent)
                    {
                        case SUBJECT_INTENT_LUIS:
                            return await stepContext.BeginDialogAsync(SubjectDialog.Id);
                        case TEACHER_INTENT_LUIS:
                            return await stepContext.BeginDialogAsync(TeacherDialog.Id);
                        case SCHEDULE_INTENT_LUIS:
                            return await stepContext.BeginDialogAsync(ScheduleDialog.Id);
                        case UNKNOWN_INTENT_LUIS:
                            return await stepContext.BeginDialogAsync(QuestionDialog.Id);
                        case LANGUAGE_INTENT_LUIS:
                        case GREETINS_INTENT_LUIS:
                        case HELP_INTENT_LUIS:
                        //TODO
                        case GOODBYE_INTENT_LUIS:
                        //TODO
                        default:
                            return await stepContext.NextAsync();
                    }
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("Ha ocurrido un error, intentelo de nuevo.");
                }

                return await stepContext.NextAsync();
            });

            AddStep(async (stepContext, cancellationToken) => { return await stepContext.EndDialogAsync(); });
        }


        public static string Id => "mainLuisDialog";
    }
}
