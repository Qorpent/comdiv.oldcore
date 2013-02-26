using Comdiv.Rules;

namespace Comdiv.Rules.Activation{
    public interface IFriendshipMatcher
    {
        bool IsFirend(IRule friendCandidate, IRule main);
    }
}