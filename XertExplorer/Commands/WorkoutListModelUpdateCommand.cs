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
	internal class WorkoutListModelUpdateCommand : ICommand
	{
		public WorkoutListModelUpdateCommand(WorkoutListViewModel viewModel)
		{
			ViewModel = viewModel;
		}

		private WorkoutListViewModel ViewModel;

		// we are implementing the interface directly, we are not deriving from concrete type (like routed command)
		// so need to connect back to the WPF command system.
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		// used for controls, for example enable or disable based on the return value of this method. 
		// Passing this functionally back to the ViewModel.
		public bool CanExecute(object parameter)
		{
			return ViewModel.CanUpdate;
		}

		public void Execute(object parameter)
		{
			ViewModel.SaveChanges();
		}
	}
}
