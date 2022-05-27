using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Interfaces
{
    public interface ISaveAnywhereApi
    {
        event EventHandler BeforeSave;
        event EventHandler AfterSave;
        event EventHandler AfterLoad;
    }
}
