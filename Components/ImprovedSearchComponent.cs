using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.ImprovedSearch.Services;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.ImprovedSearch.Components
{
    [ViewComponent(Name = "ImprovedSearch")]
    public class ImprovedSearchComponent : NopViewComponent
    {
        private readonly IImprovedSearchService _improvedSearchService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImprovedSearchComponent(IImprovedSearchService improvedSearchService, IHttpContextAccessor httpContextAccessor)
        {
            _improvedSearchService = improvedSearchService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, SearchModel additionalData)
        {
            var pagenumber_str = _httpContextAccessor.HttpContext.Request.Query["pagenumber"].ToString();
            int pagenumber;
            int.TryParse(pagenumber_str, out pagenumber);
            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }
            return View("~/Plugins/Widgets.ImprovedSearch/Views/NonCatalogResults.cshtml", await _improvedSearchService.Search(additionalData, pagenumber));
        }
    }
}
