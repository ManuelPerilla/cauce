using System;
using System.ComponentModel;
using System.Windows;
using Cauce.Desktop.ViewModels;

namespace Cauce.Desktop;

public partial class MainWindow : Window
{
    private bool closeApproved;
    private bool closePending;

    public MainWindow() => InitializeComponent();

    protected override async void OnClosing(CancelEventArgs e)
    {
        if (!closeApproved && DataContext is MainViewModel viewModel)
        {
            e.Cancel = true;
            if (closePending) return;
            closePending = true;
            try
            {
                if (await viewModel.TryCloseAsync())
                {
                    closeApproved = true;
                    _ = Dispatcher.BeginInvoke(new Action(Close));
                }
            }
            finally { closePending = false; }
            return;
        }
        base.OnClosing(e);
    }

    private void OnboardingVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
            Dispatcher.BeginInvoke(new Action(() => TutorialNextButton.Focus()));
        else if (IsLoaded)
            Dispatcher.BeginInvoke(new Action(() => GenrePicker.Focus()));
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is IDisposable disposable)
            disposable.Dispose();
        base.OnClosed(e);
    }
}
