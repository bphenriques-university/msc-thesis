using MathNet.Numerics;
using RapportActionProposer.ActionProposalDefinition;
using System.Collections.Generic;

namespace RapportAgentPlugin {
    
    public interface IActionsProposalGenerator : IAgentActions {
        IActionProposal PerformUtterance(string category, string subcategory, ushort priority, HashSet<Tuple<string, string>> variables, int initialDelay = 0, int timeout = 0);
        IActionProposal PerformUtteranceSkene(string category, string subcategory, ushort priority, int initialDelay = 0, int timeout = 0);
    }

    public interface IAgentActions {
        IActionProposal Gaze(string targetName, ushort priority, int initialDelay = 0, int timeout = 0);        
        IActionProposal Animate(string animation, ushort priority, int initialDelay = 0, int timeout = 0);
        IActionProposal Speak(string text, ushort priority, int initialDelay = 0, int timeout = 0);
        IActionProposal Sound(string id, ushort priority, int initialDelay = 0, int timeout = 0);
        IActionProposal HeadShake(int repetitions, double intensity, double frequency, ushort priority, int initialDelay = 0, int timeout = 0);
        IActionProposal HeadNod(int repetitions, double intensity, double frequency, ushort priority, int initialDelay = 0, int timeout = 0);
    }
}