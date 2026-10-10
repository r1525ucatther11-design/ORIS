using System;

namespace CustomHttpServer.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpControllerAttribute : Attribute
    {
        public string Name { get; }

        public HttpControllerAttribute(string name)
        {
            Name = name;
        }
    }
}