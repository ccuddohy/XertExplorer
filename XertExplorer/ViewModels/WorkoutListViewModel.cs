using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using XertClient;
using XertExplorer.Models;
using XertExplorer.Commands;
using System.Reflection.Metadata.Ecma335;

namespace XertExplorer.ViewModels
{
	internal class WorkoutListViewModel
	{
		/// <summary>
		/// for development, will load workouts from file. The file is copied to bin on build as set in the VS properties Build Action
		/// </summary>
		/// <param name="path"></param>
		public WorkoutListViewModel()
		{
			string JSONtxt = File.ReadAllText("workouts.json");
			List<XertWorkout> wkouts = JsonSerializer.Deserialize<List<XertWorkout>>(JSONtxt);
			WorkoutListMdl = new WorkoutListModel(wkouts);
			UpdateCommand = new WorkoutListModelUpdateCommand(this);
		}

		/// <summary>
		/// Indicates if the model can be updated
		/// </summary>
		public bool CanUpdate
		{
			get { 
				if(null == WorkoutListMdl)
				{
					return false;
				}
				if(WorkoutListMdl.WorkoutsList.Count > 0)
				{
					return true;
				}
				return false;
			}
		}

		public WorkoutListModel WorkoutListMdl { get; private set; }

		/// <summary>
		/// Gets the UpdateCommand for the ViewModel
		/// </summary>
		public ICommand UpdateCommand { get; private set; }

		/// <summary>
		/// for experimenting. We do not want to save any changes to the list back to the source.
		/// </summary>
		public void SaveChanges()
		{
			Debug.Assert(false, string.Format("was updated"));
		}

	}
}
