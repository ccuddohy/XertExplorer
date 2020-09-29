using AsyncCommands.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XertClient;
using XertExplorer.Commands;

namespace XertExplorer.ViewModels
{
	public class WorkoutListViewModel : INotifyPropertyChanged
	{
		IXertClient _client;
		public ICommand FilterCommand { get; set; }
		public ICommand SortCommand { get; set; }
		public ICommand ToggleSortDirection { get; set; }
		

		private enum SortingParam
		{
			None,
			Name,
			Duration,
			XSS,
			Difficulty,
			AdvisorScore
		};
		SortingParam _currentSortingParam;

		
		const string _pathSortOrderAssendingImage = @"/ViewComponents/ArrowDown.jpg"; // the file properties need to be set to resource so they can be used at run-time
		const string _pathSortOrderDescendingImage = @"/ViewComponents/ArrowUp.jpg"; // the file properties need to be set to resource so they can be used at run-time

		private string _pathSortOrderDirectionImage;
		public string PathSortOrderDirectionImage
		{
			get
			{
				return _pathSortOrderDirectionImage;
			}
			set
			{
				_pathSortOrderDirectionImage = value;
				OnPropertyChanged(nameof(PathSortOrderDirectionImage));
			}
		}
		private bool _assendingSortDirection;
		public bool AssendingSortDirection
		{
			get
			{
				return _assendingSortDirection;
			}
			set
			{
				_assendingSortDirection = value;
				if (_assendingSortDirection)
				{
					PathSortOrderDirectionImage = _pathSortOrderAssendingImage;
				}
				else
				{
					PathSortOrderDirectionImage = _pathSortOrderDescendingImage;
				}
				OnPropertyChanged(nameof(AssendingSortDirection));
			}
		}

		private string _searchString;
		public string SearchString
		{
			get
			{
				return _searchString;
			}
			set
			{
				_searchString = value;
				OnPropertyChanged(nameof(SearchString));
			}
		}

		private void ToggleSortOrderDirection(object parameter)
		{
			AssendingSortDirection = !AssendingSortDirection;
			ApplySorting();
		}

		/// <summary>
		/// The inverse of LoggedOff
		/// </summary>
		public bool LoggedOn { get; set; }

		private bool _loggedOff;
		/// <summary>
		/// The state of the users login status
		/// </summary>
		public bool LoggedOff
		{
			get
			{
				return _loggedOff;
			}
			set
			{
				_loggedOff = value;
				LoggedOn = !value;
				OnPropertyChanged(nameof(LoggedOff));
				OnPropertyChanged(nameof(LoggedOn));
			}
		}

		private bool _workoutsLoaded;
		public bool WorkoutsLoaded
		{
			get
			{
				return _workoutsLoaded;
			}
			set
			{
				_workoutsLoaded = value;
				OnPropertyChanged(nameof(WorkoutsLoaded));
			}
		}

		/// <summary>
		/// This is a List of all the workouts for the user. It is unfiltered and not sorted.
		/// </summary>
		private List<IXertWorkout> _allWorkOuts;

		/// <summary>
		/// simply returns a count of all the unfiltered workouts for the user.
		/// </summary>
		public int CountOfAllUserWorkouts
		{
			get
			{
				if (null == _allWorkOuts)
				{
					return 0;
				}
				return _allWorkOuts.Count();
			}
			private set	{}
		}

		private string _username;
		public string Username
		{
			get
			{
				return _username;
			}
			set
			{
				_username = value;
				OnPropertyChanged(nameof(Username));
			}
		}

		private string _statusMessage;
		
		/// <summary>
		/// An indication of login operations and workout loading status
		/// </summary>
		public string StatusMessage
		{
			get
			{
				return _statusMessage;
			}
			set
			{
				_statusMessage = value;
				OnPropertyChanged(nameof(StatusMessage));
				OnPropertyChanged(nameof(HasStatusMessage));
			}
		}

		public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

		private string _password;
		public string Password
		{
			get
			{
				return _password;
			}
			set
			{
				_password = value;
				OnPropertyChanged(nameof(Password));
			}
		}

		public ICommand LoginCommand { get; }
	
		public List<string> Filters { get; set; }

