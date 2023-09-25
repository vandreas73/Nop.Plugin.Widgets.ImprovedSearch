using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.ImprovedSearch.Services;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.ImprovedSearch.Controllers
{
    public class ImprovedSearchController : BasePluginController
    {
        private readonly ImprovedSearchService _improvedSearchService;

        public IActionResult LoadComponent(SearchModel additionalData)
        {
            return ViewComponent("ImprovedSearch", new { additionalData = additionalData });
        }
    }
}
