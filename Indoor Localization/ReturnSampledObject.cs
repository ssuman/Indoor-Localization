using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Indoor_Localization
{
    class ReturnSampledObject
    {
        public Vector l;
        public Vector3D p;
        public int index;
        public bool valid;


        public ReturnSampledObject(Vector l, Vector3D p, int index, bool valid)
        {
            this.l = l;
            this.p = p;
            this.index = index;
            this.valid = valid;
        }

    }
}
