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
				System.Uri resourceUri = new System.Uri("/Weave.UI.SettingsPages;component/SampleData/SampleCategories/SampleCategories.xaml", System.UriKind.Relative);
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

		private string _DisplayName = string.Empty;

		public string DisplayName
		{
			get
			{
				return this._DisplayName;
			}

			set
			{
				if (this._DisplayName != value)
				{
					this._DisplayName = value;
					this.OnPropertyChanged("DisplayName");
				}
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
	}

	public class Categories : System.Collections.ObjectModel.ObservableCollection<CategoriesItem>
	{ 
	}
#endif
}
