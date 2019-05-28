using ChatBOT.Bot;
using ChatBOT.Core;
using ChatBOT.Core.Extensions;
using ChatBOT.Domain;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.LanguageGeneration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public sealed class TeacherDialog : ComponentDialog
    {
        #region Properties

        private readonly TemplateEngine _lgEngine;
        private readonly ITeacherService _teacherService;
 
        #endregion
        public TeacherDialog(string dialogId, ITeacherService teacherService, IEnumerable<WaterfallStep> steps = null) : base(dialogId)
        {
            _teacherService = teacherService;

            string fullPath = Path.Combine(new string[] { ".", "Dialogs", "Teacher", "TeacherDialog.lg" });
            _lgEngine = TemplateEngine.FromFiles(fullPath);

            ChoicePrompt choicePrompt = new ChoicePrompt(nameof(ChoicePrompt));
            choicePrompt.ChoiceOptions = new ChoiceFactoryOptions { IncludeNumbers = false };
            choicePrompt.RecognizerOptions = new FindChoicesOptions { AllowPartialMatches = true };
            AddDialog(choicePrompt);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                                        IntroTeacherQuestionStepAsync,
                                        ResponseTeacherQuestionStepAsync,
                                        EndTeacherQuestionStepAsync

            }));

            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> IntroTeacherQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
            //var message = string.Empty;

            //if (state.Messages.Any())
            //{
            //    message = $"No he encontrado ningún profesor con ese nombre, asegurate de que lo estás escribiendo correctamente.";
            //}
            //else
            //{
            //    message = $"Has seleccionado horario de tutoria de un profesor, por favor indicame el nombre del profesor del que te gustaría conocer el horario de tutorías.";
            //}

            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskTeacher",null))
                });
        }

        private async Task<DialogTurnResult> ResponseTeacherQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = stepContext.Result.ToString();
            var state = await (stepContext.Context.TurnState[nameof(NexoBotAccessors)] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
            state.Messages.Add(stepContext.Result.ToString());

            var teacherList = _teacherService.GetListOfTeachers();
            if (teacherList != null)
            {
                List<TeacherModel> teachersSearched = GetTeachersFilteredByName(result, teacherList.Result);

                if (teachersSearched.Any())
                {
                    if (teachersSearched.Count == 1)
                    {
                        await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("TeacherInfo", null) + teachersSearched.FirstOrDefault().InfoUrl);
                        return await stepContext.ReplaceDialogAsync(nameof(MainLuisDialog), null, cancellationToken);
                    }

                    var choices = new List<Choice>();
                    foreach (var teacher in teachersSearched)
                        choices.Add(new Choice { Value = teacher.Name, Synonyms = teacher.Name.Split(" ").TakeLast(2).ToList() });
                    return await stepContext.PromptAsync(nameof(ChoicePrompt),
                        new PromptOptions
                        {
                            Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("FoundSomeTeachers", null)),
                            Choices = choices,
                            RetryPrompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("NoTeacherFoundAskAgain", null))
                        });
                }
                else
                {
                    //TODO AÑADIR RESULT EN EL OBJETO
                    await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("NoTeacherFound", null));
                    return await stepContext.ReplaceDialogAsync(Id);
                }

            }
            else
            {
                await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("TeacherListNotAvailable", null));
                return await stepContext.EndDialogAsync();
            }
        }

        private async Task<DialogTurnResult> EndTeacherQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var response = (stepContext.Result as FoundChoice)?.Value;

            List<TeacherModel> teachersSearched = GetTeachersFilteredByName(response, _teacherService.GetListOfTeachers().Result);
            if (teachersSearched.Any())
            {
                await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("TeacherInfo",null) + teachersSearched.FirstOrDefault().InfoUrl);
                return await stepContext.ReplaceDialogAsync(nameof(MainLuisDialog), null, cancellationToken);
            }
            else
            {
                //TODO AÑADIR RESULT EN EL OBJETO
                await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("NoTeacherFound", null));
                return await stepContext.ReplaceDialogAsync(nameof(TeacherDialog), null, cancellationToken);
            }
        }
        private List<TeacherModel> GetTeachersFilteredByName(string response, List<TeacherModel> teacherList)
        {
            return teacherList.
                Where(x => x.Name.Replace(" ", "").Contains(response.Replace(" ", "").RemoveDiacritics(), StringComparison.InvariantCultureIgnoreCase))?.ToList() ?? new List<TeacherModel>();
        }
    }
}
