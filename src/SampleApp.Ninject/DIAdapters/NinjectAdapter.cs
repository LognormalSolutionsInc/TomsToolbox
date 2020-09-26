﻿namespace SampleApp.Ninject.DIAdapters
{
    using System;
    using System.Windows.Data;

    using global::Ninject;
    using global::Ninject.Extensions.Conventions;

    using JetBrains.Annotations;

    using TomsToolbox.Composition;
    using TomsToolbox.Composition.Ninject;
    using TomsToolbox.Wpf.Converters;

    internal sealed class DIAdapter : IDisposable
    {
        [CanBeNull]
        private IKernel? _kernel;

        public IExportProvider Initialize()
        {
            var kernel = new StandardKernel();

            kernel.BindExports(typeof(MainWindow).Assembly);

            var exportProvider = new ExportProvider(kernel);

            kernel.Bind<IExportProvider>().ToConstant(exportProvider);
            kernel.Bind(syntax => syntax
                .From(typeof(CoordinatesToPointConverter).Assembly)
                .Select(type => typeof(IValueConverter).IsAssignableFrom(type))
                .BindToSelf());

            _kernel = kernel;
            return exportProvider;
        }

        public void Dispose()
        {
            _kernel?.Dispose();
        }
    }
}
