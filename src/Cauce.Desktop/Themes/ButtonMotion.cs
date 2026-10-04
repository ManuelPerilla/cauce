using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cauce.Desktop.Themes;

/// <summary>Brief pointer feedback; no animation is scheduled while reduced motion is enabled.</summary>
public static class ButtonMotion
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(ButtonMotion), new PropertyMetadata(false, Changed));
    public static bool GetIsEnabled(DependencyObject target) => (bool)target.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject target, bool value) => target.SetValue(IsEnabledProperty, value);

    private static void Changed(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        if (target is not Button button) return;
        button.MouseEnter -= Enter;
        button.MouseLeave -= Leave;
        button.PreviewMouseLeftButtonDown -= Down;
        button.PreviewMouseLeftButtonUp -= Up;
        if ((bool)e.NewValue)
        {
            button.RenderTransformOrigin = new Point(.5, .5);
            button.RenderTransform = new ScaleTransform(1, 1);
            button.MouseEnter += Enter;
            button.MouseLeave += Leave;
            button.PreviewMouseLeftButtonDown += Down;
            button.PreviewMouseLeftButtonUp += Up;
        }
        else if (button.RenderTransform is ScaleTransform scale)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            scale.ScaleX = scale.ScaleY = 1;
        }
    }

    private static void Enter(object sender, MouseEventArgs e) => Animate((Button)sender, 1.015);
    private static void Leave(object sender, MouseEventArgs e) => Animate((Button)sender, 1);
    private static void Down(object sender, MouseButtonEventArgs e) => Animate((Button)sender, .975);
    private static void Up(object sender, MouseButtonEventArgs e) => Animate((Button)sender, ((Button)sender).IsMouseOver ? 1.015 : 1);

    private static void Animate(Button button, double value)
    {
        if (!GetIsEnabled(button) || !button.IsEnabled || !SystemParameters.ClientAreaAnimation || button.RenderTransform is not ScaleTransform scale) return;
        var animation = new DoubleAnimation(value, TimeSpan.FromMilliseconds(110))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
    }
}
