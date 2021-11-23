using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BuildingPlanCalc
{
    public class WheelSpeedScrollViewer : ScrollViewer
    {
        public static readonly DependencyProperty SpeedFactorProperty =
            DependencyProperty.Register(nameof(SpeedFactor),
                                        typeof(double),
                                        typeof(WheelSpeedScrollViewer),
                                        new PropertyMetadata(0.2));

        public double SpeedFactor
        {
            get { return (double)GetValue(SpeedFactorProperty); }
            set { SetValue(SpeedFactorProperty, value); }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (!e.Handled &&
                ScrollInfo is ScrollContentPresenter scp &&
                ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                scp.SetVerticalOffset(VerticalOffset - e.Delta * SpeedFactor);
                e.Handled = true;
            }
        }
    };
}
