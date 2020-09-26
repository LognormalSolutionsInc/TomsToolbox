﻿namespace SampleApp.Mef1.Samples
{
    using System.ComponentModel.Composition;

    using TomsToolbox.Wpf.Composition.Mef;

    /// <summary>
    /// Interaction logic for SharedWidthSampleView.xaml
    /// </summary>
    [DataTemplate(typeof(SharedWidthSampleViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SharedWidthSampleView
    {
        public SharedWidthSampleView()
        {
            InitializeComponent();
        }
    }
}
