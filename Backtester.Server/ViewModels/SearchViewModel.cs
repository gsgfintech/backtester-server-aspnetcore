using DataTypes.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.ViewModels
{
    public class SearchViewModel
    {
        [Display(Name = "Search by ID")]
        public string GroupId { get; set; }

        public List<SelectListItem> StratsNames { get; private set; }

        [Display(Name = "Search by Strategy")]
        public string Strategy { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start")]
        public DateTime? RangeStart { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End")]
        public DateTime? RangeEnd { get; set; }

        public string SearchId { get; set; }

        public SearchViewModel()
        {

        }

        public SearchViewModel(IEnumerable<string> stratsNames)
        {
            SetStratsNames(stratsNames);
        }

        internal void SetStratsNames(IEnumerable<string> stratsNames)
        {
            StratsNames = stratsNames?.Select(s => new SelectListItem()
            {
                Text = s,
                Value = s
            }).ToList();
        }
    }
}
