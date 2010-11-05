using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace SharpShooting.Net
{
    public static class WebServiceHandler<TWebServiceContract>
    {
        public interface IOperationContextScopeFactory
        {
            OperationContextScope CreateOperationScope(IContextChannel contextChannel);
        }

        internal class OperationContextScopeFactory : IOperationContextScopeFactory
        {
            public OperationContextScope CreateOperationScope(IContextChannel contextChannel)
            {
                return new OperationContextScope(contextChannel);
            }
        }

        public static void Invoke(IOperationContextScopeFactory operationContextScopeFactory, IChannelFactory<TWebServiceContract> channelFactory, EndpointAddress endpointAddress, params Action<TWebServiceContract>[] actions)
        {
            TWebServiceContract webServiceProxy = default(TWebServiceContract);

            try
            {
                webServiceProxy = channelFactory.CreateChannel(endpointAddress);

                using (operationContextScopeFactory.CreateOperationScope((IContextChannel)webServiceProxy))
                {
                    foreach (var action in actions)
                        action(webServiceProxy);
                }
            }
                //catch (Exception)
                //{
                //    // TODO: any exception caught here will be supressed if ICommunicationObject.Close() throws a System.Exception; consider logging or changing this implementation.
                //}
            finally
            {
                ICommunicationObject webServiceProxyAsCommunicationObject = null;

                try
                {
                    webServiceProxyAsCommunicationObject = (ICommunicationObject)webServiceProxy;

                    if (webServiceProxyAsCommunicationObject.State != CommunicationState.Faulted)
                        webServiceProxyAsCommunicationObject.Close();
                    else
                        webServiceProxyAsCommunicationObject.Abort();
                }
                catch (CommunicationException)
                {
                    webServiceProxyAsCommunicationObject.Abort();
                }
                catch (TimeoutException)
                {
                    webServiceProxyAsCommunicationObject.Abort();
                }
                catch (Exception)
                {
                    webServiceProxyAsCommunicationObject.Abort();
                    throw;
                }
                finally
                {
                    webServiceProxyAsCommunicationObject = null;
                }
            }
        }
    }
}