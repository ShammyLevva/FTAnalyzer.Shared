using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FTAnalyzer.Utilities
{
    public class SortableBindingList<T> : BindingList<T>
    {
        readonly Dictionary<Type, PropertyComparer<T>> comparers;
        bool isSorted;
        ListSortDirection listSortDirection;
        PropertyDescriptor propertyDescriptor;

        public SortableBindingList()
            : base(new List<T>()) => comparers = new Dictionary<Type, PropertyComparer<T>>();

        public SortableBindingList(IEnumerable<T> enumeration)
            : base(new List<T>(enumeration)) => comparers = new Dictionary<Type, PropertyComparer<T>>();

        protected override bool SupportsSortingCore => true;

        protected override bool IsSortedCore => isSorted;

        protected override PropertyDescriptor SortPropertyCore => propertyDescriptor;

        protected override ListSortDirection SortDirectionCore => listSortDirection;

        protected override bool SupportsSearchingCore => true;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            OnSortStarted();
            List<T> itemsList = (List<T>)Items;

            Type propertyType = prop.PropertyType;
            if (!comparers.TryGetValue(propertyType, out PropertyComparer<T> comparer))
            {
                comparer = new PropertyComparer<T>(prop, direction);
                comparers.Add(propertyType, comparer);
            }

            comparer.SetPropertyAndDirection(prop, direction);
            MergeSort(itemsList, comparer);

           propertyDescriptor = prop;
           listSortDirection = direction;
           isSorted = true;

           OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
           OnSortFinished();
        }

        protected override void RemoveSortCore()
        {
            isSorted = false;
            propertyDescriptor = base.SortPropertyCore;
            listSortDirection = base.SortDirectionCore;

            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override int FindCore(PropertyDescriptor prop, object key)
        {
            int count = Count;
            for (int i = 0; i < count; ++i)
            {
                T element = this[i];
                if (prop.GetValue(element).Equals(key))
                    return i;
            }

            return -1;
        }

        void MergeSort(List<T> inputList, PropertyComparer<T> comparer)
        {
            int left = 0;
            int right = inputList.Count - 1;
            InternalMergeSort(inputList, comparer, left, right);
        }

        void InternalMergeSort(List<T> inputList, PropertyComparer<T> comparer, int left, int right)
        {
            int mid = 0;

            if (left < right)
            {
                mid = (left + right) / 2;
                InternalMergeSort(inputList, comparer, left, mid);
                InternalMergeSort(inputList, comparer, (mid + 1), right);
                MergeSortedList(inputList, comparer, left, mid, right);
            }
        }

        void MergeSortedList(List<T> inputList, PropertyComparer<T> comparer, int left, int mid, int right)
        {
            int total_elements = right - left + 1; //BODMAS rule
            int right_start = mid + 1;
            int temp_location = left;
            List<T> tempList = new List<T>();

            while ((left <= mid) && right_start <= right)
            {
                if (comparer.Compare(inputList[left], inputList[right_start]) <= 0)
                    tempList.Add(inputList[left++]);
                else
                    tempList.Add(inputList[right_start++]);
            }

            if (left > mid)
            {
                for (int j = right_start; j <= right; j++)
                    tempList.Add(inputList[right_start++]);
            }
            else
            {
                for (int j = left; j <= mid; j++)
                    tempList.Add(inputList[left++]);
            }

            //Array.Copy(tempArray, 0, inputArray, temp_location, total_elements); // just another way of accomplishing things (in-built copy)
            for (int i = 0, j = temp_location; i < total_elements; i++, j++)
            {
                inputList[j] = tempList[i];
            }
        }
        #region EventHandler
        public event EventHandler SortStarted;
        public void OnSortStarted()
        {
            SortStarted?.Invoke(null, EventArgs.Empty);
        }

        public event EventHandler SortFinished;
        public void OnSortFinished()
        {
            SortFinished?.Invoke(null, EventArgs.Empty);
        }
        #endregion

    }
}