		public int FilteredWorkoutCount
		{
			get
			{
				if (null == WorkoutListFiltered)
				{
					return 0;
				}
				return WorkoutListFiltered.Count();
			}
			private set	{}
		}

		private List<IXertWorkout> _workoutListFiltered;
		
		/// <summary>
		/// This is a subset of _allWorkouts. It may be filtered and it may be sorted.
		/// </summary>
		public List<IXertWorkout> WorkoutListFiltered
		{
			get
			{
				return _workoutListFiltered;
			}
			private set
			{
				_workoutListFiltered = value;
				OnPropertyChanged(nameof(WorkoutListFiltered));
				OnPropertyChanged(nameof(FilteredWorkoutCount));
			}
		}

		public WorkoutListViewModel()
		{
			_currentSortingParam = SortingParam.Name;
			AssendingSortDirection = true;
			WorkoutsLoaded = false;
			LoggedOff = true;
			Filters = new List<string>(); 
			LoadDemoWorkouts();

			FilterCommand = new RelayCommand(ExecuteFilterMethod, CanexecuteFilterMethod);
			SortCommand = new RelayCommand(ExecuteSortMethod, CanSortMethod);
			LoginCommand = new AsyncRelayCommand(Login, (ex) => StatusMessage = ex.Message);
			ToggleSortDirection = new RelayCommand(ToggleSortOrderDirection, CanToggleSortOrderDirection);
		}


		public bool CanExecuteLogin(object parameter)
		{
			return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
		}
	
		private async Task Login()
		{
			StatusMessage = "Logging in...";

			_client = new Client();
			await _client.Login(Username, Password);
			StatusMessage = "Logged in";
			LoggedOff = false; 
			await LoadAllWorkouts();
		}

		private void LoadDemoWorkouts()
		{
			_allWorkOuts = DeserializeWorkoutsFromFile("workouts.json");
			WorkoutListFiltered = _allWorkOuts;
			WorkoutsLoaded = true;
			ApplyFiltering();
			StatusMessage = string.Format("{0} Demo Workouts Loaded", _allWorkOuts.Count());
		}

		static List<IXertWorkout> DeserializeWorkoutsFromFile(string fileName)
		{
			List<IXertWorkout> workouts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IXertWorkout>>(File.ReadAllText(fileName), new Newtonsoft.Json.JsonSerializerSettings
			{
				TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
				NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
			});
			return workouts;
		}

		private async Task LoadAllWorkouts()
		{
			if (LoggedOff)
			{
				return;
			}
			StatusMessage = "Loading Workouts...";
			_allWorkOuts = await _client.GetUsersWorkouts();
			WorkoutListFiltered = _allWorkOuts;
			WorkoutsLoaded = true;
			ApplyFiltering();
			StatusMessage = string.Format("{0} Workouts Loaded", _allWorkOuts.Count());
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyname)
		{
			if(propertyname == "SearchString")
			{
				ApplyFiltering();
			}
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
			}
		}

		/// <summary>
		/// need to have workouts before they can be filtered
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		private bool CanexecuteFilterMethod(object parameter)
		{
			if (null == _allWorkOuts )
			{
				return false;
			}
			if(_allWorkOuts.Count == 0)
			{
				return false;
			}
			return true;
		}
		
		private void ApplyFiltering()
		{
			if (Filters.Count > 0)
			{
				List<IXertWorkout> filteredItems = new List<IXertWorkout>();
				foreach (string filer in Filters)
				{
					filteredItems = _allWorkOuts.Where(X => X.focus == filer).ToList();
				}
				if (!string.IsNullOrEmpty(SearchString))
				{
					filteredItems = filteredItems.Where(w => w.name.Contains(SearchString, StringComparison.OrdinalIgnoreCase) 
						|| w.description.Contains(SearchString, StringComparison.OrdinalIgnoreCase)).ToList();
				}
				WorkoutListFiltered = filteredItems;
			}
			else
			{
				List<IXertWorkout> filteredItems = new List<IXertWorkout>();
				if (!string.IsNullOrEmpty(SearchString))
				{
					filteredItems = _allWorkOuts.Where(w => w.name.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
						|| w.description.Contains(SearchString, StringComparison.OrdinalIgnoreCase)).ToList();
					WorkoutListFiltered = filteredItems;
				}
				else 
				{
					WorkoutListFiltered = _allWorkOuts;
				}
			}
			ApplySorting();
		}

