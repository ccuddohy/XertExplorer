using System;
using System.Collections.Generic;
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
using XertClient;

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
				List<XertWorkout> Wos = GetWorkouts("workouts.json");
				if (Wos.Count > 0)
				{
					ListViewWorkouts.ItemsSource = Wos;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("An exception was caught: " + ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}


		/// <summary>
		/// for development, will load workouts from file. The file is copied to bin on build as set in the VS properties Build Action
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		static List<XertWorkout> GetWorkouts(string path)
		{
			string JSONtxt = File.ReadAllText(path);
			List<XertWorkout> wkouts = JsonSerializer.Deserialize<List<XertWorkout>>(JSONtxt);//JsonSerializer.DeserializeObject<List<XertWorkout>>(JSONtxt);
			return wkouts;
		}
	}
}
