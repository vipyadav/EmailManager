using MvvmCross;
using Serilog;
using System;
using System.ComponentModel;
using System.Windows;

namespace EmailManager.Common
{
    public static class ViewModelLocator
    {
        public static bool GetAutoHookedUpViewModel(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoHookedUpViewModelProperty);
        }

        public static void SetAutoHookedUpViewModel(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoHookedUpViewModelProperty, value);
        }

        // Using a DependencyProperty as the backing store for AutoHookedUpViewModel. 
        public static readonly DependencyProperty AutoHookedUpViewModelProperty =
           DependencyProperty.RegisterAttached("AutoHookedUpViewModel",
           typeof(bool), typeof(ViewModelLocator), new
           PropertyMetadata(false, AutoHookedUpViewModelChanged));

        private static void AutoHookedUpViewModelChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d)) return;

            var viewTypeName = string.Empty;
            try
            {
                var viewType = d.GetType();

                var viewName = viewType.FullName;
                viewName = viewName.Replace(".Views.", ".ViewModels.");

                viewTypeName = viewName;
                var viewModelTypeName = viewTypeName + "Model";
                var viewModelType = Type.GetType(viewModelTypeName);
                var viewModel = Mvx.IoCProvider.IoCConstruct(viewModelType); //Activator.CreateInstance(viewModelType);
                ((FrameworkElement)d).DataContext = viewModel;
            }
            catch (Exception)
            {
                Log.Error("Failed to create ViewModel for view '{0}' from locator - check InnerException for more information",
                   viewTypeName);
            }
        }
    }
}
