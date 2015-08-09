using System;

namespace PipelineDSL
{
    public interface IAmAuthenticated<T> {}
    public interface IAmAuthorized<T> {}
    public interface IAmProcessed<T> { }
    public interface IAuthenticate<T> 
    {
        IAmAuthenticated<T> Authenticate(T intention);
    }

    public interface IAuthorize<T>
    {
        IAmAuthorized<IAmAuthenticated<T>> Authorize(IAmAuthenticated<T> authenticatedIntention);
    }
    public interface IProcess<T>
    {
        IAmProcessed<IAmAuthorized<IAmAuthenticated<T>>> Process(IAmAuthorized<IAmAuthenticated<T>> authorizedIntention);
    }
    public interface ILog<T, R>
    {
        R Do(T intention);
    }

    public interface IAmTheFinalPipelineFilter<T, R>
    {
        R Do(Exception exception);
        R Do(T value);
        R Do();
    } 

    public interface ITranslateToWebApi<T, R> : IAmTheFinalPipelineFilter<T, R>{}
}
