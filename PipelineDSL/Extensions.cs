using System;
using System.Threading.Tasks;

namespace PipelineDSL
{
    public static class Extensions
    {
        public static Pipe<R> AndThen<T, R>(this Pipe<T> pipe, Func<T, R> filter)
        {
            return pipe.Do(filter);
        }

        public static async Task<Pipe<R>> AndThen<T, R>(this Pipe<T> pipe, Func<T, Task<R>> filter)
        {
            return await pipe.Do(filter);
        }

        public static async Task<Pipe<R>> AndThen<T, R>(this Task<Pipe<T>> pipe, Func<T, Task<R>> filter)
        {
            return await pipe.Do(filter); 
        }

        public static async Task<Pipe<R>> Do<T, R>(this Task<Pipe<T>> pipe, Func<T, Task<R>> filter)
        {
            var result = await pipe;
            return await result.Do(filter);
        }

        public static async Task<Pipe<R>> Do<T, R>(this Pipe<T> pipe, Func<T, Task<R>> filter)
        {
            switch (pipe._PipeValueType)
            {
                case PipeValueType.None:
                    return Pipe<R>.None();

                case PipeValueType.Exception:
                    return Pipe<R>.With((Exception)pipe);

                default:
                    try
                    {
                        var result = await filter(pipe);
                        return result != null ? Pipe<R>.With(result) : Pipe<R>.None();
                    }
                    catch (Exception e)
                    {
                        return Pipe<R>.With(e);
                    }
            }
        }

        public static Pipe<R> Do<T, R>(this Pipe<T> pipe, Func<T, R> filter)
        {
            switch (pipe._PipeValueType)
            {
                case PipeValueType.None:
                    return Pipe<R>.None();

                case PipeValueType.Exception:
                    return Pipe<R>.With((Exception)pipe);

                default:
                    try
                    {
                        var result = filter(pipe);
                        return result != null ? Pipe<R>.With(result) : Pipe<R>.None();
                    }
                    catch (Exception e)
                    {
                        return Pipe<R>.With(e);
                    }
            }
        }

        public static R Finally<T, R>(this Pipe<T> pipe, IAmTheFinalPipelineFilter<T, R> finalFilter)
        {
            switch (pipe._PipeValueType)
            {
                case PipeValueType.None:
                    return finalFilter.Do();

                case PipeValueType.Value:
                    return finalFilter.Do((T)pipe);

                default:
                    return finalFilter.Do((Exception)pipe);
            }
        }

        public async static Task<R> Finally<T, R>(this Task<Pipe<T>> pipe, IAmTheFinalPipelineFilter<T, R> finalFilter)
        {
            var result = await pipe;
            switch (result._PipeValueType)
            {
                case PipeValueType.None:
                    return finalFilter.Do();

                case PipeValueType.Value:
                    return finalFilter.Do((T)result);

                default:
                    return finalFilter.Do((Exception)result);
            }
        }

        public static Func<A, C> Compose<A, B, C>(this Func<B, C> outter, Func<A, B> inner)
        {
            return x => outter(inner(x));
        }

        public static Func<A, B> Including<A, B>(this Func<A, B> inner, Action<B> outter)
        {
            return x =>
            {
                var result = inner(x);
                outter(result);
                return result;
            };
        }

        public static Func<A, Task<B>> Including<A, B>(this Func<A, Task<B>> inner, Action<B> outter)
        {
            return async x =>
            {
                var result = await inner(x);
                outter(result);
                return result;
            };
        }
        public static Func<T, Task<R>> Retrying<T, R>(this Func<T, Task<R>> fun, NumberTimes up_to)
        {
            return async (x) =>
            {
                var counter = 0;
                while (true)
                {
                    try
                    {
                        var result = await fun(x);
                        return result;
                    }
                    catch (Exception e)
                    {
                        counter++;
                        if (counter >= up_to.Value)
                        {
                            throw e;
                        }
                    }
                }
            };
        }

        public static Func<T, R> Retrying<T, R>(this Func<T, R> fun, NumberTimes up_to)
        {
            return (x) =>
            {
                var counter = 0;
                while (true)
                {
                    try
                    {
                        return fun(x);
                    }
                    catch (Exception e)
                    {
                        counter++;
                        if (counter >= up_to.Value)
                        {
                            throw e;
                        }
                    }
                }
            };
        }

        public class NumberTimes
        {
            public int Value { get; private set; }

            public NumberTimes(int value)
            {
                Value = value;
            }
        }

        public static NumberTimes Times(this int times)
        {
            return new NumberTimes(times);
        }
    }
}
