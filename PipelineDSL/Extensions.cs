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

        public static async Task<Pipe<R>> AndThen<T, R>(this Task<Pipe<T>> asyncPipedTask, Func<T, R> filter)
        {
            var pipe = await asyncPipedTask;
            return pipe.Do(filter);
        }

        public static async Task<Pipe<R>> AndThenAsynchronously<T, R>(this Task<Pipe<T>> asyncPipedTask, Func<T, R> filter)
        {
            var pipe = await asyncPipedTask;
            return await Task<Pipe<R>>.Run(() => pipe.Do(filter)); 
        }

        public static async Task<Pipe<R>> Asynchronously<T, R>(this Pipe<T> pipe, Func<T, R> filter)
        {
            return await Task<Pipe<R>>.Run(() => pipe.Do(filter));
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
                        return Pipe<R>.With(filter(pipe)) ?? Pipe<R>.None();
                    }
                    catch (Exception e)
                    {
                        return Pipe<R>.With(e);
                    }
            }
        }

        public static async Task<R> AtLast<T, R>(this Task<Pipe<T>> asyncPipedTask, IAmTheFinalPipelineFilter<T, R> finalFilter)
        {
            var pipe = await asyncPipedTask;
            return pipe.AtLast(finalFilter);
        }

        public static R AtLast<T, R>(this Pipe<T> pipe, IAmTheFinalPipelineFilter<T, R> finalFilter)
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
