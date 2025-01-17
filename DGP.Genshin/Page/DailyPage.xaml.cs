﻿using DGP.Genshin.ViewModel;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// DailyPage.xaml 的交互逻辑
    /// </summary>
    public partial class DailyPage : System.Windows.Controls.Page
    {
        public DailyPage()
        {
            DataContext = App.AutoWired<DailyViewModel>();
            InitializeComponent();
        }
    }
}