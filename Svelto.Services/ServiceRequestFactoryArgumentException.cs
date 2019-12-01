using System;

namespace Svelto.ServiceLayer
{
	public class ServiceRequestFactoryArgumentException: ArgumentException
	{
		public ServiceRequestFactoryArgumentException(string message):base(message)
		{}
	}
}