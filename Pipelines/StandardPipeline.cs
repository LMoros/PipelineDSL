using System;
using System.Threading.Tasks;
using PipelineDSL;

namespace Pipelines
{
    public class StandardPipeline<T,R>
    {
        #region Contructor and Private Members
        private readonly ILog<string, string> _Logging;
        private readonly Func<T, IAmAuthenticated<T>> _Authenticate;
        private readonly Func<IAmAuthenticated<T>,
                                IAmAuthorized<
                                    IAmAuthenticated<T>>> _Authorize;
        private readonly Func<IAmAuthorized<IAmAuthenticated<T>>,
                                IAmProcessed<
                                    IAmAuthorized<
                                        IAmAuthenticated<T>>>> _Process;

        private readonly ITranslateToWebApi<
                            IAmProcessed<
                                IAmAuthorized<
                                    IAmAuthenticated<T>>>, R> _AdaptOutput;

        public StandardPipeline(
            ILog<string, string> logging,
            IAuthenticate<T> authentication,
            IAuthorize<T> authorization,
            IProcess<T> process,
            ITranslateToWebApi<IAmProcessed<IAmAuthorized<IAmAuthenticated<T>>>, R> adapter) 
        {
            _Logging = logging;
            _Authenticate = authentication.Authenticate;
            _Authorize = authorization.Authorize;
            _Process = process.Process;
            _AdaptOutput = adapter;
        }
        #endregion

        public Task<R> Execute(T intension)
        {
            return SetPipelineAs
                    .With( intension)
                    .Asynchrosnully(_Authenticate.Including( Logging))
                    .AndThen(_Authorize.Including( Logging).Retrying( up_to: 2.Times()))
                    .AndThen(_Process.Including( Logging).Retrying( up_to: 5.Times()))
                    .AtLast(_AdaptOutput);
        }

        #region Logging overloads
        private IAmAuthenticated<T> Logging( IAmAuthenticated<T> authenticatedTransfer)
        {
            _Logging.Do("Intension was authenticated");
            return authenticatedTransfer;
        }

        private IAmAuthorized<IAmAuthenticated<T>> Logging(IAmAuthorized<IAmAuthenticated<T>> authenticatedTransfer)
        {
            _Logging.Do("Intension was authorized");
            return authenticatedTransfer;
        }
        private IAmProcessed<IAmAuthorized<IAmAuthenticated<T>>> Logging(IAmProcessed<IAmAuthorized<IAmAuthenticated<T>>> authenticatedTransfer)
        {
            _Logging.Do("Intension was processed");
            return authenticatedTransfer;
        }
        #endregion
    }
}
