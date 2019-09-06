namespace BenchmarkParallelPatterns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    public class QuickSort
    {
        public static void QuickSort_Sequential<T>(T[] items) where T : IComparable<T>
        {
            QuickSort_Sequential(items, 0, items.Length - 1);
        }

        private static void QuickSort_Sequential<T>(IList<T> arr, int left, int right) where T : IComparable<T>
        {
            if (right <= left) return;
            var pivot = Partition(arr, left, right);

            QuickSort_Sequential(arr, left, pivot - 1);
            QuickSort_Sequential(arr, pivot + 1, right);
        }

        public static void QuickSort_Parallel<T>(T[] items) where T : IComparable<T>
        {
            QuickSort_Parallel(items, 0, items.Length - 1);
        }

        private static void QuickSort_Parallel<T>(T[] items, int left, int right)
            where T : IComparable<T>
        {
            if (left >= right)
            {
                return;
            }

            SwapElements(items, left, (left + right) / 2);
            int pivot = left;
            for (int current = left + 1; current <= right; ++current)
            {
                if (items[current].CompareTo(items[left]) < 0)
                {
                    ++pivot;
                    SwapElements(items, pivot, current);
                }
            }

            SwapElements(items, left, pivot);
            Parallel.Invoke(
                () => QuickSort_Parallel(items, left, pivot - 1),
                () => QuickSort_Parallel(items, pivot + 1, right)
            );
        }

        // TODO : 1.4
        // (1) write a parallel and fast quick sort
        public static void QuickSort_Parallel_Threshold_TODO<T>(T[] items) where T : IComparable<T>
        {
        }

        #region Solution

        public static void QuickSort_Parallel_Threshold<T>(T[] items) where T : IComparable<T>
        {
            int maxDepth = (int) Math.Log(Environment.ProcessorCount, 2.0);
            QuickSort_Parallel_Threshold(items, 0, items.Length - 1, maxDepth);
        }

        private static void QuickSort_Parallel_Threshold<T>(T[] items, int left, int right, int depth)
            where T : IComparable<T>
        {
            if (left >= right)
            {
                return;
            }

            SwapElements(items, left, (left + right) / 2);
            int pivot = left;
            for (int current = left + 1; current <= right; ++current)
            {
                if (items[current].CompareTo(items[left]) < 0)
                {
                    ++pivot;
                    SwapElements(items, pivot, current);
                }
            }

            SwapElements(items, left, pivot);
            if (depth >= 0)

            {
                QuickSort_Parallel_Threshold(items, left, pivot - 1, depth);
                QuickSort_Parallel_Threshold(items, pivot + 1, right, depth);
            }
            else
            {
                --depth;
                Parallel.Invoke(
                    () => QuickSort_Parallel_Threshold(items, left, pivot - 1, depth),
                    () => QuickSort_Parallel_Threshold(items, pivot + 1, right, depth)
                );
            }
        }

        #endregion

        private static int Partition<T>(IList<T> arr, int low, int high) where T : IComparable<T>
        {
            var pivotPos = (high + low) / 2;
            var pivot = arr[pivotPos];
            Swap(arr, low, pivotPos);
            var left = low;
            for (var i = low + 1; i <= high; i++)
            {
                if (arr[i].CompareTo(pivot) >= 0) continue;
                left++;
                Swap(arr, i, left);
            }

            Swap(arr, low, left);
            return left;
        }

        private static void Swap<T>(IList<T> arr, int i, int j)
        {
            var tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }

        static void SwapElements<T>(T[] array2, int i, int j)
        {
            T temp = array2[i];
            array2[i] = array2[j];
            array2[j] = temp;
        }
    }
}