using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SimpleFactorRoot.SDK
{
    public class BotFrameworkControllerBase : ControllerBase
    {
        private BotFrameworkHandler _handler;

        public BotFrameworkControllerBase(BotFrameworkHandler handler)
        {
            _handler = handler;
        }

        // ASP method or methods that call into the _handler


    }
}
