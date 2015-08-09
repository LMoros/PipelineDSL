using System;
using System.Threading.Tasks;
using PipelineDSL;

namespace Pipelines
{
    public class StandardPipeline<T,R>
    {
        #region Contructor and Private Members
        private readonly ILog<string, string> _Logging;
        private readonly IAuthenticate<T> _Authentication;
        private readonly IAuthorize<T> _Authorization;
        private readonly IProcess<T> _Process;
        private readonly ITranslateToWebApi<IAmProcessed<IAmAuthorized<IAmAuthenticated<T>>>, R> _AdaptOutput;

        public StandardPipeline(
            ILog<string, string> logging,
            IAuthenticate<T> authentication,
            IAuthorize<T> authorization,
            IProcess<T> process,
            ITranslateToWebApi<IAmProcessed<IAmAuthorized<IAmAuthenticated<T>>>, R> adapter) 
        {
            _Logging = logging;
            _Authentication = authentication;
            _Authorization = authorization;
            _Process = process;
            _AdaptOutput = adapter;
        }
        #endregion

        public Task<R> Execute(T intension)
        {
            #region definning local variables
            Func<T, IAmAuthenticated<T>> authenticate = _Authentication.Authenticate;
            Func<IAmAuthenticated<T>, 
                 IAmAuthorized<IAmAuthenticated<T>>> authorize = _Authorization.Authorize;
            Func<IAmAuthorized<IAmAuthenticated<T>>,
                 IAmProcessed<IAmAuthorized<IAmAuthenticated<T>>>> process = _Process.Process;
            var adaptOuput = _AdaptOutput;
            #endregion

            return SetPipelineAs
                    .With( intension)
                    .Asynchrosnully( authenticate.Including( Logging))
                    .AndThen( authorize.Including( Logging).Retrying( up_to: 2.Times()))
                    .AndThen( process.Including( Logging).Retrying( up_to: 5.Times()))
                    .AtLast( adaptOuput);
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
