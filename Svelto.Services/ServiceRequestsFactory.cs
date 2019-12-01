using System;
using System.Collections.Generic;

namespace Svelto.ServiceLayer
{
    public abstract class ServiceRequestsFactory : IServiceRequestsFactory
    {
        public RequestInterface Create<RequestInterface>() where RequestInterface : class, IServiceRequest
        {
            var ret = RetrieveObjectType<RequestInterface>();

            return ret.CreateInstance() as RequestInterface;
        }

        protected void AddRelation<RequestInterface, RequestClass>() where RequestClass : class, RequestInterface, new()
            where RequestInterface : IServiceRequest

        {
            _requestMap[typeof(RequestInterface)] = new Value<RequestClass>();
        }

        IHoldValue RetrieveObjectType<RequestInterface>()
        {
            if (_requestMap.ContainsKey(typeof(RequestInterface)) == false)
                throw new ServiceRequestFactoryArgumentException("Request not registered");

            var ret = _requestMap[typeof(RequestInterface)];

            if (ret == null)
                throw new ServiceRequestFactoryArgumentException("Request not found");

            return ret;
        }

        readonly Dictionary<Type, IHoldValue> _requestMap = new Dictionary<Type, IHoldValue>();

        interface IHoldValue
        {
            IServiceRequest CreateInstance();
        }

        class Value<RequestClass> : IHoldValue where RequestClass : class, IServiceRequest, new()
        {
            public IServiceRequest CreateInstance()
            {
                return new RequestClass();
            }
        }
    }
}
