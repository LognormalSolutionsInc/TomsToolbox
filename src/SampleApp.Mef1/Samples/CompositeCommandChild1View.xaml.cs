﻿namespace SampleApp.Mef1.Samples
{
    using TomsToolbox.Wpf.Composition.Mef;

    /// <summary>
    /// Interaction logic for CommandViewChild1.xaml
    /// </summary>
    [DataTemplate(typeof(CompositeCommandChild1ViewModel))]
    public partial class CompositeCommandChild1View
    {
        public CompositeCommandChild1View()
        {
            InitializeComponent();
        }
    }
}
