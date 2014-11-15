using System;
using System.Threading;
using Rikrop.Core.Framework.Exceptions;
using Rikrop.Core.Wpf;
using Rikrop.Core.Wpf.Unity;
using Microsoft.Practices.Unity;

namespace TestRegistration
{
    class Program
    {
        private static void Main(string[] args)
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<IDialogShower, DialogShower>(new InjectionConstructor("Test"));
            unityContainer.RegisterExceptionHandlers(o => o.CatchedExceptionHandler()
                                                           .OperationCanceledExceptionHandler()
                                                           .BusinessExceptionHandler(new ResolvedParameter<IDialogShower>())
                                                           .ErrorWorkspaceExceptionHandler()
                                                           .Custom<SecurityAccessDeniedExceptionHandler>(new InjectionConstructor("Test")));

            var handler = unityContainer.Resolve<IExceptionHandler>();

        }
    }

    public class SecurityAccessDeniedExceptionHandler : IExceptionHandler
    {
        private readonly string _dialogShower;
        private int _trigger;

        public SecurityAccessDeniedExceptionHandler(string dialogShower)
        {
            _dialogShower = dialogShower;
        }

        public bool Handle(Exception exception)
        {
            if (exception is BusinessException)
            {
                if (Interlocked.CompareExchange(ref _trigger, 1, 0) == 1) return true;


                Interlocked.Exchange(ref _trigger, 0);
                return true;
            }

            return false;
        }
    }
}
