using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using XertExplorer.ViewModels;

namespace XertExplorer.Commands
{
	/// <summary>
	/// 
	/// </summary>
	internal class UpdateWorkoutListModelCommand : ICommand
	{
		public UpdateWorkoutListModelCommand(WorkoutListViewModel viewModel)
		{
			_ViewModel = viewModel;
		}

		private WorkoutListViewModel _ViewModel;

		// we are implementing the interface directly, we are not deriving from concrete type (like routed command)
		// so need to connect back to the WPF command system. 
		public event System.EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}

		// used for controls, for example enable or disable based on the return value of this method. 
		// Passing this functionally back to the _ViewModel.
		public bool CanExecute(object parameter)
		{
			return _ViewModel.CanUpdate;
		}

		public void Execute(object parameter)
		{
			_ViewModel.SaveChanges();
		}
	}
}
