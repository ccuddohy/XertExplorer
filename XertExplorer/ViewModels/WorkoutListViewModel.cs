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
		public ICommand DemoModeCommand { get; set; }

		public bool LoggedOff {get; set;}
		public bool LoginError { get; set; }

		private List<XertWorkout> _allWorkOuts;

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

			LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
			LoggedOff = true;
		}


		public bool CanExecuteLogin(object parameter)
		{
			return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
		}

		public void ExecuteLogin(object parameter)
		{
			LogIn();
		}

		public async Task LogIn()
		{
			if (null == _client)
			{
				try
				{
					IXertClient _client = new Client();
					await _client.Login(Username, Password);

					LoggedOff = false;
					OnPropertyChanged("LoggedOff");
					LoginError = false;
					OnPropertyChanged("LoginError");
				}
				catch(Exception)
				{
					LoginError = true;
					OnPropertyChanged("LoginError");
				}
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
		private void OnPropertyChanged(string propertyname)
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
				OnPropertyChanged("WorkoutList");
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
				OnPropertyChanged("WorkoutList");
			}
			else
			{
				WorkoutList = _allWorkOuts;
				OnPropertyChanged("WorkoutList");
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
