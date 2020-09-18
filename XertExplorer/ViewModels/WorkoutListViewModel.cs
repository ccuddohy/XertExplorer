using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
		public ICommand FilterCommand { get; set; }
		
		private List<XertWorkout> _allWorkOuts;

		public ObservableCollection<string> Filters { get; set; }

		public List<XertWorkout> WorkoutList
		{
			get; 
			private set; 
		}

		public WorkoutListViewModel()
		{
			LoadAllWorkouts();
			WorkoutList = _allWorkOuts;
			
			FilterCommand = new RelayCommand(ExecuteFilterMethod, CanexecutFilterMmethod);
			Filters = new ObservableCollection<string>();
		}

		private void LoadAllWorkouts()
		{
			string JSONtxt = File.ReadAllText("workouts.json");
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

		/// <summary>
		/// to-do
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		private bool CanexecutFilterMmethod(object parameter)
		{
			return true;
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


	}
}
