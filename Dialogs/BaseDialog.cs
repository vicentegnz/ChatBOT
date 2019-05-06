using ChatBOT.Conf;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public class BaseDialog : WaterfallDialog
    {
        public BaseDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {
        }
        public static async Task<DialogTurnResult> DialogByIntent(WaterfallStepContext stepContext, (string intent, double score)? topIntent)
        {
            switch (topIntent.Value.intent)
            {
                case LuisServiceConfiguration.SubjectIntent:
                    return await stepContext.BeginDialogAsync(SubjectDialog.Id);
                case LuisServiceConfiguration.TeacherIntent:
                    return await stepContext.BeginDialogAsync(TeacherDialog.Id);
                case LuisServiceConfiguration.ScheduleIntent:
                    return await stepContext.BeginDialogAsync(ScheduleDialog.Id);
                case LuisServiceConfiguration.UnknownIntent:
                    return await stepContext.BeginDialogAsync(QuestionDialog.Id);
                case LuisServiceConfiguration.LanguageNotValidIntent:
                    return await stepContext.BeginDialogAsync(LanguageNotValidDialog.Id);
                case LuisServiceConfiguration.GreetinsIntent:
                    return await stepContext.BeginDialogAsync(GratitudeDialog.Id);
                case LuisServiceConfiguration.HelpIntent:
                    return await stepContext.BeginDialogAsync(HelpDialog.Id);
                case LuisServiceConfiguration.GoodByeIntent:
                    return await stepContext.BeginDialogAsync(GoodByeDialog.Id);
                case LuisServiceConfiguration.NotIntent:
                    return await stepContext.BeginDialogAsync(NegationDialog.Id);
                default:
                    return await stepContext.BeginDialogAsync(MainLuisDialog.Id);
            }
        }
    }
}
