using System;

namespace VsBuild.MSBuildRunner
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs() : base()
        {

        }

        public EventArgs(T data) : this()
        {
            Data = data;
        }

        public T Data { get; set; }
    }
}
