﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls.Records
{
    public class ActivityTemplateSelector : DataTemplateSelector
    {
        private static readonly Dictionary<string, string> TemplateDict = new Dictionary<string, string> 
        { 
            { "activities[0]", "Effigy" },
            { "activities[1]", "Mechanicus" },
            { "activities[2]", "FleurFair" },
            { "activities[3]", "ChannellerSlab" },
            { "activities[4]", "MartialLegend" },
            { "activities[5]", "Chess" }
        };

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element && item != null)
            {
                JToken token = item as JToken;
                string path = token.Path;
                return element.FindResource(TemplateDict[path]) as DataTemplate;
            }
            return null;
        }
    }
}