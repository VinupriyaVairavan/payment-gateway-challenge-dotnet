using AutoMapper;

namespace PaymentGateway.Api.AutomapperProfiles;

public class CardNumberMaskResolver<TSource, TDestination, TMember> : IValueResolver<TSource, TDestination, TMember>
{
    private readonly Func<TSource, TMember> _resolverFunc;

    public CardNumberMaskResolver(Func<TSource, TMember> resolverFunc)
    {
        _resolverFunc = resolverFunc;
    }

    public TMember Resolve(TSource source, TDestination destination, TMember destMember, ResolutionContext context)
    {
        return _resolverFunc(source);
    }
}