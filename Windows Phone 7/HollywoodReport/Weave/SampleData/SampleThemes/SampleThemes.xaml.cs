﻿//      *********    DO NOT MODIFY THIS FILE     *********
//      This file is regenerated by a design tool. Making
//      changes to this file can cause errors.
namespace Expression.Blend.SampleData.SampleThemes
{
	using System; 

// To significantly reduce the sample data footprint in your production application, you can set
// the DISABLE_SAMPLE_DATA conditional compilation constant and disable sample data at runtime.
#if DISABLE_SAMPLE_DATA
	internal class SampleThemes { }
#else

	public class SampleThemes : System.ComponentModel.INotifyPropertyChanged
	{
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}

		public SampleThemes()
		{
			try
			{
				System.Uri resourceUri = new System.Uri("/weave;component/SampleData/SampleThemes/SampleThemes.xaml", System.UriKind.Relative);
				if (System.Windows.Application.GetResourceStream(resourceUri) != null)
				{
					System.Windows.Application.LoadComponent(this, resourceUri);
				}
			}
			catch (System.Exception)
			{
			}
		}

		private Themes _Themes = new Themes();

		public Themes Themes
		{
			get
			{
				return this._Themes;
			}
		}

		private Fonts _Fonts = new Fonts();

		public Fonts Fonts
		{
			get
			{
				return this._Fonts;
			}
		}

		private FontSizes _FontSizes = new FontSizes();

		public FontSizes FontSizes
		{
			get
			{
				return this._FontSizes;
			}
		}
	}

	public class Themes : System.Collections.ObjectModel.ObservableCollection<ThemesItem>
	{ 
	}

	public class ThemesItem : System.ComponentModel.INotifyPropertyChanged
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

		private string _Color = string.Empty;

		public string Color
		{
			get
			{
				return this._Color;
			}

			set
			{
				if (this._Color != value)
				{
					this._Color = value;
					this.OnPropertyChanged("Color");
				}
			}
		}
	}

	public class Fonts : System.Collections.ObjectModel.ObservableCollection<FontsItem>
	{ 
	}

	public class FontsItem : System.ComponentModel.INotifyPropertyChanged
	{
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}

		private string _FontName = string.Empty;

		public string FontName
		{
			get
			{
				return this._FontName;
			}

			set
			{
				if (this._FontName != value)
				{
					this._FontName = value;
					this.OnPropertyChanged("FontName");
				}
			}
		}

		private string _FontFamily = string.Empty;

		public string FontFamily
		{
			get
			{
				return this._FontFamily;
			}

			set
			{
				if (this._FontFamily != value)
				{
					this._FontFamily = value;
					this.OnPropertyChanged("FontFamily");
				}
			}
		}
	}

	public class FontSizes : System.Collections.ObjectModel.ObservableCollection<FontSizesItem>
	{ 
	}

	public class FontSizesItem : System.ComponentModel.INotifyPropertyChanged
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

		private double _Size = 0;

		public double Size
		{
			get
			{
				return this._Size;
			}

			set
			{
				if (this._Size != value)
				{
					this._Size = value;
					this.OnPropertyChanged("Size");
				}
			}
		}
	}
#endif
}
