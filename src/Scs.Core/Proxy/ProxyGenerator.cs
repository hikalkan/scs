using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hik.Proxy
{
    public class ProxyGenerator : DispatchProxy
    {
        private IInterceptor interceptor { get; set; }

        /// <summary>
        /// 创建代理实例
        /// </summary>
        /// <param name="targetType">所要代理的接口类型</param>
        /// <param name="interceptor">拦截器</param>
        /// <returns>代理实例</returns>
        public static object Create(Type targetType, IInterceptor interceptor)
        {
            var interceptorType = interceptor.GetType();
            //声明一个返回值变量
            var variable = Expression.Variable(targetType);
            var callexp = Expression.Call(typeof(DispatchProxy), nameof(DispatchProxy.Create), new[] { targetType, typeof(ProxyGenerator) });
            var interceptorProperty = Expression.Property(Expression.Convert(variable, typeof(ProxyGenerator)), nameof(interceptor));
            var assign1 = Expression.Assign(variable, callexp);//赋值操作
            var assign2 = Expression.Assign(interceptorProperty, Expression.Constant(interceptor));//给接口赋值

            //构造代码块
            var block = Expression.Block(new[] { variable }, assign1, assign2, variable);
            return Expression.Lambda<Func<object>>(block).Compile()();//建议缓存
        }

        /// <summary>
        /// 创建代理实例
        /// </summary>
        /// <param name="targetType">所要代理的接口类型</param>
        /// <param name="interceptorType">拦截器类型</param>
        /// <param name="parameters">拦截器构造函数参数值</param>
        /// <returns>代理实例</returns>
        public static object Create(Type targetType, Type interceptorType, params object[] parameters)
        {
            //声明一个返回值变量
            var variable = Expression.Variable(targetType);

            var callexp = Expression.Call(typeof(DispatchProxy), nameof(DispatchProxy.Create), new[] { targetType, typeof(ProxyGenerator) });
            var ctorParams = parameters.Select(x => x.GetType()).ToArray();
            var paramsExp = parameters.Select(x => Expression.Constant(x));
            var newExp = Expression.New(interceptorType.GetConstructor(ctorParams), paramsExp);
            var interceptorProperty = Expression.Property(Expression.Convert(variable, typeof(ProxyGenerator)), nameof(interceptor));

            var assign1 = Expression.Assign(variable, callexp);//赋值操作
            var assign2 = Expression.Assign(interceptorProperty, newExp);//给接口赋值

            //构造代码块
            var block = Expression.Block(new[] { variable }, assign1, assign2, variable);
            return Expression.Lambda<Func<object>>(block).Compile()();//建议缓存
        }


        /// <summary>
        /// 创建代理实例 TTarget:所要代理的接口类型 TInterceptor:拦截器类型
        /// </summary>
        /// <param name="parameters">拦截器构造函数参数值</param>
        /// <returns>代理实例</returns>
        public static TTarget Create<TTarget, TInterceptor>(params object[] parameters) where TInterceptor : IInterceptor
        {
            var targetType = typeof(TTarget);
            var interceptorType = typeof(TInterceptor);
            //声明一个返回值变量
            var variable = Expression.Variable(targetType);

            var callexp = Expression.Call(typeof(DispatchProxy), nameof(DispatchProxy.Create), new[] { targetType, typeof(ProxyGenerator) });
            var ctorParams = parameters.Select(x => x.GetType()).ToArray();
            var paramsExp = parameters.Select(x => Expression.Constant(x));
            var newExp = Expression.New(interceptorType.GetConstructor(ctorParams), paramsExp);
            var interceptorProperty = Expression.Property(Expression.Convert(variable, typeof(ProxyGenerator)), nameof(interceptor));

            var assign1 = Expression.Assign(variable, callexp);//赋值操作
            var assign2 = Expression.Assign(interceptorProperty, newExp);//给接口赋值

            //构造代码块
            var block = Expression.Block(new[] { variable }, assign1, assign2, variable);
            return Expression.Lambda<Func<TTarget>>(block).Compile()();//建议缓存
        }

        /// <summary>
        /// 创建代理实例 TTarget:所要代理的接口类型 TInterceptor:拦截器类型
        /// </summary>
        /// <param name="parameters">IInterceptor接口</param>
        /// <returns>代理实例</returns>
        public static TTarget Create<TTarget>(IInterceptor iinterceptor)
        {
            return DispatchProxyDelegate<TTarget>.GetFunc()(iinterceptor);
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            return interceptor.Intercept(targetMethod, args);
        }
    }
}
