namespace Orchard.Tokens {
    public interface ITokenProvider : IDependency {
        void Describe(DescribeContext context);
        void Evaluate(EvaluateContext context);
    }
}