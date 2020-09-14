using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XertExplorer.ViewModels;
//using XertClient;

namespace XertExplorer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		//List<XertWorkout> AllWorkouts;
		//ObservableCollection<XertWorkout> FilteredWorkouts;

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

		private void HandleEndurCheck(object sender, RoutedEventArgs e)
		{
			
		}

		private void HandleEndurUnchecked(object sender, RoutedEventArgs e)
		{
		}

		private void HandleClimberCheck(object sender, RoutedEventArgs e)
		{
		}

		private void HandleClimberUnchecked(object sender, RoutedEventArgs e)
		{
		}

		private void HandleGCCheck(object sender, RoutedEventArgs e)
		{
		}

		private void HandleGCUnchecked(object sender, RoutedEventArgs e)
		{
		}

		private void HandleRoulerCheck(object sender, RoutedEventArgs e)
		{
		}

		private void HandleRoulerUnchecked(object sender, RoutedEventArgs e)
		{
		}

		private void HandleBreakAwyCheck(object sender, RoutedEventArgs e)
		{
		}

		private void HandleBreakAwyUnchecked(object sender, RoutedEventArgs e)
		{
		}

		private void HandlePuncheurCheck(object sender, RoutedEventArgs e)
		{
		}

		private void HandlePuncheurUnchecked(object sender, RoutedEventArgs e)
		{
		}

		private void HandlePursuiterCheck(object sender, RoutedEventArgs e)
		{
		}

		private void HandlePursuiterUnchecked(object sender, RoutedEventArgs e)
		{
		}

		private void HandleRoadSprinterCheck(object sender, RoutedEventArgs e)
		{
		}

		private void HandleRoadSprinterUnchecked(object sender, RoutedEventArgs e)
		{
		}

	

	}
}
