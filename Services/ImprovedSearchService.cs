using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Topics;
using Nop.Data;
using Nop.Plugin.Widgets.ImprovedSearch.Models;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Models.Blogs;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Topics;
using StackExchange.Profiling.Internal;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace Nop.Plugin.Widgets.ImprovedSearch.Services
{
    public interface IImprovedSearchService
    {
        public Task<ImprovedSearchModel> Search(SearchModel searchModel);
    }

    public class ImprovedSearchService : IImprovedSearchService
    {
        private readonly IRepository<BlogPost> _blogPostRepository;
        private readonly IBlogModelFactory _blogModelFactory;
        private readonly CatalogSettings _catalogSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<Topic> _topicRepository;
        private readonly ITopicModelFactory _topicModelFactory;

        public ImprovedSearchService(
            IRepository<BlogPost> blogPostRepository,
            IBlogModelFactory blogModelFactory,
            CatalogSettings catalogSettings,
            ILocalizationService localizationService,
            IRepository<Topic> topicRepository,
            ITopicModelFactory topicModelFactory)
        {
            _blogPostRepository = blogPostRepository;
            _blogModelFactory = blogModelFactory;
            _catalogSettings = catalogSettings;
            _localizationService = localizationService;
            _topicRepository = topicRepository;
            _topicModelFactory = topicModelFactory;
        }

        public async Task<ImprovedSearchModel> Search(SearchModel searchModel)
        {
            var improvedSearchModel = new ImprovedSearchModel();
            improvedSearchModel.BlogPostListModel = await SearchBlogs(searchModel);
            improvedSearchModel.TopicListModel = await SearchTopics(searchModel);

            return improvedSearchModel;
        }

        private async Task<ImprovedTopicListModel> SearchTopics(SearchModel searchModel)
        {
            var improvedTopicListModel = new ImprovedTopicListModel();
            var searchTerms = await ValidateSearchTermsAndPlaceWarning(searchModel, improvedTopicListModel);
            if (!string.IsNullOrWhiteSpace(improvedTopicListModel.WarningMessage))
            {
                return improvedTopicListModel;
            }
            var topics = await GetMatchingTopics(searchTerms);

            if (topics.Count == 0)
            {
                improvedTopicListModel.NoResultMessage = await _localizationService.GetResourceAsync("plugins.widgets.improvedsearch.topic.noresult");
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

        private async Task<IList<Topic>> GetMatchingTopics(string[] searchTerms)
        {
            var matchingTopicsQuery = _topicRepository.Table;
            foreach (var keyword in searchTerms)
            {
                matchingTopicsQuery = matchingTopicsQuery.Where(t => t.Body.Contains(keyword) || t.Title.Contains(keyword) || t.MetaTitle.Contains(keyword)
                    || t.MetaDescription.Contains(keyword) || t.MetaKeywords.Contains(keyword));
            }
            matchingTopicsQuery = matchingTopicsQuery.OrderByDescending(t => searchTerms.Count(keyword => t.Body.Contains(keyword) || t.Title.Contains(keyword) || t.MetaTitle.Contains(keyword)
                    || t.MetaDescription.Contains(keyword) || t.MetaKeywords.Contains(keyword)));
            return await matchingTopicsQuery.ToListAsync();
        }

        private async Task<ImprovedBlogPostListModel> SearchBlogs(SearchModel searchModel)
        {
            var improvedBlogPostListModel = new ImprovedBlogPostListModel();
            var searchTerms = await ValidateSearchTermsAndPlaceWarning(searchModel, improvedBlogPostListModel);
            if (!string.IsNullOrWhiteSpace(improvedBlogPostListModel.WarningMessage))
            {
                return improvedBlogPostListModel;
            }
            var blogPosts = await GetMatchingBlogPosts(searchTerms);

            if (blogPosts.Count == 0)
            {
                improvedBlogPostListModel.NoResultMessage = await _localizationService.GetResourceAsync("plugins.widgets.improvedsearch.blog.noresult");
                return improvedBlogPostListModel;
            }

            improvedBlogPostListModel.BlogPosts = new List<BlogPostModel>();
            foreach (var blogPost in blogPosts)
            {
                var blogPostModel = new BlogPostModel();
                await _blogModelFactory.PrepareBlogPostModelAsync(blogPostModel, blogPost, false);
                improvedBlogPostListModel.BlogPosts.Add(blogPostModel);
            }
            return improvedBlogPostListModel;
        }

        private async Task<IList<BlogPost>> GetMatchingBlogPosts(string[] searchTerms)
        {
            var matchingBlogPostsQuery = _blogPostRepository.Table;
            foreach (var keyword in searchTerms)
            {
                matchingBlogPostsQuery = matchingBlogPostsQuery.Where(b => b.Body.Contains(keyword) || b.Title.Contains(keyword) || b.Tags.Contains(keyword)
                                   || b.BodyOverview.Contains(keyword) || b.MetaTitle.Contains(keyword)
                                   || b.MetaDescription.Contains(keyword) || b.MetaKeywords.Contains(keyword));
            }
            matchingBlogPostsQuery = matchingBlogPostsQuery.OrderByDescending(b => b.CreatedOnUtc);
            matchingBlogPostsQuery = matchingBlogPostsQuery.OrderByDescending(b => searchTerms.Count(keyword => b.Body.Contains(keyword) || b.Title.Contains(keyword) || b.Tags.Contains(keyword)
                                   || b.BodyOverview.Contains(keyword) || b.MetaTitle.Contains(keyword)
                                   || b.MetaDescription.Contains(keyword) || b.MetaKeywords.Contains(keyword)));
            return await matchingBlogPostsQuery.OrderByDescending(b => b.CreatedOnUtc).ToListAsync();
        }

        private async Task<string[]> ValidateSearchTermsAndPlaceWarning<T>(SearchModel searchModel, T modelToReturn) where T : ImprovedModelBase
        {
            if (string.IsNullOrWhiteSpace(searchModel?.q) || searchModel.q.Trim().Length < _catalogSettings.ProductSearchTermMinimumLength)
            {
                modelToReturn.WarningMessage = string.Format(await _localizationService.GetResourceAsync("Search.SearchTermMinimumLengthIsNCharacters"),
                            _catalogSettings.ProductSearchTermMinimumLength);
            }

            return searchModel.q.Split();
        }
    }
}
