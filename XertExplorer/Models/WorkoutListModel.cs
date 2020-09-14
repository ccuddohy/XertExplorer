using System.Collections.Generic;
using System.ComponentModel;
using XertClient;

namespace XertExplorer.Models
{
	/// <summary>
	/// The model has a list of XertWorkout. The application will not edit the elements of the list
	/// other than filter the list or sort the list for viewing. We do not need to be concerned with the 
	/// details of the XertWorkout object other than exposing them for displaying.
	/// </summary>
	public class WorkoutListModel : INotifyPropertyChanged
	{
		public List<XertWorkout> WorkoutsList { get; private set; }

		/// <summary>
		/// Creates a new WorkoutListModel
		/// </summary>
		/// <param name="workouts"></param>
		public WorkoutListModel(List<XertWorkout> workouts)
		{
			WorkoutsList = new List<XertWorkout>(workouts);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if(null != handler )
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
