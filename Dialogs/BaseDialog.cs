using ChatBOT.Conf;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public class BaseDialog : ComponentDialog
    {
        public BaseDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId ?? nameof(BaseDialog))
        {
        }
        public async Task<DialogTurnResult> DialogByIntent(WaterfallStepContext stepContext, (string intent, double score)? topIntent)
        {
            switch (topIntent.Value.intent)
            {
                case LuisServiceConfiguration.SubjectIntent:
                    return await stepContext.BeginDialogAsync(nameof(SubjectDialog));
                case LuisServiceConfiguration.TeacherIntent:
                    return await stepContext.BeginDialogAsync(nameof(TeacherDialog));
                case LuisServiceConfiguration.UnknownIntent:
                    return await stepContext.BeginDialogAsync(nameof(QuestionDialog));
                case LuisServiceConfiguration.LanguageNotValidIntent:
                    return await stepContext.BeginDialogAsync(nameof(LanguageNotValidDialog));
                case LuisServiceConfiguration.GreetinsIntent:
                    return await stepContext.BeginDialogAsync(nameof(GratitudeDialog));
                case LuisServiceConfiguration.HelpIntent:
                    return await stepContext.BeginDialogAsync(nameof(HelpDialog));
                case LuisServiceConfiguration.GoodByeIntent:
                    return await stepContext.BeginDialogAsync(nameof(GoodByeDialog));
                case LuisServiceConfiguration.NotIntent:
                    return await stepContext.BeginDialogAsync(nameof(NegationDialog));
                default:
                    return await stepContext.BeginDialogAsync(nameof(MainLuisDialog));
            }
        }
    }
}
