using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using XertClient;
using XertExplorer.Models;


namespace XertExplorer.ViewModels
{
	internal class WorkoutListViewModel
	{
		public WorkoutListViewModel(string path)
		{
			string JSONtxt = File.ReadAllText(path);
			List<XertWorkout> wkouts = JsonSerializer.Deserialize<List<XertWorkout>>(JSONtxt);
			WorkoutListMdl = new WorkoutListModel(wkouts);
		}

		public WorkoutListModel WorkoutListMdl { get; private set; }

		/// <summary>
		/// for development, will load workouts from file. The file is copied to bin on build as set in the VS properties Build Action
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		//public WorkoutListModel GetWorkoutListMdl(string path)
		//{
		//	string JSONtxt = File.ReadAllText(path);
		//	WorkoutListMdl = new WorkoutListModel(JsonSerializer.Deserialize<List<XertWorkout>>(JSONtxt));
		//	return WorkoutListMdl;
		//}

		/// <summary>
		/// for experimenting. We do not want to save any changes to the list back to the source.
		/// </summary>
		public void SaveChanges()
		{
			Debug.Assert(false, string.Format("was updated"));
		}

	}
}
