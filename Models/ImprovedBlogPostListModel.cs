using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Models.Blogs;

namespace Nop.Plugin.Widgets.ImprovedSearch.Models
{
    public record ImprovedBlogPostListModel : BlogPostListModel, IImprovedModel
    {
        public string WarningMessage { get; set; }
        public string NoResultMessage { get; set; }
    }
}