		private void ApplySorting()
		{
			List<IXertWorkout> sortedWorkouts = WorkoutListFiltered;
			if (_currentSortingParam == SortingParam.Name)
			{
				if (AssendingSortDirection)
				{
					sortedWorkouts = WorkoutListFiltered.OrderBy(o => o.name).ToList();
				}
				else
				{
					sortedWorkouts = WorkoutListFiltered.OrderByDescending(o => o.name).ToList();
				}
			}
			if (_currentSortingParam == SortingParam.Duration)
			{
				if (AssendingSortDirection)
				{
					sortedWorkouts = WorkoutListFiltered.OrderBy(o => o.duration).ToList();
				}
				else
				{
					sortedWorkouts = WorkoutListFiltered.OrderByDescending(o => o.duration).ToList();
				}
			}
			if (_currentSortingParam == SortingParam.XSS)
			{
				if (AssendingSortDirection)
				{
					sortedWorkouts = WorkoutListFiltered.OrderBy(o => o.xss).ToList();
				}
				else
				{
					sortedWorkouts = WorkoutListFiltered.OrderByDescending(o => o.xss).ToList();
				}
			}
			if (_currentSortingParam == SortingParam.Difficulty)
			{
				if (AssendingSortDirection)
				{
					sortedWorkouts = WorkoutListFiltered.OrderBy(o => o.difficulty).ToList();
				}
				else
				{
					sortedWorkouts = WorkoutListFiltered.OrderByDescending(o => o.difficulty).ToList();
				}
			}
			if (_currentSortingParam == SortingParam.AdvisorScore)
			{
				if (AssendingSortDirection)
				{
					sortedWorkouts = WorkoutListFiltered.OrderBy(o => o.advisorScore).ToList();
				}
				else
				{
					sortedWorkouts = WorkoutListFiltered.OrderByDescending(o => o.advisorScore).ToList();
				}
			}
			WorkoutListFiltered = sortedWorkouts;
		}

		private void ExecuteFilterMethod(object parameter)
		{
			if(null != parameter)
			{
				var values = (object[])parameter;
				string focusFilter = (string)values[0];
				bool check = (bool)values[1];
				if (check)
				{
					Filters.Add(focusFilter);
					OnPropertyChanged(nameof(Filters));
				}
				else
				{
					Filters.Remove(focusFilter);
					OnPropertyChanged(nameof(Filters));
				}
			}
			
			ApplyFiltering();
		}

		private void ExecuteSortMethod(object parameter)
		{
			var sortParam = (string)parameter;
			if (String.Equals(sortParam, "Name", StringComparison.OrdinalIgnoreCase))
			{
				_currentSortingParam = SortingParam.Name;
			}
			else if (String.Equals(sortParam, "Duration", StringComparison.OrdinalIgnoreCase))
			{
				_currentSortingParam = SortingParam.Duration;
			}
			else if (String.Equals(sortParam, "XSS", StringComparison.OrdinalIgnoreCase))
			{
				_currentSortingParam = SortingParam.XSS;
			}
			else if (String.Equals(sortParam, "Difficulty", StringComparison.OrdinalIgnoreCase))
			{
				_currentSortingParam = SortingParam.Difficulty;
			}
			else if(String.Equals(sortParam, "Advisor Score", StringComparison.OrdinalIgnoreCase))
			{
				_currentSortingParam = SortingParam.AdvisorScore;
			}
			else
			{
				_currentSortingParam = SortingParam.None;
			}
			ApplySorting();
		}
		
		private bool CanToggleSortOrderDirection(object parameter)
		{
			if (null == WorkoutListFiltered)
			{
				return false;
			}
			if (WorkoutListFiltered.Count == 0)
			{
				return false;
			}
			if(_currentSortingParam == SortingParam.None)
			{
				return false;
			}
			return true;
		}


		private bool CanSortMethod(object parameter)
		{
			if (null == WorkoutListFiltered)
			{
				return false;
			}
			if (WorkoutListFiltered.Count == 0)
			{
				return false;
			}
			return true;
		}
	}
}
