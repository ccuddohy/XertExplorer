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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using XertClient;
using XertExplorer.Commands;

namespace XertExplorer.ViewModels
{
	public class WorkoutListViewModel : INotifyPropertyChanged
	{
		private IXertClient _client;
		public ICommand FocusFilterCommand { get; set; }
		public ICommand DurationFilterCommand { get; set; }
		public ICommand SortCommand { get; set; }
		public ICommand ToggleSortDirection { get; set; }
		public ICommand LoginCommand { get; set; }
		public ICommand RatingFilterCommand { get; set; }

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
		public List<string> FocusFilters { get; set; }
		public List<string> DurationFilters { get; set; }
		public List<string> RatingFilters { get; set; }

		private const string _pathSortOrderAssendingImage = @"/ViewComponents/ArrowDown.jpg"; // the file properties need to be set to resource so they can be used at run-time
		private const string _pathSortOrderDescendingImage = @"/ViewComponents/ArrowUp.jpg"; // the file properties need to be set to resource so they can be used at run-time
		private readonly TimeSpan _lowLimTimeSpan = new TimeSpan(0, 45, 0);
		private readonly TimeSpan _highLimTimeSpan = new TimeSpan(2, 30, 0);
		readonly TimeSpan _minuteIncrementTimeSpan = new TimeSpan(0, 15, 0);

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
			ApplySorting(WorkoutListFilteredSorted);
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
		private List<IXertWorkout> _allWorkouts;

		/// <summary>
		/// a subset of _allWorkouts that is filtered to include only the focus parameters in FocusFilters
		/// </summary>
		private List<IXertWorkout> _focusFilteredWorkouts;
			
