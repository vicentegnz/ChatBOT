using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatBot.Services;
using ChatBOT.Bot;
using ChatBOT.Conf;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ChatBOT.Dialogs
{
    
    public class MainLuisDialog : BaseDialog
    {
        #region "Properties"
        private readonly BotServices _services;
        #endregion

        public MainLuisDialog(string dialogId, BotServices botServices, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {

            _services = botServices ?? throw new ArgumentNullException(nameof(botServices));

            if (!_services.LuisServices.ContainsKey(LuisServiceConfiguration.LuisKey))
                throw new ArgumentException($"La configuración no es correcta. Por favor comprueba que existe en tu fichero '.bot' un servicio LUIS llamado '{LuisServiceConfiguration.LuisKey}'.");

            AddStep(async (stepContext, cancellationToken) =>
            {

                var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
                var message = $"¿En que te puedo ayudar?";
              
                if (state.Messages.Any())
                {
                    var recognizerResult = await _services.LuisServices[LuisServiceConfiguration.LuisKey].RecognizeAsync(stepContext.Context, cancellationToken);
                    var topIntent = recognizerResult?.GetTopScoringIntent();

                    if (topIntent != null)
                        message = topIntent.Value.intent == LuisServiceConfiguration.OkIntent ? $"Perfecto, pues dime en que más te puedo ayudar." : $"¿Necesitas algo más?";
                    else
                        message = "¿Quieres que te ayude en algo más?";
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
                var recognizerResult = await _services.LuisServices[LuisServiceConfiguration.LuisKey].RecognizeAsync(stepContext.Context, cancellationToken);
                var topIntent = recognizerResult?.GetTopScoringIntent();
                state.Messages.Add(stepContext.Result.ToString());

                if (topIntent != null)
                {
                    return await DialogByIntent(stepContext, topIntent);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("Ha ocurrido un error, intentelo de nuevo.");
                }

                return await stepContext.NextAsync();
            });

            AddStep(async (stepContext, cancellationToken) => { return await stepContext.EndDialogAsync(); });
        }
        
        public new static string Id => "mainLuisDialog";
    }
}
