using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace FTAnalyzer.Utilities
{
    public class PropertyComparer<T> : IComparer<T>
    {
        readonly IComparer comparer;
        PropertyDescriptor propertyDescriptor;
        int reverse;

        public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
        {
            propertyDescriptor = property ?? throw new ArgumentNullException(nameof(property), "Property cannot be null");
            Type comparerForPropertyType = typeof(Comparer<>).MakeGenericType(property.PropertyType);
            comparer = (IComparer)comparerForPropertyType.InvokeMember("Default", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public, null, null, null);
            SetListSortDirection(direction);
        }

        #region IComparer<T> Members

        public int Compare(T x, T y)
        {
            var xValue = propertyDescriptor.GetValue(x);
            var yValue = propertyDescriptor.GetValue(y);
            string xString = xValue?.ToString();
            string yString = yValue?.ToString();
            if (string.IsNullOrEmpty(xString) && string.IsNullOrEmpty(yString))
                return 0;
            if (string.IsNullOrEmpty(xString))
                return reverse;
            if (string.IsNullOrEmpty(yString))
                return -1 * reverse;
            return reverse * comparer.Compare(xValue, yValue);
        }

        #endregion

        void SetPropertyDescriptor(PropertyDescriptor descriptor) => propertyDescriptor = descriptor;

        void SetListSortDirection(ListSortDirection direction) => reverse = direction == ListSortDirection.Ascending ? 1 : -1;

        public void SetPropertyAndDirection(PropertyDescriptor descriptor, ListSortDirection direction)
        {
            SetPropertyDescriptor(descriptor);
            SetListSortDirection(direction);
        }
    }
}