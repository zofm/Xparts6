namespace Smartstore.PagOnline.Services
{
    public interface IPagOnlineService
    {
        Task<PagOnlineInitResponse> InitAsync(PagOnlinePaymentInitRequest request);

        Task<PagOnlineVerifyResponse> VerifyAsync(PagOnlinePaymentVerifyRequest request);

        Task<PagOnlineConfirmResponse> ConfirmAsync(PagOnlinePaymentConfirmRequest request);

        Task<PagOnlineCancelResponse> CancelAsync(PagOnlinePaymentCancelRequest request);

        Task<PagOnlineRefundResponse> RefundAsync(PagOnlinePaymentRefundRequest request);
    }
}
