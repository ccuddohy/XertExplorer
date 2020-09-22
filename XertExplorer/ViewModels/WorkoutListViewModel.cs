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

		public bool LoggedOn
		{
			get
			{
				return !LoggedOff;
			}
			private set { }
		}

		public bool LoggedOff { get; set; }
		

		public bool LoginError { get; set; }

		private List<IXertWorkout> _allWorkOuts;

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
	
		public ObservableCollection<string> Filters { get; set; }

		public int WorkoutCount
		{
			get
			{
				if (null == WorkoutList)
				{
					return 0;
				}
				return WorkoutList.Count();
			}
			private set	{}
		}


		public List<IXertWorkout> WorkoutList
		{
			get
			{
				return WorkoutList;
			}
			private set
			{
				WorkoutList = value;
				OnPropertyChanged("WorkoutList");
				OnPropertyChanged("WorkoutCount");
			}
		}

		public WorkoutListViewModel()
		{
			LoggedOff = true;
			//LoadDemoWorkouts();
			//WorkoutList = _allWorkOuts;

			FilterCommand = new RelayCommand(ExecuteFilterMethod, CanexecutFilterMmethod);
			Filters = new ObservableCollection<string>();
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
			LoggedOff = false;
			OnPropertyChanged("LoggedOff");
			StatusMessage = "Logged in";

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
			WorkoutList = _allWorkOuts;
			//OnPropertyChanged("WorkoutList");
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
				List<IXertWorkout> filteredItems = new List<IXertWorkout>();
				foreach (string filer in Filters)
				{
					filteredItems.AddRange(_allWorkOuts.Where(X => X.focus == filer));
				}
				WorkoutList = filteredItems;
				//OnPropertyChanged("WorkoutList");
			}
			else
			{
				WorkoutList = _allWorkOuts;
				//OnPropertyChanged("WorkoutList");
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
				OnPropertyChanged("Filters");
			}
			else
			{
				Filters.Remove(focusFilter);
				OnPropertyChanged("Filters");
			}
			ApplyFiltering();

		}


	}
}
