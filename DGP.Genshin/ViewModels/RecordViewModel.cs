﻿using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.DataModels.MiHoYo2;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
    public class RecordViewModel : ObservableObject
    {
        private readonly IRecordService recordService;
        public RecordViewModel(IRecordService recordService)
        {
            this.recordService = recordService;
        }

        private Record? currentRecord;
        public Record? CurrentRecord { get => currentRecord; set => SetProperty(ref currentRecord, value); }

        private bool isQuerying = false;
        public bool IsQuerying { get => isQuerying; set => SetProperty(ref isQuerying, value); }

        public IRecordService RecordService => recordService;
    }
}