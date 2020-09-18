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

		private ObservableCollection<string> _filters;
		public ObservableCollection<string> Filters
		{
			get { return _filters; ; }
			set
			{
				_filters = value;
				OnPropertyChange("Filters");
			}
		}

		private List<XertWorkout> _workoutList;
		public List<XertWorkout> WorkoutList
		{
			get { 
				return _workoutList; ; 
			}
			set
			{
				_workoutList = value;
				OnPropertyChange("WorkoutList");
			}
		}

		public WorkoutListViewModel()
		{
			LoadAllWorkouts();
			WorkoutList = _allWorkOuts;
			
			FilterCommand = new RelayCommand(executeFilterMethod, canexecutFilterMmethod);
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
		/// todo
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		private bool canexecutFilterMmethod(object parameter)
		{
			return true;
		}

		private void executeFilterMethod(object parameter)
		{
			var values = (object[])parameter;
			string focusFilter = (string)values[0];
			bool check = (bool)values[1];
			if (check)
			{
				Filters.Add(focusFilter);
			}
			else
			{
				Filters.Remove(focusFilter);
			}

			if (Filters.Count > 0)
			{
				List<XertWorkout> filteredItems = new List<XertWorkout>();
				foreach (string filer in Filters)
				{
					filteredItems.AddRange(_allWorkOuts.Where(X => X.focus == filer));
				}
				WorkoutList = filteredItems;
			}
			else
			{
				WorkoutList = _allWorkOuts;
			}
		}


	}
}
