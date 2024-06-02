using System;

namespace RTS_Engine.Exceptions;

public class NoTerrainException : InvalidOperationException
{
    public NoTerrainException() : base() {}
    public NoTerrainException(string message) : base(message){}
}