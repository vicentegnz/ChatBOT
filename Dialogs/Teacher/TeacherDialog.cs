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

                return await stepContext.PromptAsync("textPrompt",
                    new PromptOptions
                    {
                        Prompt = stepContext.Context.Activity.CreateReply("¿Podrías indicarme el nombre del profesor?")
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

                        if (teachersSearched.Any())
                        {
                            if (teachersSearched.Count == 1)
                            {
                                await stepContext.Context.SendActivityAsync($"En esta dirección encontrarás toda su información {teachersSearched.FirstOrDefault().InfoUrl}.");
                                return await stepContext.ReplaceDialogAsync(MainLuisDialog.Id, null, cancellationToken);
                            }

                            var choices = new List<Choice>();
                            foreach (var teacher in teachersSearched)
                                choices.Add(new Choice { Value = teacher.Name, Synonyms = teacher.Name.Split(" ").TakeLast(2).ToList() });
                            return await stepContext.PromptAsync("choicePrompt",
                                new PromptOptions
                                {
                                    Prompt = stepContext.Context.Activity.CreateReply($"He encontrado varios profesores con esos datos {Environment.NewLine} ¿De cual de estos profesores necesitas más información?"),
                                    Choices = choices,
                                    RetryPrompt = stepContext.Context.Activity.CreateReply("Por favor, comprueba que has escrito uno de los profesores listados.")
                                });
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync($"No tengo en la base de datos ningún profesor con estos datos ({result}).");
                            return await stepContext.ReplaceDialogAsync(Id);
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
                
                    List<TeacherModel> teachersSearched = GetTeachersFilteredByName(response, teacherService.GetListOfTeachers().Result);
                    if (teachersSearched.Any())
                    { 
                        await stepContext.Context.SendActivityAsync($"En esta dirección encontrarás toda su información {teachersSearched.FirstOrDefault().InfoUrl}.");
                        return await stepContext.ReplaceDialogAsync(MainLuisDialog.Id, null, cancellationToken);
                    }
                    else
                    { 
                        await stepContext.Context.SendActivityAsync($"No tengo ningun profesor con esos datos.");
                        return await stepContext.ReplaceDialogAsync(TeacherDialog.Id, null, cancellationToken);
                    }
                });

        }

        private List<TeacherModel> GetTeachersFilteredByName(string response, List<TeacherModel> teacherList)
        {
            return teacherList.
                Where(x => x.Name.Replace(" ", "").Contains(response.Replace(" ", "").RemoveDiacritics(), StringComparison.InvariantCultureIgnoreCase))?.ToList() ?? new List<TeacherModel>();
        }

        public new static string Id => "TeacherDialog";
    }
}
