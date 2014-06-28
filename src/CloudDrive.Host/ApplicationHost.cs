using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace CloudDrive.Host
{
    public class ApplicationHost
    {
        public IContainer AppContainer { get; protected set; }

        public ApplicationHost()
        {
            AppContainer = new ApplicationBuilder().BuildApplication();

            Core.CoreApp.TraceWriter = AppContainer.Resolve<CloudDrive.Tracing.TraceWriter>();
        }
    }
}
