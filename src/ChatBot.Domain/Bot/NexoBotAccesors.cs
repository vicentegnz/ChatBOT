using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ChatBot.Domain.Bot
{
    public class NexoBotAccessors
    {
        public NexoBotAccessors(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }

        public static string NexoBotStateAccesorName { get; } = $"{nameof(NexoBotAccessors)}.NexoBotState";
        public IStatePropertyAccessor<NexoBotState> NexoBotStateStateAccessor { get; internal set; }

        public static string DialogStateAccessorName { get; } = $"{nameof(NexoBotAccessors)}.DialogState";
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; internal set; }


        public ConversationState ConversationState { get; }
    }
}
    