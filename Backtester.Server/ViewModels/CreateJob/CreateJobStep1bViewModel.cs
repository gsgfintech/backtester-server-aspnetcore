using Backtester.Server.Models;
using Capital.GSG.FX.Data.Core.ContractData;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.ViewModels.CreateJob
{
    public class CreateJobStep1bViewModel
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public BacktestJobSettingsModel Settings { get; set; }

        public List<CreateJobStep1bPairViewModel> Pairs { get; set; }

        public List<SelectListItem> CrossesList { get; private set; }

        public CreateJobStep1bViewModel()
        {
            PopulatePairsList();
        }

        public CreateJobStep1bViewModel(BacktestJobSettingsModel settings)
        {
            Settings = settings;

            PopulatePairsList();
        }

        private void PopulatePairsList()
        {
            CrossesList = CrossUtils.AllCrosses.Select(c => new SelectListItem()
            {
                Text = c.ToString(),
                Value = c.ToString()
            }).ToList();

            Pairs = new List<CreateJobStep1bPairViewModel>()
            {
                new CreateJobStep1bPairViewModel()
            };
        }
    }

    public class CreateJobStep1bPairViewModel
    {
        public string Cross { get; set; }
        public int Quantity { get; set; }
    }
}
