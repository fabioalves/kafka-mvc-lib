using System;
using System.Collections.Generic;
using System.Text;

namespace TvOpenPlatform.Consumer.Result.Options
{
    public class BusinessErrorOptions
    {
        public BusinessErrorOptions(string actionName, string controllerName)
        {
            ActionName = actionName;
            ControllerName = controllerName;
        }

        public string ActionName { get; }
        public string ControllerName { get; }
    }
}
