using System.Windows;

namespace Sce.Atf.Wpf;

public delegate MessageBoxResult ShowMessageBoxDelegate(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon);
