using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class DisposableContainer: IDisposable
    {
        public DisposableContainer()
        {
            Object = null;
            DisposeNeed = false;
        }

        public void Dispose()
        {
            if (Object != null && DisposeNeed)
                Object.Dispose();
        }

        public IDisposable Object { get; set; }
        public bool DisposeNeed { get; set; }
    }
}
