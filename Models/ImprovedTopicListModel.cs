using System.Collections.Generic;
using Nop.Web.Models.Topics;

namespace Nop.Plugin.Widgets.ImprovedSearch.Models
{
    public class ImprovedTopicListModel : ImprovedModelBase
    {
        public IList<TopicModel> Topics { get; set; } = new List<TopicModel>();
    }
}