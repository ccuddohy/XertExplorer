using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using XertClient;
using XertExplorer.Commands;

namespace XertExplorer.ViewModels
{
	internal class WorkoutListViewModel:INotifyPropertyChanged
	{
		IXertClient _client; 
		public ICommand FilterCommand { get; set; }
		public ICommand DemoModeCommand { get; set; }

		private List<XertWorkout> _allWorkOuts;

		public string UserName { get; set; }
		public string Password { get; set; }

		public ObservableCollection<string> Filters { get; set; }

		public bool DemoMode { get; set; }

		public List<XertWorkout> WorkoutList
		{
			get; 
			private set; 
		}

		public WorkoutListViewModel()
		{
			DemoMode = true;
			LoadAllWorkouts();
			WorkoutList = _allWorkOuts;
			DemoMode = true;
			FilterCommand = new RelayCommand(ExecuteFilterMethod, CanexecutFilterMmethod);
			Filters = new ObservableCollection<string>();

			DemoModeCommand = new RelayCommand(ExecuteDemoModeMethod, CanexexecuteDemoModeMethod);
		}
		
		public void LogInLogOff()
		{
			if (null == _client)
			{
				IXertClient _client = new Client();
			}
		}

		private void LoadAllWorkouts()
		{
			string JSONtxt = "";
			if (DemoMode)
			{
				JSONtxt = File.ReadAllText("workouts.json");
			}
			
			_allWorkOuts = JsonSerializer.Deserialize<List<XertWorkout>>(JSONtxt);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChange(string propertyname)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
			}
		}

		private void ExecuteDemoModeMethod(object parameter)
		{
			var values = (object[])parameter;
			string focusFilter = (string)values[0];
			bool check = (bool)values[1];
			if (check)
			{
				DemoMode = true;
				LoadAllWorkouts();
				WorkoutList = _allWorkOuts;
				ApplyFiltering();
			}
			else
			{
				DemoMode = false;
				//if not demo mode and logged in, load the real workouts and apply the filtering

				//if not demo mode and not logged in clear all workouts
				_allWorkOuts = new List<XertWorkout>();
				
				WorkoutList = _allWorkOuts;
				OnPropertyChange("WorkoutList");
			}
		}

		private bool CanexexecuteDemoModeMethod(object parameter)
		{
			return true;
		}


		/// <summary>
		/// to-do
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		private bool CanexecutFilterMmethod(object parameter)
		{
			return true;
		}

		private void ApplyFiltering()
		{
			if (Filters.Count > 0)
			{
				List<XertWorkout> filteredItems = new List<XertWorkout>();
				foreach (string filer in Filters)
				{
					filteredItems.AddRange(_allWorkOuts.Where(X => X.focus == filer));
				}
				WorkoutList = filteredItems;
				OnPropertyChange("WorkoutList");
			}
			else
			{
				WorkoutList = _allWorkOuts;
				OnPropertyChange("WorkoutList");
			}
		}

		private void ExecuteFilterMethod(object parameter)
		{
			var values = (object[])parameter;
			string focusFilter = (string)values[0];
			bool check = (bool)values[1];
			if (check)
			{
				Filters.Add(focusFilter);
				OnPropertyChange("Filters");
			}
			else
			{
				Filters.Remove(focusFilter);
				OnPropertyChange("Filters");
			}
			ApplyFiltering();

		}


	}
}
