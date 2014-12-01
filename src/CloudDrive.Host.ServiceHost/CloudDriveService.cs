using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using CloudDrive.Core;
using CloudDrive.Tracing;


namespace CloudDrive.Host.ServiceHost
{
    public partial class CloudDriveService : ServiceBase
    {
		SyncService _syncService;

		public void StartService()
		{
			var appHost = new ApplicationHost();

			var logTracer = appHost.AppContainer.Resolve<ITracer>();

			_syncService = appHost.AppContainer.Resolve<SyncService>();
			System.Threading.Tasks.Task.Run(() => _syncService.Start());
		}

        public CloudDriveService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
			try
			{
				StartService();
			}
			catch (Exception ex)
			{				
			}			
        }

        protected override void OnStop()
        {
			if (_syncService != null)
			{
				_syncService.Stop();
			}
        }
    }
}
