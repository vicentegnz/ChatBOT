
namespace ChatBot.Domain.Core
{
    public interface ISpellCheckService
    {
        string GetSpellCheckFromMessage(string message);
    }
}
