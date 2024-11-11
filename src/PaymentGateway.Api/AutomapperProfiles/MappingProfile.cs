using AutoMapper;
using PaymentGateway.Api.Entities;
using PaymentGateway.Api.Models;
using PaymentGateway.Models.Requests;
using PaymentGateway.Models.Responses;

namespace PaymentGateway.Api.AutomapperProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PostPaymentRequest, PostPaymentProviderRequest>()
            .ForMember(dest => dest.Cvv, opt => opt.MapFrom(src => src.Cvv.ToString()))
            .ForMember(dest => dest.card_number, opt => opt.MapFrom(src => src.CardNumberLastFour))
            .ForMember(dest => dest.expiry_date, opt => opt.MapFrom(src => src.ExpiryMonth.ToString("D2") + "/" + src.ExpiryYear));
        CreateMap<PostPaymentResponse, Payment>();
        CreateMap<PostPaymentRequest, PostPaymentResponse>()
            .ForMember(dest => dest.CardNumberLastFour, 
                opt => opt.MapFrom(new CardNumberMaskResolver<PostPaymentRequest, PostPaymentResponse, string>(src => MaskCardNumber(src.CardNumberLastFour))));
        CreateMap<Payment, GetPaymentResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CardNumberLastFour, opt => opt.MapFrom(new CardNumberMaskResolver<Payment, GetPaymentResponse, string>(src => MaskCardNumber(src.CardNumberLastFour))))
            .ForMember(dest => dest.ExpiryMonth, opt => opt.MapFrom(src => src.ExpiryMonth))
            .ForMember(dest => dest.ExpiryYear, opt => opt.MapFrom(src => src.ExpiryYear));
        CreateMap<GetPaymentResponse, PostPaymentResponse>();
        // CreateMap<(PostPaymentRequest,PostPaymentProviderResponse), PostPaymentResponse>()
        //     .ForMember(dest => dest.Status, 
        //     opt => opt.MapFrom(src => src.Item2.Authorized ? PaymentStatus.Authorized.ToString() : PaymentStatus.Declined.ToString()))
        //     .ForMember(dest => dest.AuthorizationCode, 
        //     opt => opt.MapFrom(src => src.Item2.Authorized ? src.Item2.AuthorizationCode : null))
        //     .ForMember(dest => dest.CardNumberLastFour, 
        //         opt => opt.MapFrom(new CardNumberMaskResolver<PostPaymentRequest, PostPaymentResponse, string>(src => MaskCardNumber(src.CardNumberLastFour))));

    }
    
    public static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
            return cardNumber;

        return new string('*', cardNumber.Length - 4) + cardNumber[^4..];
    }
}