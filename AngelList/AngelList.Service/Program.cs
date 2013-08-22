using Atlas;
using Autofac;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Service
{
	class Program
	{
		static void Main(string[] args)
		{
			var serviceName = ConfigurationManager.AppSettings["ServiceName"] ?? "AngelList.Service";

			var configuration = Host.Configure<Service>()
				   .Named(serviceName, serviceName, serviceName)
				   .AllowMultipleInstances()
				   .WithRegistrations(
					   delegate(ContainerBuilder b)
					   {
						   b.Register(c => new Service()).As<IAmAHostedProcess>();
					   })
				   .WithArguments(args);
			Host.Start(configuration);
		}
	}
}
