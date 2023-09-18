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
        private readonly BlogSettings _blogSettings;
        private readonly IWorkContext _workContext;
        private readonly IBlogModelFactory _blogModelFactory;

        public ImprovedSearchModelFactory
            (BlogSettings blogSettings,
            IWorkContext workContext,
            IBlogModelFactory blogModelFactory)
        {
            _blogSettings = blogSettings;
            _workContext = workContext;
            _blogModelFactory = blogModelFactory;
        }

        public async Task<ImprovedBlogPostListModel> PrepareBlogPostListModelAsync(IPagedList<BlogPost> blogPosts)
        {
            var command = new BlogPagingFilteringModel();

            if (command.PageSize <= 0)
                command.PageSize = _blogSettings.PostsPageSize;
            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            
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
