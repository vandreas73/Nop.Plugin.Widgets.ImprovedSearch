using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using Nop.Core;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Topics;
using Nop.Data;
using Nop.Plugin.Widgets.ImprovedSearch.Factories;
using Nop.Plugin.Widgets.ImprovedSearch.Models;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Models.Blogs;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Topics;
using Nop.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Framework.UI.Paging;

namespace Nop.Plugin.Widgets.ImprovedSearch.Services
{
    public interface IImprovedSearchService
    {
        public Task<ImprovedSearchModel> Search(SearchModel searchModel, int pagenumber);
        public Task<IPagedList<BlogPost>> GetMatchingBlogPosts(SearchModel searchModel, int languageId, int pageIndex = 0,
            int pageSize = int.MaxValue);
    }

    public class ImprovedSearchService : IImprovedSearchService
    {
        private readonly IRepository<BlogPost> _blogPostRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<Topic> _topicRepository;
        private readonly ITopicModelFactory _topicModelFactory;
        private readonly IImprovedSearchModelFactory _improvedSearchModelFactory;
        private readonly IWorkContext _workContext;

        public ImprovedSearchService(
            IRepository<BlogPost> blogPostRepository,
            IBlogModelFactory blogModelFactory,
            CatalogSettings catalogSettings,
            ILocalizationService localizationService,
            IRepository<Topic> topicRepository,
            ITopicModelFactory topicModelFactory,
            IImprovedSearchModelFactory improvedSearchModelFactory,
            IWorkContext workContext)
        {
            _blogPostRepository = blogPostRepository;
            _catalogSettings = catalogSettings;
            _localizationService = localizationService;
            _topicRepository = topicRepository;
            _topicModelFactory = topicModelFactory;
            _improvedSearchModelFactory = improvedSearchModelFactory;
            _workContext = workContext;
        }

        public async Task<ImprovedSearchModel> Search(SearchModel searchModel, int pagenumber)
        {
            var improvedSearchModel = new ImprovedSearchModel();
            improvedSearchModel.BlogPostListModel = await SearchBlogs(searchModel, pagenumber);
            improvedSearchModel.TopicListModel = await SearchTopics(searchModel);

            return improvedSearchModel;
        }

        private async Task<ImprovedTopicListModel> SearchTopics(SearchModel searchModel)
        {
            var improvedTopicListModel = new ImprovedTopicListModel();
            await ValidateSearchTermsAndPlaceWarning(searchModel, improvedTopicListModel);
            if (!string.IsNullOrWhiteSpace(improvedTopicListModel.WarningMessage))
            {
                return improvedTopicListModel;
            }
            var language = await _workContext.GetWorkingLanguageAsync();
            var topics = await GetMatchingTopics(searchModel);

            if (topics.Count == 0)
            {
                improvedTopicListModel.NoResultMessage = string.Format(await _localizationService.GetResourceAsync(
                    "plugins.widgets.improvedsearch.topic.noresult"), searchModel.q);
                return improvedTopicListModel;
            }

            improvedTopicListModel.Topics = new List<TopicModel>();
            foreach (var topic in topics)
            {
                var topicModel = await _topicModelFactory.PrepareTopicModelAsync(topic);
                improvedTopicListModel.Topics.Add(topicModel);
            }
            return improvedTopicListModel;
        }

        private async Task<IList<Topic>> GetMatchingTopics(SearchModel searchModel, int languageId = 0)
        {
            return await _topicRepository.GetAllAsync(query =>
            {
                query = query.Where(t => t.Body.Contains(searchModel.q) || t.Title.Contains(searchModel.q) || t.MetaTitle.Contains(searchModel.q)
                                   || t.MetaDescription.Contains(searchModel.q) || t.MetaKeywords.Contains(searchModel.q));
                return query;
            });
        }

        private async Task<ImprovedBlogPostListModel> SearchBlogs(SearchModel searchModel, int pagenumber)
        {
            var improvedBlogPostListModel = new ImprovedBlogPostListModel();
            await ValidateSearchTermsAndPlaceWarning(searchModel, improvedBlogPostListModel);
            if (!string.IsNullOrWhiteSpace(improvedBlogPostListModel.WarningMessage))
            {
                return improvedBlogPostListModel;
            }
            var language = await _workContext.GetWorkingLanguageAsync();
            var blogPosts = await GetMatchingBlogPosts(searchModel, language.Id, pagenumber, 5);

            if (blogPosts.Count == 0)
            {
                improvedBlogPostListModel.NoResultMessage = string.Format(await _localizationService.GetResourceAsync(
                    "plugins.widgets.improvedsearch.blog.noresult"), searchModel.q);
                return improvedBlogPostListModel;
            }

            improvedBlogPostListModel = await _improvedSearchModelFactory.PrepareBlogPostListModelAsync(blogPosts);

            //improvedBlogPostListModel.BlogPosts = new List<BlogPostModel>();
            //foreach (var blogPost in blogPosts)
            //{
            //    var blogPostModel = new BlogPostModel();
            //    await _blogModelFactory.PrepareBlogPostModelAsync(blogPostModel, blogPost, false);
            //    improvedBlogPostListModel.BlogPosts.Add(blogPostModel);
            //}
            return improvedBlogPostListModel;
        }

        public async Task<IPagedList<BlogPost>> GetMatchingBlogPosts(SearchModel searchModel, int languageId = 0, int pageIndex = 0, 
            int pageSize = int.MaxValue)
        {
            return await _blogPostRepository.GetAllPagedAsync(query =>
            {
                query = query.Where(b => b.Body.Contains(searchModel.q) || b.Title.Contains(searchModel.q) || b.Tags.Contains(searchModel.q)
                                    || b.BodyOverview.Contains(searchModel.q) || b.MetaTitle.Contains(searchModel.q)
                                    || b.MetaDescription.Contains(searchModel.q) || b.MetaKeywords.Contains(searchModel.q));
                if (languageId > 0)
                    query = query.Where(b => languageId == b.LanguageId);
                query = query.OrderByDescending(b => b.StartDateUtc ?? b.CreatedOnUtc);
                return query;
            }, pageIndex, pageSize);
        }

        private async Task ValidateSearchTermsAndPlaceWarning<T>(SearchModel searchModel, T modelToReturn) where T : IImprovedModel
        {
            if (string.IsNullOrWhiteSpace(searchModel?.q) || searchModel.q.Trim().Length < _catalogSettings.ProductSearchTermMinimumLength)
            {
                modelToReturn.WarningMessage = string.Format(await _localizationService.GetResourceAsync("Search.SearchTermMinimumLengthIsNCharacters"),
                            _catalogSettings.ProductSearchTermMinimumLength);
            }
        }

    }
}
