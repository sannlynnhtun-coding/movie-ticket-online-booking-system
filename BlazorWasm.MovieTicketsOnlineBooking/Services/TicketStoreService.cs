using BlazorWasm.MovieTicketsOnlineBooking.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorWasm.MovieTicketsOnlineBooking.Services;

public class TicketStoreService : ITicketStoreService
{
    private readonly IJSRuntime _js;

    public TicketStoreService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitAsync()
    {
        await _js.InvokeAsync<bool>("cinematix.initDb");
    }

    public async Task SaveTicketsAsync(List<TicketRecord> tickets)
    {
        // Convert to JS-friendly objects via JSON round-trip
        var jsonTickets = tickets.Select(t => new
        {
            bookingVoucherDetailId = t.BookingVoucherDetailId.ToString(),
            bookingVoucherHeadId   = t.BookingVoucherHeadId.ToString(),
            buildingName           = t.BuildingName,
            movieName              = t.MovieName,
            roomName               = t.RoomName,
            seatId                 = t.SeatId,
            seat                   = t.Seat,
            seatPrice              = t.SeatPrice,
            showDate               = t.ShowDate.ToString("o"),
            bookingDate            = t.BookingDate.ToString("o")
        }).ToArray();

        await _js.InvokeAsync<int>("cinematix.saveTickets", new object[] { jsonTickets });
    }

    public async Task<List<TicketRecord>> GetAllTicketsAsync()
    {
        try
        {
            var raw = await _js.InvokeAsync<JsonElement[]>("cinematix.getAllTickets");
            return raw.Select(MapJsonToTicketRecord).ToList();
        }
        catch
        {
            return new List<TicketRecord>();
        }
    }

    public async Task ClearAllTicketsAsync()
    {
        await _js.InvokeAsync<bool>("cinematix.clearTickets");
    }

    public async Task DeleteTicketAsync(Guid bookingVoucherDetailId)
    {
        await _js.InvokeAsync<bool>("cinematix.deleteTicket", bookingVoucherDetailId.ToString());
    }

    private TicketRecord MapJsonToTicketRecord(JsonElement el)
    {
        return new TicketRecord
        {
            BookingVoucherDetailId = Guid.TryParse(el.GetProperty("bookingVoucherDetailId").GetString(), out var id) ? id : Guid.Empty,
            BookingVoucherHeadId   = Guid.TryParse(el.GetProperty("bookingVoucherHeadId").GetString(), out var hid) ? hid : Guid.Empty,
            BuildingName           = el.GetProperty("buildingName").GetString() ?? "",
            MovieName              = el.GetProperty("movieName").GetString() ?? "",
            RoomName               = el.GetProperty("roomName").GetString() ?? "",
            SeatId                 = el.GetProperty("seatId").GetInt32(),
            Seat                   = el.GetProperty("seat").GetString() ?? "",
            SeatPrice              = el.GetProperty("seatPrice").GetInt32(),
            ShowDate               = DateTime.Parse(el.GetProperty("showDate").GetString() ?? DateTime.MinValue.ToString("o")),
            BookingDate            = DateTime.Parse(el.GetProperty("bookingDate").GetString() ?? DateTime.MinValue.ToString("o"))
        };
    }
}
