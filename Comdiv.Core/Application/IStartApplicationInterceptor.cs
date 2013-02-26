namespace Comdiv.Application {
    public interface IStartApplicationInterceptor : IApplicationInterceptor {
        void OnStartApplication();
    }
}