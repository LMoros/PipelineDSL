using System;

namespace PipelineDSL
{
    public enum PipeValueType
    {
        None,
        Value,
        Exception,
    }

    public class SetPipelineAs
    {
        public static Pipe<T> With<T>(T value)
        {
            return Pipe<T>.With(value);
        }
    }
    public class Pipe<T>
    {
        internal PipeValueType _PipeValueType { get; private set; }

        protected Pipe(PipeValueType pipeValueType) { }

        public static Pipe<T> With(T value)
        {
            return new ValuePipe<T>(value);
        }

        public static Pipe<T> With(Exception exception)
        {
            return new ExceptionPipe<T>(exception);
        }

        public static Pipe<T> None()
        {
            return new EmptyPipe<T>();
        }

        public static implicit operator T (Pipe<T> pipe) 
        {
            return ((ValuePipe<T>)pipe)._Value;
        }

        public static implicit operator Exception(Pipe<T> pipe)
        {
            return ((ExceptionPipe<T>)pipe)._Exception;
        }
    }

    public class EmptyPipe<T> : Pipe<T>
    {
        internal EmptyPipe()
            : base(PipeValueType.None) { }
    }

    public class ExceptionPipe<T> : Pipe<T>
    {
        internal Exception _Exception { get; private set; }

        internal ExceptionPipe(Exception exception)
            : base(PipeValueType.Exception)
        {
            _Exception = exception;
        }
    }

    public class ValuePipe<T> : Pipe<T>
    {
        internal T _Value { get; private set; }

        internal ValuePipe(T value)
            : base(PipeValueType.Value)
        {
            _Value = value;
        }
    }
}
