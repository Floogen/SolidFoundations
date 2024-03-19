using System;

namespace SolidFoundations.Framework.Interfaces
{
    public interface ISaveAnywhereApi
    {
        event EventHandler BeforeSave;
        event EventHandler AfterSave;
        event EventHandler AfterLoad;
    }
}
