using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBOT.Dialog
{
    public class ComplexDialogBot : IBot
    {
        // Define constants for the bot...

        // Define properties for the bot's accessors and dialog set.
        private readonly ComplexDialogBotAccessors _accessors;
        private readonly DialogSet _dialogs;

        // Initialize the bot and add dialogs and prompts to the dialog set.
        public ComplexDialogBot(ComplexDialogBotAccessors accessors)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            // Create a dialog set for the bot. It requires a DialogState accessor, with which
            // to retrieve the dialog state from the turn context.
            _dialogs = new DialogSet(accessors.DialogStateAccessor);

            // Add the prompts we need to the dialog set.
            _dialogs
                .Add(new TextPrompt(NamePrompt))
                .Add(new NumberPrompt<int>(AgePrompt))
                .Add(new ChoicePrompt(SelectionPrompt));

            // Add the dialogs we need to the dialog set.
            _dialogs.Add(new WaterfallDialog(TopLevelDialog)
                .AddStep(NameStepAsync)
                .AddStep(AgeStepAsync)
                .AddStep(StartSelectionStepAsync)
                .AddStep(AcknowledgementStepAsync));

            _dialogs.Add(new WaterfallDialog(ReviewSelectionDialog)
                .AddStep(SelectionStepAsync)
                .AddStep(LoopStepAsync));
        }


        // The first step of the top-level dialog.
        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create an object in which to collect the user's information within the dialog.

            stepContext.Values["QuestionState"] = new QuestionState();

            // Ask the user to enter question.
            return await stepContext.PromptAsync(
                NamePrompt,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") },
                cancellationToken);
        }

        // The second step of the top-level dialog.
        private async Task<DialogTurnResult> AgeStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Set the user's name to what they entered in response to the name prompt.
            ((UserProfile)stepContext.Values[UserInfo]).Name = (string)stepContext.Result;

            // Ask the user to enter their age.
            return await stepContext.PromptAsync(
                AgePrompt,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your age.") },
                cancellationToken);
        }

        // The third step of the top-level dialog.
        private async Task<DialogTurnResult> StartSelectionStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Set the user's age to what they entered in response to the age prompt.
            int age = (int)stepContext.Result;
            ((UserProfile)stepContext.Values[UserInfo]).Age = age;

            if (age < 25)
            {
                // If they are too young, skip the review-selection dialog, and pass an empty list to the next step.
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("You must be 25 or older to participate."),
                    cancellationToken);
                return await stepContext.NextAsync(new List<string>(), cancellationToken);
            }
            else
            {
                // Otherwise, start the review-selection dialog.
                return await stepContext.BeginDialogAsync(ReviewSelectionDialog, null, cancellationToken);
            }
        }

        // The final step of the top-level dialog.
        private async Task<DialogTurnResult> AcknowledgementStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Set the user's company selection to what they entered in the review-selection dialog.
            List<string> list = stepContext.Result as List<string>;
            ((UserProfile)stepContext.Values[UserInfo]).CompaniesToReview = list ?? new List<string>();

            // Thank them for participating.
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Thanks for participating, {((UserProfile)stepContext.Values[UserInfo]).Name}."),
                cancellationToken);

            // Exit the dialog, returning the collected user information.
            return await stepContext.EndDialogAsync(stepContext.Values[UserInfo], cancellationToken);
        }

        // Turn handler and other supporting methods...
    }
}
