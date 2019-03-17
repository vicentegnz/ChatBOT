using System;
using System.Collections.Generic;
using System.Linq;
using ChatBOT.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace ChatBOT.Dialogs
{
    public class MainDialog : WaterfallDialog
    {
        private const string SCHEDULE_CHOICE = "Horario del grado";
        private const string QUESTION_CHOICE = "Otra consulta";
        private const string SUBJECT_CHOICE = "Ficha de una asignatura";
        private const string TEACHER_CHOICE = "Horario de tutoria de un profesor";

        public MainDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {

            AddStep(async (stepContext, cancellationToken) =>
            {
                return await stepContext.PromptAsync("choicePrompt",
                    new PromptOptions
                    {
                        Prompt = stepContext.Context.Activity.CreateReply($"Hola soy nexo-bot 🤖 {Environment.NewLine} ¿De las siguientes opciones que te gustaría consultar?"),
                        Choices = new[] {
                            new Choice { Value = SUBJECT_CHOICE },
                            new Choice { Value = TEACHER_CHOICE },
                            new Choice { Value = SCHEDULE_CHOICE },
                            new Choice { Value = QUESTION_CHOICE }
                        }.ToList()
                    });
            });

            AddStep(async (stepContext, cancellationToken) =>
            {
                var response = (stepContext.Result as FoundChoice)?.Value;

                switch (response)
                {
                    case SUBJECT_CHOICE:
                        return await stepContext.BeginDialogAsync(QuestionDialog.Id);
                    case TEACHER_CHOICE:
                        return await stepContext.BeginDialogAsync(QuestionDialog.Id);
                    case SCHEDULE_CHOICE:
                        return await stepContext.BeginDialogAsync(QuestionDialog.Id);
                    case QUESTION_CHOICE:
                        return await stepContext.BeginDialogAsync(QuestionDialog.Id);
                    default:
                        return await stepContext.NextAsync();
                }
           
            });

            AddStep(async (stepContext, cancellationToken) => { return await stepContext.ReplaceDialogAsync(Id); });
        }


        public static string Id => "mainDialog";

        public static MainDialog Instance { get; } = new MainDialog(Id);
    }
}
