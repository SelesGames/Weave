using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Linq;
using System.Text;
using SelesGames;

namespace weave
{
    public partial class AddNewCategoryPage : PhoneApplicationPage
    {
        public AddNewCategoryPage()
        {
            InitializeComponent();
            ApplicationTitle.Text = AppSettings.AppName.ToUpper();
        }

        void saveButton_Click(object sender, EventArgs e)
        {
            string categoryName = this.categoryName.Text;
            if (string.IsNullOrEmpty(categoryName) || categoryName.Length < 2)
            {
                MessageBox.Show("Please enter a Category name that is 2 letters or longer");
                return;
            }

            var stringBuilder = categoryName.Split(' ')
                .Select(word => word.Trim())
                .Select(word => CapitalizeFirstLetter(word))
                .Aggregate(new StringBuilder(), (sb, word) => sb.Append(word + " "));

            categoryName = stringBuilder.Remove(stringBuilder.Length - 1, 1).ToString();               

            var newCategory = new Category
            {
                Name = categoryName,
                IsEnabled = true,
                UserAdded = true,
            };

            var dal = ServiceResolver.Get<Data.DataAccessLayer>();
            dal.AddCustomCategory(newCategory);
            NavigationService.GoBack();
        }

        string CapitalizeFirstLetter(string word)
        {
            if (string.IsNullOrEmpty(word) || word.Length < 2)
                return word;

            return word.Substring(0, 1).ToUpper()
                + word.Substring(1, word.Length - 1);
        }

        //void cancelButton_Click(object sender, EventArgs e)
        //{
        //        NavigationService.GoBack();
        //}
    }
}