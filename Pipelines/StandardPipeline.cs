using System;
using System.Threading.Tasks;
using PipelineDSL;

namespace Pipelines
{
    public class StandardPipeline<TIntention,TResult,TOuput>
    {
        #region Contructor and Private Members
        private readonly ILog _Logging;

        private readonly Func<TIntention, IAmAuthenticated<TIntention>> _Authenticate;

        private readonly Func<IAmAuthenticated<TIntention>,
                                IAmAuthorized<IAmAuthenticated<TIntention>>> _Authorize;

        private readonly Func<IAmAuthorized<IAmAuthenticated<TIntention>>,Task<TResult>> _Process;

        private readonly ITranslateToWebApi<TResult, TOuput> _AdaptOutput;

        public StandardPipeline(
            ILog logging,
            IAuthenticate<TIntention> authentication,
            IAuthorize<TIntention> authorization,
            IProcess<TIntention,Task<TResult>> process,
            ITranslateToWebApi<TResult, TOuput> adapter) 
        {
            _Logging = logging;
            _Authenticate = authentication.Authenticate;
            _Authorize = authorization.Authorize;
            _Process = process.Process;
            _AdaptOutput = adapter;
        }
        #endregion

        public Task<TOuput> Execute(TIntention intension)
        {
            return SetPipelineAs
                    .With( intension)
                    .Do(_Authenticate.Including( Logging))
                    .AndThen(_Authorize.Including(Logging).Retrying(up_to: 2.Times()))
                    .AndThen(_Process.Including(Logging).Retrying(up_to: 5.Times()))
                    .Finally(_AdaptOutput);
        }

        #region Logging overloads
        private void Logging( IAmAuthenticated<TIntention> authenticatedTransfer)
        {
            _Logging.Do("Intension was authenticated");
        }

        private void Logging(IAmAuthorized<IAmAuthenticated<TIntention>> authenticatedTransfer)
        {
            _Logging.Do("Intension was authorized");
        }
        private void Logging(TResult authenticatedTransfer)
        {
            _Logging.Do("Intension was processed");
        }
        #endregion
    }
}
