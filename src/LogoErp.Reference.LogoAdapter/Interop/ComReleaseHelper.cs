using System;
using System.Runtime.InteropServices;

namespace LogoErp.Reference.LogoAdapter.Interop
{
    public static class ComReleaseHelper
    {
        public static void Release(object comObject)
        {
            if (comObject == null)
                return;

            if (!Marshal.IsComObject(comObject))
                return;

            try
            {
                Marshal.FinalReleaseComObject(comObject);
            }
            catch (InvalidComObjectException)
            {
                // Object was already released. Nothing else to do.
            }
        }

        public static void ReleaseAndClear<T>(ref T comObject) where T : class
        {
            var instance = comObject;
            comObject = null;
            Release(instance);
        }
    }
}
