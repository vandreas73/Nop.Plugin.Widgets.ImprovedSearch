using System.Collections.Generic;
using Nop.Web.Models.Topics;

namespace Nop.Plugin.Widgets.ImprovedSearch.Models
{
    public class ImprovedTopicListModel : IImprovedModel
    {
        public IList<TopicModel> Topics { get; set; } = new List<TopicModel>();
        public string WarningMessage { get; set; }
        public string NoResultMessage { get; set; }
    }
}