using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Models.Blogs;

namespace Nop.Plugin.Widgets.ImprovedSearch.Models
{
    public class ImprovedBlogPostListModel : ImprovedModelBase
    {
        public IList<BlogPostModel> BlogPosts { get; set; } = new List<BlogPostModel>();
    }
}
