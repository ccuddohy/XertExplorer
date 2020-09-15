using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using XertClient;
using XertExplorer.Models;
using XertExplorer.Commands;

namespace XertExplorer.ViewModels
{
	internal class WorkoutListViewModel
	{
		public WorkoutListViewModel()
		{
			//LoadWorkouts();
			string JSONtxt = File.ReadAllText("workouts.json");
			List<XertWorkout> wkouts = JsonSerializer.Deserialize<List<XertWorkout>>(JSONtxt);
			_WorkoutListMdl = new WorkoutListModel(wkouts);
			UpdateCommand = new UpdateWorkoutListModelCommand(this);
		}
				
		/// <summary>
		/// If the workoutList model is not null and is not empty then returns true.
		/// </summary>
		public bool CanUpdate
		{
			get { 
				if(null == WorkoutListMdl)
				{
					return false;
				}
				if(0 == WorkoutListMdl.WorkoutsList.Count)
				{
					return false;
				}
				return true;
			}
		}

		public WorkoutListModel WorkoutListMdl {
			get {
				return _WorkoutListMdl;
			}
		}

		WorkoutListModel _WorkoutListMdl;

		/// <summary>
		/// Gets the UpdateCommand for the _ViewModel
		/// </summary>
		public ICommand UpdateCommand { 
			get; 
			private set; 
		}

		/// <summary>
		/// Saves changes to the WorkoutListModel instance
		/// </summary>
		public void SaveChanges()
		{
			Debug.Assert(false, string.Format("was updated"));
		}

	}
}
