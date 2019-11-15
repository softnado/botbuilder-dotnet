using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace SimpleFactorRoot.SDK
{
    public class SkillHandler : BotFrameworkHandler
    {
        private BotAdapter _adapter;

        public SkillHandler(BotAdapter adapter)
        {
            _adapter = adapter;
        }

    }
}
