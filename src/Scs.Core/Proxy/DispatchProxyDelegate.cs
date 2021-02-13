using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Hik.Proxy
{
    public class DispatchProxyDelegate<TTarget>
    {
        private static Func<IInterceptor, TTarget> dispatchProxyfunc;

        public static Func<IInterceptor, TTarget> GetFunc()
        {
            if (dispatchProxyfunc == null)
            {
                var targetType = typeof(TTarget);
                var interceptorType = typeof(IInterceptor);
                //声明一个返回值变量
                var variable = Expression.Variable(targetType);
                var parm = Expression.Parameter(interceptorType);
                var callexp = Expression.Call(typeof(DispatchProxy), nameof(DispatchProxy.Create), new[] { targetType, typeof(ProxyGenerator) });
                var interceptorProperty = Expression.Property(Expression.Convert(variable, typeof(ProxyGenerator)), "interceptor");

                var assign1 = Expression.Assign(variable, callexp);//赋值操作
                var assign2 = Expression.Assign(interceptorProperty, parm);//给接口赋值

                //构造代码块
                var block = Expression.Block(new[] { variable }, parm, assign1, assign2, variable);
                dispatchProxyfunc = Expression.Lambda<Func<IInterceptor, TTarget>>(block, parm).Compile();
            }
            return dispatchProxyfunc;
        }
    }
}
