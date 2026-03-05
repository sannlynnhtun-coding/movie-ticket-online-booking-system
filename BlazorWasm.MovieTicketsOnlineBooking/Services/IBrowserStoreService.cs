using BlazorWasm.MovieTicketsOnlineBooking.Models;
using BlazorWasm.MovieTicketsOnlineBooking.Models.DataModels;

namespace BlazorWasm.MovieTicketsOnlineBooking.Services;

public interface IBrowserStoreService
{
    Task InitAsync();

    Task SaveBookingDraftAsync(BookingModel draft);
    Task<List<BookingModel>> GetBookingDraftsByScopeAsync(string scopeKey);
    Task DeleteBookingDraftBySeatAsync(string scopeKey, int seatId);
    Task ClearBookingDraftsByScopeAsync(string scopeKey);
    Task ClearAllBookingDraftsAsync();

    Task SaveVoucherDetailAsync(BookingVoucherDetailDataModel detail);
    Task<List<BookingVoucherDetailDataModel>> GetVoucherDetailsAsync();

    Task SaveVoucherHeadAsync(BookingVoucherHeadDataModel head);
    Task<List<BookingVoucherHeadDataModel>> GetVoucherHeadsAsync();
}
