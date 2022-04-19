using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelLib
{
    /// <summary>
    /// <b>Specifies that the the method is exeuted in the application's UI thread.</b><br/><br/>
    /// UI form controls can be directly accessed within the annotated method, e.g. 
    /// <code>label.Text = "neat attribute";</code>
    /// You do <b>NOT</b> have to call into the UI controller's thread anymore by
    /// <code>label.Invoke(() => { label.Text = "neat attribute"; });</code>
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method)]
    public class UIThreadAccessible : System.Attribute
    {
        // attribute class, NOOP
    }
}
