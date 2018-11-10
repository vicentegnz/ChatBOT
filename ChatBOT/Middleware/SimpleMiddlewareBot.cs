using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ChatBOT.Bot
{
    public class SimpleMiddlewareBot : IMiddleware
    {

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            await turnContext.SendActivityAsync($"[SimpleMiddlewareBot] {turnContext.Activity.Type}/OnTurnAsync/Before");

            if (turnContext.Activity.Type == ActivityTypes.Message && turnContext.Activity.Text == "secret password")
            {
                // calling next() is totally optional. if the middleware does not call next then the
                // next middleware in the pipeline will not be called, AND the bot will not receive the message.
                //
                // in this instance, we are only handing the message to downstream bots if the user says "secret password"
                await next(cancellationToken);
            }

            await turnContext.SendActivityAsync($"[SimpleMiddlewareBot] {turnContext.Activity.Type}/OnTurnAsync/After");
        }

    }
}
