﻿namespace TomsToolbox.Wpf
{
    using System;
    using System.Windows.Threading;

    using JetBrains.Annotations;

    /// <summary>
    /// Implements a simple timed throttle.<para/>
    /// Calling <see cref="Tick()"/> multiple times will restart the timer; there will be one single 
    /// call to the action when the delay time has elapsed after the last tick.
    /// </summary>
    public class Throttle
    {
        [NotNull]
        private readonly Action _target;
        [NotNull]
        private readonly DispatcherTimer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Throttle"/> class with a default timeout of 100ms.
        /// </summary>
        /// <param name="target">The target action to invoke when the throttle condition is hit.</param>
        public Throttle([NotNull] Action target)
            : this(TimeSpan.FromMilliseconds(100), target)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Throttle"/> class.
        /// </summary>
        /// <param name="timeout">The timeout to wait for after the last <see cref="Tick()"/>.</param>
        /// <param name="target">The target action to invoke when the throttle condition is hit.</param>
        public Throttle(TimeSpan timeout, [NotNull] Action target)
        {
            _target = target;
            _timer = new DispatcherTimer { Interval = timeout };
            _timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Ticks this instance to trigger the throttle.
        /// </summary>
        public void Tick()
        {
            _timer.Stop();
            _timer.Start();
        }

        private void Timer_Tick(object? sender, [NotNull] EventArgs e)
        {
            _timer.Stop();
            _target();
        }
    }
}
