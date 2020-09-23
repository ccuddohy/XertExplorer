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

		/// <summary>
		/// The inverse of LoggedOff
		/// </summary>
		public bool LoggedOn { get; set; }

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
			//_allWorkOuts = new List<IXertWorkout>();
			//WorkoutListFiltered = new List<IXertWorkout>();
			LoggedOff = true;
			//LoadDemoWorkouts();
			//WorkoutListFiltered = _allWorkOuts;

			FilterCommand = new RelayCommand(ExecuteFilterMethod, CanexecutFilterMethod);
			Filters = new List<string>();
			LoginCommand = new AsyncRelayCommand(Login, (ex) => StatusMessage = ex.Message);
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
			try
			{
				//string JSONtxt = File.ReadAllText("workouts.json");
				//_allWorkOuts = JsonSerializer.Deserialize<List<IXertWorkout>>(JSONtxt);


				//byte[] jsonUtf8Bytes = File.ReadAllBytes("workouts.json");
				//var options = new JsonReaderOptions
				//{
				//	AllowTrailingCommas = true,
				//	CommentHandling = JsonCommentHandling.Skip
				//};
				//var utf8Reader = new Utf8JsonReader(jsonUtf8Bytes);
				//_allWorkOuts = (List<IXertWorkout>)JsonSerializer.Deserialize<IXertWorkout>(ref utf8Reader);
			}
			catch (Exception ex)
			{
				int e = 1;
			}
			
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
			StatusMessage = string.Format("{0} Workouts Loaded", _allWorkOuts.Count());
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyname)
		{
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
		private bool CanexecutFilterMethod(object parameter)
		{
			if(null == _allWorkOuts )
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
					filteredItems.AddRange(_allWorkOuts.Where(X => X.focus == filer));
				}
				WorkoutListFiltered = filteredItems;
			}
			else
			{
				WorkoutListFiltered = _allWorkOuts;
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
				OnPropertyChanged(nameof(Filters));
			}
			else
			{
				Filters.Remove(focusFilter);
				OnPropertyChanged(nameof(Filters));
			}
			ApplyFiltering();

		}


	}
}
