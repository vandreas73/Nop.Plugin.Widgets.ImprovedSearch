using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Blogs;
using Nop.Core;
using Nop.Web.Models.Blogs;
using Nop.Plugin.Widgets.ImprovedSearch.Services;
using Nop.Web.Models.Catalog;
using Nop.Web.Factories;
using Nop.Plugin.Widgets.ImprovedSearch.Models;

namespace Nop.Plugin.Widgets.ImprovedSearch.Factories
{
    public interface IImprovedSearchModelFactory
    {
        public Task<ImprovedBlogPostListModel> PrepareBlogPostListModelAsync(IPagedList<BlogPost> blogPosts);
    }

    public class ImprovedSearchModelFactory : IImprovedSearchModelFactory
    {
        private readonly IBlogModelFactory _blogModelFactory;

        public ImprovedSearchModelFactory
            (IBlogModelFactory blogModelFactory)
        {
            _blogModelFactory = blogModelFactory;
        }

        public async Task<ImprovedBlogPostListModel> PrepareBlogPostListModelAsync(IPagedList<BlogPost> blogPosts)
        {
            var model = new ImprovedBlogPostListModel
            {
                PagingFilteringContext = { },
                BlogPosts = await blogPosts.SelectAwait(async blogPost =>
                {
                    var blogPostModel = new BlogPostModel();
                    await _blogModelFactory.PrepareBlogPostModelAsync(blogPostModel, blogPost, false);
                    return blogPostModel;
                }).ToListAsync()
            };
            model.PagingFilteringContext.LoadPagedList(blogPosts);

            return model;
        }
    }
}
