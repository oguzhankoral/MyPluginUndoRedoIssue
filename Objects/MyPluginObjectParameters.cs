using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPluginUndoRedoIssue.Objects
{
    public class MyPluginObjectParameters :System.ICloneable
    {
        public int NumberOfObjects { get; set; }
        public double Height { get; set; }
        public double Distance { get; set; }

        /// <summary>
        /// Calculated property
        /// It should always consistent whenever object updated
        /// </summary>
        public double TotalDistance => NumberOfObjects * Distance;

        /// <summary>
        /// Calculated property
        /// It should always consistent whenever object updated
        /// </summary>
        public double SecondHeight => Height * 0.9;

        public object Clone()
        {
            return new MyPluginObjectParameters()
            {
                NumberOfObjects = NumberOfObjects,
                Height = Height,
                Distance = Distance,
            };
        }
    }

    public static class DefaultObjects
    {
        public static MyPluginObjectParameters CreateDefault()
        {
            MyPluginObjectParameters parameters = new MyPluginObjectParameters()
            {
                NumberOfObjects = 4,
                Height = 3,
                Distance = 10
            };
            return parameters;
        }
    }
}
