using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Conflux.Components.WebApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Conflux.Components.WebApi
{
    public class ConfluxController : ControllerBase
    {
        protected readonly IConfluxWebServices webServices;

        public ConfluxController(IConfluxWebServices registeredServices)
        {
            webServices = registeredServices;
        }
    }
}
