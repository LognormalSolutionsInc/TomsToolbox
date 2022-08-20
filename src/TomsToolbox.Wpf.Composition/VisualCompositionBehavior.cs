﻿namespace TomsToolbox.Wpf.Composition;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

using TomsToolbox.Composition;
using TomsToolbox.Essentials;
using TomsToolbox.Wpf.Composition.XamlExtensions;
using TomsToolbox.Wpf.Interactivity;

/// <summary>
/// Base class to implement visual composition behaviors.
/// </summary>
/// <typeparam name="T">The type the VisualCompositionBehavior can be attached to.</typeparam>
/// <remarks>
/// ViewModels declare themselves as candidates for visual composition by adding the VisualCompositionExportAttribute,
/// and the visual composition behaviors dynamically link the view models to the views with the matching region ids.
/// </remarks>
public abstract class VisualCompositionBehavior<T> : FrameworkElementBehavior<T>, IVisualCompositionBehavior
    where T : FrameworkElement
{
    private readonly DispatcherThrottle _deferredUpdateThrottle;

    private INotifyChanged? _exportProviderChangeTracker;
    private IExportProvider? _exportProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="VisualCompositionBehavior{T}"/> class.
    /// </summary>
    protected VisualCompositionBehavior()
    {
        _deferredUpdateThrottle = new DispatcherThrottle(DispatcherPriority.Background, Update);
    }

    /// <summary>
    /// Gets or sets the id of the region. The region id is used to select candidates for composition.
    /// </summary>
    public string? RegionId
    {
        get => (string?)GetValue(RegionIdProperty);
        set => SetValue(RegionIdProperty, value);
    }
    /// <summary>
    /// Identifies the <see cref="RegionId"/> dependency property
    /// </summary>
    public static readonly DependencyProperty RegionIdProperty =
        DependencyProperty.Register("RegionId", typeof(string), typeof(VisualCompositionBehavior<T>), new FrameworkPropertyMetadata((sender, _) => ((VisualCompositionBehavior<T>)sender)?.RegionId_Changed()));


    /// <summary>
    /// Gets or sets the composition context.
    /// </summary>
    public object? CompositionContext
    {
        get => GetValue(CompositionContextProperty);
        set => SetValue(CompositionContextProperty, value);
    }
    /// <summary>
    /// Identifies the <see cref="CompositionContext"/> dependency property
    /// </summary>
    public static readonly DependencyProperty CompositionContextProperty =
        DependencyProperty.Register("CompositionContext", typeof(object), typeof(VisualCompositionBehavior<T>), new FrameworkPropertyMetadata(null, (sender, _) => ((VisualCompositionBehavior<T>)sender)?.Update()));


    /// <summary>
    /// Gets or sets the region identifier binding;
    /// the binding will be applied to the <see cref="RegionId"/> property only after the behavior is attached to the logical tree, so you don't get misleading binding errors.
    /// </summary>
    /// <remarks>
    /// Use this property instead of setting a direct binding to the <see cref="RegionId"/> property if the direct binding will generate binding error message, e.g. in style setters.
    /// </remarks>
    public BindingBase? RegionIdBinding
    {
        get => (BindingBase?)GetValue(_regionIdBindingProperty);
        set => SetValue(_regionIdBindingProperty, value);
    }

    /// <summary>
    /// Backing "field" for the <see cref="RegionIdBinding"/> property.
    /// Internally it must be a <see cref="DependencyProperty"/>, else <see cref="Freezable.Clone"/> would not clone it,
    /// but for the framework the <see cref="RegionIdBinding"/> property must look like a regular property, else it would try to apply the binding instead of simply assigning it.
    /// </summary>
    private static readonly DependencyProperty _regionIdBindingProperty =
        DependencyProperty.Register("InternalRegionIdBinding", typeof(BindingBase), typeof(VisualCompositionBehavior<T>));


    /// <summary>
    /// Gets or sets the composition context binding.
    /// the binding will be applied to the <see cref="RegionId"/> property only after the behavior is attached to the logical tree, so you don't get misleading binding errors.
    /// </summary>
    /// <remarks>
    /// Use this property instead of setting a direct binding to the <see cref="RegionId"/> property if the direct binding will generate binding error message, e.g. in style setters.
    /// </remarks>
    public BindingBase? CompositionContextBinding
    {
        get => (BindingBase?)GetValue(_compositionContextBindingProperty);
        set => SetValue(_compositionContextBindingProperty, value);
    }

    /// <summary>
    /// Backing "field" for the <see cref="CompositionContextBinding"/> property.
    /// Internally it must be a <see cref="DependencyProperty"/>, else <see cref="Freezable.Clone"/> would not clone it,
    /// but for the framework the <see cref="CompositionContextBinding"/> property must look like a regular property, else it would try to apply the binding instead of simply assigning it.
    /// </summary>
    private static readonly DependencyProperty _compositionContextBindingProperty =
        DependencyProperty.Register("InternalCompositionContextBinding", typeof(BindingBase), typeof(VisualCompositionBehavior<T>));

    /// <summary>
    /// Gets or sets the export provider (DI container). The export provider must be registered with the <see cref="ExportProviderLocator"/>.
    /// </summary>
    protected IExportProvider? ExportProvider
    {
        get => InternalExportProvider ??= GetExportProvider();
        private set => InternalExportProvider = value;
    }

    private IExportProvider? InternalExportProvider
    {
        get => _exportProvider;
        set
        {
            if (_exportProvider == value)
                return;

            if (_exportProvider != null)
            {
                _exportProvider.ExportsChanged -= ExportProvider_ExportsChanged;
            }

            _exportProvider = value;

            if (_exportProvider != null)
            {
                _exportProvider.ExportsChanged += ExportProvider_ExportsChanged;
                Update();
            }
        }
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();

        VisualComposition.OnTrace(this, $"OnAttached: {GetType()} to {AssociatedObject?.GetType()}");

        if (RegionIdBinding != null)
            BindingOperations.SetBinding(this, RegionIdProperty, RegionIdBinding);

        if (CompositionContextBinding != null)
            BindingOperations.SetBinding(this, CompositionContextProperty, CompositionContextBinding);

        var associatedObject = AssociatedObject;
        if ((associatedObject == null) || DesignerProperties.GetIsInDesignMode(associatedObject))
            return;

        _exportProviderChangeTracker = associatedObject.Track(ExportProviderLocator.ExportProviderProperty);
    }

    /// <inheritdoc />
    protected override void OnAssociatedObjectLoaded()
    {
        try
        {
            base.OnAssociatedObjectLoaded();

            if (_exportProviderChangeTracker != null)
            {
                _exportProviderChangeTracker.Changed -= ExportProvider_Changed;
                _exportProviderChangeTracker.Changed += ExportProvider_Changed;
            }

            ExportProvider = AssociatedObject?.TryGetExportProvider();

            VisualComposition.OnTrace(this, $"AssociatedObject loaded, export provider is {ExportProvider?.GetType()}");
        }
        catch (Exception ex)
        {
            VisualComposition.OnError(this, ex);
        }
    }

    /// <inheritdoc />
    protected override void OnAssociatedObjectUnloaded()
    {
        base.OnAssociatedObjectUnloaded();

        if (_exportProviderChangeTracker != null)
            _exportProviderChangeTracker.Changed -= ExportProvider_Changed;

        ExportProvider = null;

        VisualComposition.OnTrace(this, "AssociatedObject unloaded");
    }

    /// <summary>
    /// Gets the visual composition exports for the specified region.
    /// </summary>
    /// <param name="regionId">The region identifier.</param>
    /// <returns>The exports for the region, or <c>null</c> if the export provider is not set yet.</returns>
    protected IEnumerable<IExport<object, IVisualCompositionMetadata>>? GetExports(string? regionId)
    {
        return ExportProvider?
            .GetExports<IVisualCompositionMetadata>(typeof(object), VisualComposition.ExportContractName, item => new VisualCompositionMetadata(item))
            .Where(item => item.Metadata?.TargetRegions?.Contains(regionId) == true);
    }

    /// <summary>
    /// Gets the target object for the item.
    /// If the item implements <see cref="IComposablePartFactory"/>, the element returned by the factory is returned;
    /// otherwise the item itself is returned.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The item or the factory generated item.</returns>
    protected object? GetTarget(object? item)
    {
        return (item is IComposablePartFactory partFactory) ? partFactory.GetPart(CompositionContext) : item;
    }

    /// <summary>
    /// Called when any of the constraints have changed and the target needs to be updated.
    /// </summary>
    protected void Update()
    {
        try
        {
            OnUpdate();
        }
        catch (Exception ex)
        {
            VisualComposition.OnError(this, ex);
        }
    }

    /// <summary>
    /// Called when any of the constraints have changed and the target needs to be updated.
    /// </summary>
    /// <remarks>
    /// Derived classes override this to update the target element.
    /// </remarks>
    protected abstract void OnUpdate();

    private IExportProvider? GetExportProvider()
    {
        var associatedObject = AssociatedObject;
        if (associatedObject == null)
            return null;

        var exportProvider = associatedObject.TryGetExportProvider();

        Dispatcher?.BeginInvoke(DispatcherPriority.Input, () =>
        {
            if (IsLoaded && (exportProvider == null))
            {
                VisualComposition.OnError(this, associatedObject.GetMissingExportProviderMessage());
            }
        });

        return exportProvider;
    }

    private void RegionId_Changed()
    {
        VisualComposition.OnTrace(this, "RegionId changed: " + RegionId);
        Update();
    }

    private void ExportProvider_ExportsChanged(object? sender, EventArgs? e)
    {
        // Defer update using a throttle:
        // - Export events may come from any thread, must dispatch to UI thread anyhow.
        // - Adding many catalogs will result in many ExportsChanged events.
        _deferredUpdateThrottle.Tick();
    }

    private void ExportProvider_Changed(object? sender, EventArgs? e)
    {
        ExportProvider = AssociatedObject?.TryGetExportProvider();

        VisualComposition.OnTrace(this, "IExportProvider changed: " + ExportProvider?.GetType());

        _deferredUpdateThrottle.Tick();
    }
}