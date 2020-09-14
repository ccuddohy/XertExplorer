using System;
using System.Windows;
using XertExplorer.ViewModels;

namespace XertExplorer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			try
			{
				DataContext = new WorkoutListViewModel();
			}
			catch (Exception ex) 
			{
				MessageBox.Show("An exception was caught: " + ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}
	

	}
}
