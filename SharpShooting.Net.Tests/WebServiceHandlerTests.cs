using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SharpShooting.Net.Tests
{
    [TestClass]
    public class WebServiceHandlerTests
    {
        private readonly Mock<IChannelFactory<IWebServiceContract>> _channelFactoryMock = new Mock<IChannelFactory<IWebServiceContract>>();
        private readonly Mock<WebServiceProxyDummy> _webServiceProxyMock = new Mock<WebServiceProxyDummy>();
        private readonly Mock<WebServiceHandler<IWebServiceContract>.IOperationContextScopeFactory> _operationContextScopeFactoryMock = new Mock<WebServiceHandler<IWebServiceContract>.IOperationContextScopeFactory>();
        private readonly Action<IWebServiceContract> _actionDummy = it => { };

        public interface IWebServiceContract
        {
            string SomeMethodThatTakesAndReturnsString(string argument);
        }

        public class WebServiceProxyDummy : IWebServiceContract, IContextChannel
        {
            public virtual string SomeMethodThatTakesAndReturnsString(string argument)
            {
                throw new NotImplementedException();
            }

            public virtual void Abort()
            {
                throw new NotImplementedException();
            }

            public virtual void Close()
            {
                throw new NotImplementedException();
            }

            public virtual void Close(TimeSpan timeout)
            {
                throw new NotImplementedException();
            }

            public virtual IAsyncResult BeginClose(AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public virtual IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public virtual void EndClose(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public virtual void Open()
            {
                throw new NotImplementedException();
            }

            public virtual void Open(TimeSpan timeout)
            {
                throw new NotImplementedException();
            }

            public virtual IAsyncResult BeginOpen(AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public virtual IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public virtual void EndOpen(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public virtual CommunicationState State
            {
                get { throw new NotImplementedException(); }
            }

            public virtual event EventHandler Closed;
            public virtual event EventHandler Closing;
            public virtual event EventHandler Faulted;
            public virtual event EventHandler Opened;
            public virtual event EventHandler Opening;
            public virtual T GetProperty<T>() where T : class
            {
                throw new NotImplementedException();
            }

            public virtual IExtensionCollection<IContextChannel> Extensions
            {
                get { throw new NotImplementedException(); }
            }

            public virtual bool AllowOutputBatching
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public virtual IInputSession InputSession
            {
                get { throw new NotImplementedException(); }
            }

            public virtual EndpointAddress LocalAddress
            {
                get { throw new NotImplementedException(); }
            }

            public virtual TimeSpan OperationTimeout
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public virtual IOutputSession OutputSession
            {
                get { throw new NotImplementedException(); }
            }

            public virtual EndpointAddress RemoteAddress
            {
                get { throw new NotImplementedException(); }
            }

            public virtual string SessionId
            {
                get { throw new NotImplementedException(); }
            }
        }

        private class SomeException : Exception { }

        [TestInitialize]
        public void TestInitialize()
        {
            _channelFactoryMock.Setup(it => it.CreateChannel(It.IsAny<EndpointAddress>())).Returns(_webServiceProxyMock.Object);

            var operationContextScopeFake = new OperationContextScope(OperationContext.Current);
            _operationContextScopeFactoryMock.Setup(it => it.CreateOperationScope(It.IsAny<IContextChannel>())).Returns(operationContextScopeFake);
        }

        [TestMethod]
        public void ShouldCallCreateChannelMethod()
        {
            WebServiceHandler<IWebServiceContract>.Invoke(_operationContextScopeFactoryMock.Object, _channelFactoryMock.Object, It.IsAny<EndpointAddress>(), _actionDummy);

            _channelFactoryMock.Verify(it => it.CreateChannel(It.IsAny<EndpointAddress>()), Times.Once());
        }

        [TestMethod]
        public void ShouldCallCreateOperationContextScopeMethod()
        {
            WebServiceHandler<IWebServiceContract>.Invoke(_operationContextScopeFactoryMock.Object, _channelFactoryMock.Object, It.IsAny<EndpointAddress>(), _actionDummy);

            _operationContextScopeFactoryMock.VerifyAll();
        }

        [TestMethod]
        public void ShouldCallAbortMethodOnCommunicationObjectIfStateIsFaulted()
        {
            _webServiceProxyMock.SetupGet(it => it.State).Returns(CommunicationState.Faulted);

            WebServiceHandler<IWebServiceContract>.Invoke(_operationContextScopeFactoryMock.Object, _channelFactoryMock.Object, It.IsAny<EndpointAddress>(), _actionDummy);

            _webServiceProxyMock.Verify(it => it.Abort(), Times.Once());
        }

        [TestMethod]
        public void ShouldCallCloseMethodOnCommunicationObjectIfStateIsNotFaulted()
        {
            const CommunicationState someArbitraryCommunicationStateDifferentFromFaulted = CommunicationState.Closed;

            _webServiceProxyMock.SetupGet(it => it.State).Returns(someArbitraryCommunicationStateDifferentFromFaulted);

            WebServiceHandler<IWebServiceContract>.Invoke(_operationContextScopeFactoryMock.Object, _channelFactoryMock.Object, It.IsAny<EndpointAddress>(), _actionDummy);

            _webServiceProxyMock.Verify(it => it.Close(), Times.Once());
        }

        [TestMethod]
        public void ShouldCallAbortMethodOnCommunicationObjectIfStateIsNotFaultedButCloseMethodThrowsCommunicationException()
        {
            const CommunicationState someArbitraryCommunicationStateDifferentFromFaulted = CommunicationState.Closed;

            _webServiceProxyMock.SetupGet(it => it.State).Returns(someArbitraryCommunicationStateDifferentFromFaulted);
            _webServiceProxyMock.Setup(it => it.Close()).Throws(new CommunicationException());

            WebServiceHandler<IWebServiceContract>.Invoke(_operationContextScopeFactoryMock.Object, _channelFactoryMock.Object, It.IsAny<EndpointAddress>(), _actionDummy);

            _webServiceProxyMock.Verify(it => it.Close(), Times.Once());
            _webServiceProxyMock.Verify(it => it.Abort(), Times.Once());
        }

        [TestMethod]
        public void ShouldCallAbortMethodOnCommunicationObjectIfStateIsNotFaultedButCloseMethodThrowsTimeoutException()
        {
            const CommunicationState someArbitraryCommunicationStateDifferentFromFaulted = CommunicationState.Closed;

            _webServiceProxyMock.SetupGet(it => it.State).Returns(someArbitraryCommunicationStateDifferentFromFaulted);
            _webServiceProxyMock.Setup(it => it.Close()).Throws(new TimeoutException());

            WebServiceHandler<IWebServiceContract>.Invoke(_operationContextScopeFactoryMock.Object, _channelFactoryMock.Object, It.IsAny<EndpointAddress>(), _actionDummy);

            _webServiceProxyMock.Verify(it => it.Close(), Times.Once());
            _webServiceProxyMock.Verify(it => it.Abort(), Times.Once());
        }

        [TestMethod]
        public void ShouldCallAbortMethodOnCommunicationObjectAndRethowExceptionIfStateIsNotFaultedButCloseMethodThrowsException()
        {
            const CommunicationState someArbitraryCommunicationStateDifferentFromFaulted = CommunicationState.Closed;
            var exceptionToBeThrown = new SomeException();

            _webServiceProxyMock.SetupGet(it => it.State).Returns(someArbitraryCommunicationStateDifferentFromFaulted);
            _webServiceProxyMock.Setup(it => it.Close()).Throws(exceptionToBeThrown);

            try
            {
                WebServiceHandler<IWebServiceContract>.Invoke(_operationContextScopeFactoryMock.Object, _channelFactoryMock.Object, It.IsAny<EndpointAddress>(), _actionDummy);
                Assert.Fail("Should have thrown an exception.");
            }
            catch (SomeException someException)
            {
                Assert.AreSame(exceptionToBeThrown, someException, "Exception raised on Close method wasn't re-thrown.");
            }

            _webServiceProxyMock.Verify(it => it.Close(), Times.Once());
            _webServiceProxyMock.Verify(it => it.Abort(), Times.Once());
        }

        [TestMethod]
        public void ShouldExecuteAction()
        {
            var actionsExecuted = 0;
            Action<IWebServiceContract> actionSpy = it => { actionsExecuted++; };

            WebServiceHandler<IWebServiceContract>.Invoke(
                _operationContextScopeFactoryMock.Object,
                _channelFactoryMock.Object,
                It.IsAny<EndpointAddress>(),
                actionSpy,
                actionSpy);

            Assert.AreEqual(2, actionsExecuted);
        }
    }
}
