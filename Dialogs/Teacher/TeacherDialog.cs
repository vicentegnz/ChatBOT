using ChatBOT.Bot;
using ChatBOT.Core;
using ChatBOT.Core.Extensions;
using ChatBOT.Domain;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public sealed class TeacherDialog : WaterfallDialog
    {

        public TeacherDialog(string dialogId, ITeacherService teacherService, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {


            AddStep(async (stepContext, cancellationToken) =>
            {
                var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
                var message = string.Empty;

                if (state.Messages.Any())
                {
                    message = $"No he encontrado ningún profesor con ese nombre, asegurate de que lo estás escribiendo correctamente.";
                }
                else
                {
                    message = $"Has seleccionado horario de tutoria de un profesor, por favor indicame el nombre del profesor del que te gustaría conocer el horario de tutorías.";
                }


                return await stepContext.PromptAsync("textPrompt",
                    new PromptOptions
                    {
                        Prompt = stepContext.Context.Activity.CreateReply(message)
                    });
            });

            AddStep(async (stepContext, cancellationToken) =>
            {
                var result = stepContext.Result.ToString();
                var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
                state.Messages.Add(stepContext.Result.ToString());

                var teacherList = teacherService.GetListOfTeachers();
                if (teacherList != null)
                {
                    List<TeacherModel> teachersSearched = GetTeachersFilteredByName(result, teacherList.Result);

                    if (teachersSearched.Count == 1)
                    {
                        await stepContext.Context.SendActivityAsync(teachersSearched.FirstOrDefault().InfoUrl);
                        return await stepContext.EndDialogAsync();
                    }
                    else
                    {
                        List<Choice> choices = new List<Choice>();
                        foreach (var teacher in teachersSearched) {
                            var nameSeparated = teacher.Name.Split(" ");
                            choices.Add(new Choice { Value = teacher.Name , Synonyms = nameSeparated.TakeLast(nameSeparated.Length).ToList() });
                        }
                        return await stepContext.PromptAsync("choicePrompt",
                            new PromptOptions
                            {
                                Prompt = stepContext.Context.Activity.CreateReply("¿Cual de estos profesores es para el que necesitas más información?"),
                                Choices = choices,
                                RetryPrompt = stepContext.Context.Activity.CreateReply("Por favor, comprueba que me has escrito uno de estos profesores.")
                            });
                    }
                }
                else
                { 
                    await stepContext.Context.SendActivityAsync("En estos momentos la lista de profesores no está disponible, intentelo mas tarde.");
                    return await stepContext.EndDialogAsync();
                }
            });

            AddStep(async (stepContext, cancellationToken) =>
            {
                var response = (stepContext.Result as FoundChoice)?.Value;
                var teacherList = teacherService.GetListOfTeachers();

                List<TeacherModel> teachersSearched = GetTeachersFilteredByName(response, teacherList.Result);

                await stepContext.Context.SendActivityAsync($"En esta dirección encontrarás toda su información {teachersSearched.FirstOrDefault().InfoUrl}.");
                return await stepContext.EndDialogAsync();

            });

        }

        private List<TeacherModel> GetTeachersFilteredByName(string response, List<TeacherModel> teacherList)
        {
            return teacherList.
                Where(x => x.Name.Replace(" ", "").Contains(response.Replace(" ", "").RemoveDiacritics(), StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public static string Id => "TeacherDialog";
    }
}
