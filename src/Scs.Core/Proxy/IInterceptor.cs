using System.Reflection;

namespace Hik.Proxy
{
    public interface IInterceptor
    {
        object Intercept(MethodInfo method, object[] parameters);
    }
}
