using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VeeamZipper
{
    interface IZipProcessor
    {
        bool perform(String source, String destination);
    }
}
