
namespace System.Windows.Data
{
    public class shim
    {
        internal FrameworkElement element;
        internal DependencyProperty property;
    }

    public static class BindingHelperExtensions
    {
        public static shim Bind(this FrameworkElement element, DependencyProperty property)
        {
            var temp = new shim();
            temp.element = element;
            temp.property = property;
            return temp;
        }

        public static BindingExpressionBase To(this shim shim, object source, string path)
        {
            return shim.element.SetBinding(shim.property, new Binding(path) { Source = source });
        }
    }
}
