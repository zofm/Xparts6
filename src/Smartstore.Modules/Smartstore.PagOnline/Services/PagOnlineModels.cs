namespace Smartstore.PagOnline.Services
{
    #region Responses

    public class PagOnlineBaseResponse
    {
        public bool Success { get; set; }
        public string Rc { get; set; }
        public string RedirectUrl { get; set; }
        public string ErrorDescription { get; set; }
    }

    public class PagOnlineInitResponse : PagOnlineBaseResponse { }

    public class PagOnlineConfirmResponse : PagOnlineBaseResponse
    {
        public long TranID { get; set; }
    }

    public class PagOnlineCancelResponse : PagOnlineBaseResponse { }

    public class PagOnlineRefundResponse : PagOnlineBaseResponse { }

    public class PagOnlineVerifyResponse : PagOnlineBaseResponse
    {
        public long TranID { get; set; }
        public string EnrStatus { get; set; }
        public string AuthStatus { get; set; }
    }

    #endregion

    #region Requests

    public class PagOnlinePaymentBaseRequest
    {
        public int OrderId { get; set; }
        public int StoreId { get; set; }
    }

    public class PagOnlinePaymentInitRequest : PagOnlinePaymentBaseRequest
    {
        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string Language { get; set; }
    }

    public class PagOnlinePaymentVerifyRequest : PagOnlinePaymentBaseRequest { }

    public class PagOnlinePaymentConfirmRequest : PagOnlinePaymentBaseRequest
    {
        public long TranID { get; set; }
        public decimal Amount { get; set; }
    }

    public class PagOnlinePaymentCancelRequest : PagOnlinePaymentBaseRequest
    {
        public long TranID { get; set; }
        public decimal Amount { get; set; }
    }

    public class PagOnlinePaymentRefundRequest : PagOnlinePaymentBaseRequest
    {
        public long TranID { get; set; }
        public decimal Amount { get; set; }
    }

    #endregion
}