		/// <summary>
		/// simply returns a count of all the unfiltered workouts for the user.
		/// </summary>
		public int CountOfAllUserWorkouts
		{
			get
			{
				if (null == _allWorkouts)
				{
					return 0;
				}
				return _allWorkouts.Count();
			}
			private set { }
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
	
		public int FilteredWorkoutCount
		{
			get
			{
				if (null == WorkoutListFilteredSorted)
				{
					return 0;
				}
				return WorkoutListFilteredSorted.Count();
			}
			private set	{}
		}

		private List<IXertWorkout> _workoutListFilteredSorted;
		
		/// <summary>
		/// This is a subset of _allWorkouts. It may be filtered and it may be sorted.
		/// </summary>
		public List<IXertWorkout> WorkoutListFilteredSorted
		{
			get
			{
				return _workoutListFilteredSorted;
			}
			private set
			{
				_workoutListFilteredSorted = value;
				OnPropertyChanged(nameof(WorkoutListFilteredSorted));
				OnPropertyChanged(nameof(FilteredWorkoutCount));
			}
		}

		public WorkoutListViewModel()
		{
			_currentSortingParam = SortingParam.Name;
			AssendingSortDirection = true;
			WorkoutsLoaded = false;
			LoggedOff = true;
			FocusFilters = new List<string>();
			DurationFilters = new List<string>();
			RatingFilters = new List<string>();
			LoadDemoWorkouts();

			FocusFilterCommand = new RelayCommand(ExecuteFocusFilterMethod, CanexecuteFilterMethod);
			DurationFilterCommand = new RelayCommand(ExecuteDurationFilterMethod, CanexecuteFilterMethod);
			RatingFilterCommand = new RelayCommand(ExecuteRatingFilterMethod, CanexecuteFilterMethod);
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
			_allWorkouts = DeserializeWorkoutsFromFile("workouts.json");
			WorkoutsLoaded = true;
			ApplyAllFiltering(true);
			StatusMessage = string.Format("{0} Demo Workouts Loaded", _allWorkouts.Count());
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
			_allWorkouts = await _client.GetUsersWorkouts();
			WorkoutsLoaded = true;
			ApplyAllFiltering(true);
			StatusMessage = string.Format("{0} Workouts Loaded", _allWorkouts.Count());
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyname)
		{
			if(propertyname == "SearchString")
			{
				ApplyAllFiltering(false);
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
			if (null == _allWorkouts )
			{
				return false;
			}
			if(_allWorkouts.Count == 0)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Sets _focusFilteredWorkouts to a subset of _allWorkouts that is filtered by FocusFilters. If FocusFilters is empty,
		/// _focusFilteredWorkouts  is set to _allWorkouts.
		/// </summary>
		/// <returns></returns>
		private void ApplyFocusFilter()
		{
			if (FocusFilters.Count > 0)
			{
				_focusFilteredWorkouts = new List<IXertWorkout>();
				foreach (string focusFilter in FocusFilters)
				{
					_focusFilteredWorkouts.AddRange(_allWorkouts.Where(X => X.focus == focusFilter));
				}
				return;
			}
			_focusFilteredWorkouts =  _allWorkouts;
		}


		private List<IXertWorkout> ApplyRatingFilter(List<IXertWorkout> inputWorkouts)
		{
			if(RatingFilters.Count > 0)
			{
				List<IXertWorkout> RatingFilteredWorkouts = new List<IXertWorkout>();
				foreach (string ratingFilter in RatingFilters)
				{
					RatingFilteredWorkouts.AddRange(inputWorkouts.Where(x => x.rating.Contains(ratingFilter)));
				}
				return RatingFilteredWorkouts;
			}

			return inputWorkouts;
		}

		/// <summary>
		/// returns a subset of the input list, that contains SearchString in the name or description. If SearchString is empty
		/// returns inputWorkouts
		/// </summary>
		/// <param name="inputWorkouts"></param>
		/// <returns></returns>
		private List<IXertWorkout> ApplySearchFilter(List<IXertWorkout> inputWorkouts)
		{
			List<IXertWorkout> searchFiltered = new List<IXertWorkout>();
			if (!string.IsNullOrEmpty(SearchString))
			{
				searchFiltered = inputWorkouts.Where(w => w.name.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
					|| w.description.Contains(SearchString, StringComparison.OrdinalIgnoreCase)).ToList();
				return searchFiltered;
			}
			return inputWorkouts;
		}

		private TimeSpan StringToTimeSpan(string input)
		{
			TimeSpan intervalVal;
			if (TimeSpan.TryParse(input, out intervalVal))
			{
				return intervalVal;
			}
			else
			{
				StatusMessage = string.Format("Error converting {0} to TimeSpan", input);
				return new TimeSpan(0,0,0);
			}
		}

		private List<IXertWorkout> ApplyDurationFilter(List<IXertWorkout> inputWorkouts)
		{
			List<IXertWorkout> durationFilteredWorkouts = new List<IXertWorkout>();
			if (DurationFilters.Count > 0)
			{
				foreach (string durationFilter in DurationFilters)
				{
					if (durationFilter.Contains("Less than", StringComparison.OrdinalIgnoreCase))
					{
						string parsedDurationStr = Regex.Replace(durationFilter, "Less than", "", RegexOptions.IgnoreCase);
						TimeSpan parsedDuration = StringToTimeSpan(parsedDurationStr);
						durationFilteredWorkouts.AddRange(inputWorkouts.Where(X => TimeSpan.Parse(X.duration) < parsedDuration));
					}
					else if (durationFilter.Contains("More than", StringComparison.OrdinalIgnoreCase))
					{
						string parsedDurationStr = Regex.Replace(durationFilter, "More than", "", RegexOptions.IgnoreCase);
						TimeSpan parsedDuration = StringToTimeSpan(parsedDurationStr);
						durationFilteredWorkouts.AddRange(inputWorkouts.Where(X => TimeSpan.Parse(X.duration) > parsedDuration));
					}
					else
					{
						TimeSpan parsedTimeSpan = StringToTimeSpan(durationFilter);
						TimeSpan low = parsedTimeSpan - (.5 * _minuteIncrementTimeSpan);
						if(low <= _lowLimTimeSpan)
						{
							low = _lowLimTimeSpan;
						}
						TimeSpan high = parsedTimeSpan + (.5 * _minuteIncrementTimeSpan);
						if (high >= _highLimTimeSpan)
						{
							high = _highLimTimeSpan;
						}
						durationFilteredWorkouts.AddRange(inputWorkouts.Where(X => TimeSpan.Parse(X.duration) >= low && TimeSpan.Parse(X.duration) <= high));
					}
				}
				return durationFilteredWorkouts;
			}
			return inputWorkouts;
		}

		/// <summary>
		/// Applies filtering to _allWorkouts and sorts the resulting list before setting it WorkoutListFilteredSorted
		/// </summary>
		private void ApplyAllFiltering(bool includeFocusFilter)
		{
			if(includeFocusFilter)
			{
				ApplyFocusFilter();
			}
			List<IXertWorkout>  focusAndSearchFilteredWOs = ApplySearchFilter(_focusFilteredWorkouts);
			List<IXertWorkout> focusSearchDurFilteredWOs = ApplyDurationFilter(focusAndSearchFilteredWOs);
			List<IXertWorkout> focusSearchDurRatingFilteredWOs = ApplyRatingFilter(focusSearchDurFilteredWOs);
			ApplySorting(focusSearchDurRatingFilteredWOs);
		}

		/// <summary>
		/// This sets WorkoutListFilteredSorted to be displayed on the UI after sorting per _currentSortingParam
		/// </summary>
		/// <param name="workoutLstFiltered"></param>
		private void ApplySorting(List<IXertWorkout> workoutLstFiltered)
		{
			List<IXertWorkout> sortedWorkouts = new List<IXertWorkout>();
			if (_currentSortingParam == SortingParam.Name)
			{
				if (AssendingSortDirection)
				{
					sortedWorkouts = workoutLstFiltered.OrderBy(o => o.name).ToList();
				}
				else
				{
					sortedWorkouts = workoutLstFiltered.OrderByDescending(o => o.name).ToList();
				}
			}
			else if (_currentSortingParam == SortingParam.Duration)
			{
				if (AssendingSortDirection)
				{
					sortedWorkouts = workoutLstFiltered.OrderBy(o => o.duration).ToList();
				}
				else
				{
					sortedWorkouts = workoutLstFiltered.OrderByDescending(o => o.duration).ToList();
				}
			}
			else if (_currentSortingParam == SortingParam.XSS)
			{
				if (AssendingSortDirection)
				{
					sortedWorkouts = workoutLstFiltered.OrderBy(o => o.xss).ToList();
				}
				else
				{
					sortedWorkouts = workoutLstFiltered.OrderByDescending(o => o.xss).ToList();
				}
			}
			else if (_currentSortingParam == SortingParam.Difficulty)
			{
				if (AssendingSortDirection)
				{
					sortedWorkouts = workoutLstFiltered.OrderBy(o => o.difficulty).ToList();
				}
				else
				{
					sortedWorkouts = workoutLstFiltered.OrderByDescending(o => o.difficulty).ToList();
				}
			}
			else if (_currentSortingParam == SortingParam.AdvisorScore)
			{
				if (AssendingSortDirection)
				{
					sortedWorkouts = workoutLstFiltered.OrderBy(o => o.advisorScore).ToList();
				}
				else
				{
					sortedWorkouts = workoutLstFiltered.OrderByDescending(o => o.advisorScore).ToList();
				}
			}
			else
			{
				sortedWorkouts = workoutLstFiltered;
			}
			WorkoutListFilteredSorted = sortedWorkouts;
		}

		private void ExecuteDurationFilterMethod(object parameter)
		{
			if (null != parameter)
			{
				var values = (object[])parameter;
				string durationFilter = (string)values[0];
				bool check = (bool)values[1];
				if (check)
				{
					DurationFilters.Add(durationFilter);
					OnPropertyChanged(nameof(DurationFilters));
				}
				else
				{
					DurationFilters.Remove(durationFilter);
					OnPropertyChanged(nameof(DurationFilters));
				}
			}
			ApplyAllFiltering(false);
		}

		private void ExecuteRatingFilterMethod(object parameter)
		{
			if (null != parameter)
			{
				var values = (object[])parameter;
				string ratingFilter = (string)values[0];
				bool check = (bool)values[1];
				if (check)
				{
					RatingFilters.Add(ratingFilter);
					OnPropertyChanged(nameof(RatingFilters));
				}
				else
				{
					RatingFilters.Remove(ratingFilter);
					OnPropertyChanged(nameof(RatingFilters));
				}
			}
			ApplyAllFiltering(false);
		}

		private void ExecuteFocusFilterMethod(object parameter)
		{
			if(null != parameter)
			{
				var values = (object[])parameter;
				string focusFilter = (string)values[0];
				bool check = (bool)values[1];
				if (check)
				{
					FocusFilters.Add(focusFilter);
					OnPropertyChanged(nameof(FocusFilters));
				}
				else
				{
					FocusFilters.Remove(focusFilter);
					OnPropertyChanged(nameof(FocusFilters));
				}
			}
			ApplyAllFiltering(true);
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
			ApplySorting(WorkoutListFilteredSorted);
		}
		
		private bool CanToggleSortOrderDirection(object parameter)
		{
			if (null == WorkoutListFilteredSorted)
			{
				return false;
			}
			if (WorkoutListFilteredSorted.Count == 0)
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
			if (null == WorkoutListFilteredSorted)
			{
				return false;
			}
			if (WorkoutListFilteredSorted.Count == 0)
			{
				return false;
			}
			return true;
		}
	}
}
