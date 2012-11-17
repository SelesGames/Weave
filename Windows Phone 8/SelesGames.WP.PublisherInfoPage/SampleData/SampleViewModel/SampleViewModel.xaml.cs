﻿//      *********    DO NOT MODIFY THIS FILE     *********
//      This file is regenerated by a design tool. Making
//      changes to this file can cause errors.
namespace Expression.Blend.SampleData.SampleViewModel
{

    // To significantly reduce the sample data footprint in your production application, you can set
// the DISABLE_SAMPLE_DATA conditional compilation constant and disable sample data at runtime.
#if DISABLE_SAMPLE_DATA
	internal class SampleViewModel { }
#else

	public class SampleViewModel : System.ComponentModel.INotifyPropertyChanged
	{
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}

		public SampleViewModel()
		{
			try
			{
				System.Uri resourceUri = new System.Uri("/SelesGames.WP.PublisherInfoPage;component/SampleData/SampleViewModel/SampleViewModel.xaml", System.UriKind.Relative);
				if (System.Windows.Application.GetResourceStream(resourceUri) != null)
				{
					System.Windows.Application.LoadComponent(this, resourceUri);
				}
			}
			catch (System.Exception)
			{
			}
		}

		private Results _Results = new Results();

		public Results Results
		{
			get
			{
				return this._Results;
			}
		}
	}

	public class ResultsItem : System.ComponentModel.INotifyPropertyChanged
	{
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}

		private string _AppName = string.Empty;

		public string AppName
		{
			get
			{
				return this._AppName;
			}

			set
			{
				if (this._AppName != value)
				{
					this._AppName = value;
					this.OnPropertyChanged("AppName");
				}
			}
		}

		private string _Category = string.Empty;

		public string Category
		{
			get
			{
				return this._Category;
			}

			set
			{
				if (this._Category != value)
				{
					this._Category = value;
					this.OnPropertyChanged("Category");
				}
			}
		}

		private string _FormattedPrice = string.Empty;

		public string FormattedPrice
		{
			get
			{
				return this._FormattedPrice;
			}

			set
			{
				if (this._FormattedPrice != value)
				{
					this._FormattedPrice = value;
					this.OnPropertyChanged("FormattedPrice");
				}
			}
		}

		private string _RatingStars = string.Empty;

		public string RatingStars
		{
			get
			{
				return this._RatingStars;
			}

			set
			{
				if (this._RatingStars != value)
				{
					this._RatingStars = value;
					this.OnPropertyChanged("RatingStars");
				}
			}
		}
	}

	public class Results : System.Collections.ObjectModel.ObservableCollection<ResultsItem>
	{ 
	}
#endif
}
