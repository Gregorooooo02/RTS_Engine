using System;

namespace RTS_Engine.Exceptions;

public class HowDidWeGetHereException : InvalidOperationException
{
    public HowDidWeGetHereException() : base() {}
    public HowDidWeGetHereException(string message) : base(message){}
}