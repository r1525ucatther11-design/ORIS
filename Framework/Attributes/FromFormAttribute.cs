using System;

namespace CustomHttpServer.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromFormAttribute : Attribute { }
}