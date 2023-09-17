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
            await ValidateSearchTermsAndPlaceWarning(searchModel, improvedTopicListModel);
            if (!string.IsNullOrWhiteSpace(improvedTopicListModel.WarningMessage))
            {
                return improvedTopicListModel;
            }
            var topics = await GetMatchingTopics(searchModel);

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

        private async Task<IList<Topic>> GetMatchingTopics(SearchModel searchModel)
        {
            return await _topicRepository.GetAllAsync(query =>
            {
                return from t in query
                       where t.Body.Contains(searchModel.q) || t.Title.Contains(searchModel.q) || t.MetaTitle.Contains(searchModel.q)
                                   || t.MetaDescription.Contains(searchModel.q) || t.MetaKeywords.Contains(searchModel.q)
                       select t;
            });
        }

        private async Task<ImprovedBlogPostListModel> SearchBlogs(SearchModel searchModel)
        {
            var improvedBlogPostListModel = new ImprovedBlogPostListModel();
            await ValidateSearchTermsAndPlaceWarning(searchModel, improvedBlogPostListModel);
            if (!string.IsNullOrWhiteSpace(improvedBlogPostListModel.WarningMessage))
            {
                return improvedBlogPostListModel;
            }
            var blogPosts = await GetMatchingBlogPosts(searchModel);

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

        private async Task<IList<BlogPost>> GetMatchingBlogPosts(SearchModel searchModel)
        {
            return await _blogPostRepository.GetAllAsync(query =>
            {
                return from b in query
                       where b.Body.Contains(searchModel.q) || b.Title.Contains(searchModel.q) || b.Tags.Contains(searchModel.q)
                                   || b.BodyOverview.Contains(searchModel.q) || b.MetaTitle.Contains(searchModel.q)
                                   || b.MetaDescription.Contains(searchModel.q) || b.MetaKeywords.Contains(searchModel.q)
                       orderby b.CreatedOnUtc descending
                       select b;
            });
        }

        private async Task ValidateSearchTermsAndPlaceWarning<T>(SearchModel searchModel, T modelToReturn) where T : ImprovedModelBase
        {
            if (string.IsNullOrWhiteSpace(searchModel?.q) || searchModel.q.Trim().Length < _catalogSettings.ProductSearchTermMinimumLength)
            {
                modelToReturn.WarningMessage = string.Format(await _localizationService.GetResourceAsync("Search.SearchTermMinimumLengthIsNCharacters"),
                            _catalogSettings.ProductSearchTermMinimumLength);
            }
        }
    }
}
