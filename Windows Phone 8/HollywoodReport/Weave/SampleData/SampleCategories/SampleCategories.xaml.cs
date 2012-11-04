﻿//      *********    DO NOT MODIFY THIS FILE     *********
//      This file is regenerated by a design tool. Making
//      changes to this file can cause errors.
namespace Expression.Blend.SampleData.SampleCategories
{
	using System; 

// To significantly reduce the sample data footprint in your production application, you can set
// the DISABLE_SAMPLE_DATA conditional compilation constant and disable sample data at runtime.
#if DISABLE_SAMPLE_DATA
	internal class SampleCategories { }
#else

	public class SampleCategories : System.ComponentModel.INotifyPropertyChanged
	{
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}

		public SampleCategories()
		{
			try
			{
				System.Uri resourceUri = new System.Uri("/weave;component/SampleData/SampleCategories/SampleCategories.xaml", System.UriKind.Relative);
				if (System.Windows.Application.GetResourceStream(resourceUri) != null)
				{
					System.Windows.Application.LoadComponent(this, resourceUri);
				}
			}
			catch (System.Exception)
			{
			}
		}

		private Categories _Categories = new Categories();

		public Categories Categories
		{
			get
			{
				return this._Categories;
			}
		}
	}

	public class Categories : System.Collections.ObjectModel.ObservableCollection<CategoriesItem>
	{ 
	}

	public class CategoriesItem : System.ComponentModel.INotifyPropertyChanged
	{
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}

		private string _Name = string.Empty;

		public string Name
		{
			get
			{
				return this._Name;
			}

			set
			{
				if (this._Name != value)
				{
					this._Name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}

		private bool _IsEnabled = false;

		public bool IsEnabled
		{
			get
			{
				return this._IsEnabled;
			}

			set
			{
				if (this._IsEnabled != value)
				{
					this._IsEnabled = value;
					this.OnPropertyChanged("IsEnabled");
				}
			}
		}

		private bool _UserAdded = false;

		public bool UserAdded
		{
			get
			{
				return this._UserAdded;
			}

			set
			{
				if (this._UserAdded != value)
				{
					this._UserAdded = value;
					this.OnPropertyChanged("UserAdded");
				}
			}
		}
	}
#endif
}
