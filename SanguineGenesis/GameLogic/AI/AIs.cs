using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.AI
{
    /// <summary>
    /// All existing AIFactories.
    /// </summary>
    static class AIs
    {
        /// <summary>
        /// All existing AIFactories.
        /// </summary>
        private static Dictionary<string, IAIFactory> AIFactories { get; }

        static AIs()
        {
            AIFactories = new Dictionary<string, IAIFactory>
            {
                { "Default AI", new DefaultAIFactory() },
                { "Tutorial AI", new TutorialAIFactory() }
            };
        }

        /// <summary>
        /// Returns AIFactory with the given name. If the name doesn't exist, returns DefaultAIFactory.
        /// </summary>
        public static IAIFactory GetAIFactory(string aiFactoryName)
        {
            if(AIFactories.TryGetValue(aiFactoryName, out var aiFact)){
                return aiFact;
            }
            else
            {
                return AIFactories["Default AI"];
            }
        }

        /// <summary>
        /// Returns names of every AIFactory in AIFactories.
        /// </summary>
        public static IEnumerable<string> GetAINames()
        {
            return AIFactories.Select(kvp => kvp.Key);
        }
    }
}
