// AForge Core Library
// AForge.NET framework
//
// Copyright © Andrew Kirillov, 2006-2008
// andrew.kirillov@gmail.com
//
using System.Runtime.Serialization;

namespace AForge
{
    using System;

    /// <summary>
    /// Represents an integer range with minimum and maximum values.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>The class represents an integer range with inclusive limits -
    /// both minimum and maximum values of the range are included into it.
    /// Mathematical notation of such range is <b>[min, max]</b>.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // create [1, 10] range
    /// IntStepRange range1 = new IntStepRange( 1, 10 );
    /// // create [5, 15] range
    /// IntStepRange range2 = new IntStepRange( 5, 15 );
    /// // check if values is inside of the first range
    /// if ( range1.IsInside( 7 ) )
    /// {
    ///     // ...
    /// }
    /// // check if the second range is inside of the first range
    /// if ( range1.IsInside( range2 ) )
    /// {
    ///     // ...
    /// }
    /// // check if two ranges overlap
    /// if ( range1.IsOverlapping( range2 ) )
    /// {
    ///     // ...
    /// }
    /// </code>
    /// </remarks>
    /// 
    [DataContract(Name = "IntStepRange", Namespace="")]
    public class IntStepRange
    {
        [DataMember(Name="min")]
        private int min;
        [DataMember(Name = "max")]
        private int max;
        [DataMember(Name = "step")]
        private int step;
        [DataMember(Name = "isExtract")]
        private int isExtract;
        [DataMember(Name = "isNonExport")]
        private int isNonExport;

        /// <summary>
        /// Minimum value of the range.
        /// </summary>
        /// 
        /// <remarks><para>The property represents minimum value (left side limit) or the range -
        /// [<b>min</b>, max].</para></remarks>
        /// 
        public int Min
        {
            get { return min; }
            set { min = value; }
        }

        /// <summary>
        /// Maximum value of the range.
        /// </summary>
        /// 
        /// <remarks><para>The property represents maximum value (right side limit) or the range -
        /// [min, <b>max</b>].</para></remarks>
        /// 
        public int Max
        {
            get { return max; }
            set { max = value; }
        }

        /// <summary>
        /// Length of the range (deffirence between maximum and minimum values).
        /// </summary>
        public int Length
        {
            get { return (max - min)/step + 1; }
        }

        public int Step
        {
            get
            {
                return step;
            }
        }

        public int IsExtract
        {
            get
            {
                return isExtract;
            }
        }
        public int IsNonExport
        {
            get
            {
                return isNonExport;
            }
            set
            {
                isNonExport = value;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="IntStepRange"/> class.
        /// </summary>
        /// 
        /// <param name="min">Minimum value of the range.</param>
        /// <param name="max">Maximum value of the range.</param>
        /// 
        public IntStepRange(int min, int max, int step)
        {
            this.min = min;
            this.max = max;
            this.step = step;
        }

        /// <summary>
        /// Check if the specified value is inside of the range.
        /// </summary>
        /// 
        /// <param name="x">Value to check.</param>
        /// 
        /// <returns><b>True</b> if the specified value is inside of the range or
        /// <b>false</b> otherwise.</returns>
        /// 
        public bool IsInside(int x)
        {
            return ((x >= min) && (x <= max));
        }

        /// <summary>
        /// Check if the specified range is inside of the range.
        /// </summary>
        /// 
        /// <param name="range">Range to check.</param>
        /// 
        /// <returns><b>True</b> if the specified range is inside of the range or
        /// <b>false</b> otherwise.</returns>
        /// 
        public bool IsInside(IntStepRange range)
        {
            return ((IsInside(range.min)) && (IsInside(range.max)));
        }

        /// <summary>
        /// Check if the specified range overlaps with the range.
        /// </summary>
        /// 
        /// <param name="range">Range to check for overlapping.</param>
        /// 
        /// <returns><b>True</b> if the specified range overlaps with the range or
        /// <b>false</b> otherwise.</returns>
        /// 
        public bool IsOverlapping(IntStepRange range)
        {
            return ((IsInside(range.min)) || (IsInside(range.max)));
        }

    }
}
