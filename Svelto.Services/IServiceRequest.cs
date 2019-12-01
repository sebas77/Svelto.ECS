using System.Collections.Generic;
using Svelto.Tasks;

namespace Svelto.ServiceLayer
{
	public interface IServiceRequest
	{
		IEnumerator<TaskContract> Execute();
	}
	
	public interface IServiceRequest<in TDependency>: IServiceRequest
	{
		IServiceRequest Inject(TDependency registerData);
	}
}
