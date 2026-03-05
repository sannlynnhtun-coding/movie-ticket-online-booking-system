using BlazorWasm.MovieTicketsOnlineBooking.Models;

namespace BlazorWasm.MovieTicketsOnlineBooking.Services;

public interface ITicketStoreService
{
    Task InitAsync();
    Task SaveTicketsAsync(List<TicketRecord> tickets);
    Task<List<TicketRecord>> GetAllTicketsAsync();
    Task ClearAllTicketsAsync();
    Task DeleteTicketAsync(Guid bookingVoucherDetailId);
}
