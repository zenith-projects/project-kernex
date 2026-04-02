using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectKernex.Core.Enums;

namespace ProjectKernex.Core.Interfaces;

public interface IAxiaDialogueProvider
{
    Task<string> GetResponseAsync(string userMessage, AxiaLevel level, Dictionary<string, string> gameContext);
}
