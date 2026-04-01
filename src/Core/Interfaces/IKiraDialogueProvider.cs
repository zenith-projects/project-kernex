using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectKernex.Core.Enums;

namespace ProjectKernex.Core.Interfaces;

public interface IKiraDialogueProvider
{
    Task<string> GetResponseAsync(string userMessage, KiraLevel level, Dictionary<string, string> gameContext);
}
