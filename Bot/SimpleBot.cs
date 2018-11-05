using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace UniversityOfExtremaduraBOT.Bot
{
    public class SimpleBot : IBot
    {

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type is ActivityTypes.Message)
            {
                string input = turnContext.Activity.Text;
                await turnContext.SendActivityAsync($"SimpleBot: {input}");
            }
        }
        
    }
}
