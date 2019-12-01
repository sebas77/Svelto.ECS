namespace Svelto.ServiceLayer
{
	public interface IServiceRequestsFactory
	{
		RequestInterface Create<RequestInterface>() where RequestInterface:class, IServiceRequest;
	}
}

