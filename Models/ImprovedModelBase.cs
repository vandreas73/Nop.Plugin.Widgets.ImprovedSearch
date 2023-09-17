using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Models.Blogs;

namespace Nop.Plugin.Widgets.ImprovedSearch.Models
{
    public class ImprovedModelBase
    {
        /// <summary>
        /// Gets or sets the warning message
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets or sets the message if there are no products to return
        /// </summary>
        public string NoResultMessage { get; set; }
    }
}
