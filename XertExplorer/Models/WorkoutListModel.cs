using System.Collections.Generic;
using System.ComponentModel;
using XertClient;

namespace XertExplorer.Models
{
	public class WorkoutListModel : INotifyPropertyChanged
	{
		public List<XertWorkout> _WorkoutsList { get; private set; }
			//get
			//{
			//	return _WorkoutsList;
			//}
			//set 
			//{
				//_WorkoutsList = value;
				//_WorkoutsList = new List<XertWorkout>(value);
				//OnPropertyChanged("List");
			//} 
		//}

		/// <summary>
		/// Creates a new WorkoutListModel
		/// </summary>
		/// <param name="workouts"></param>
		public WorkoutListModel(List<XertWorkout> workouts)
		{
			//_WorkoutsList = workouts;
			_WorkoutsList = new List<XertWorkout>(workouts);
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
