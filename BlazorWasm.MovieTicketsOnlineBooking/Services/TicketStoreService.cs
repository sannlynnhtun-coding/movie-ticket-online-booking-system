using BlazorWasm.MovieTicketsOnlineBooking.Models;
using BlazorWasm.MovieTicketsOnlineBooking.Models.DataModels;
using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorWasm.MovieTicketsOnlineBooking.Services;

public class TicketStoreService : ITicketStoreService, IBrowserStoreService
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
            showDateId             = t.ShowDateId,
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
            ShowDateId             = el.TryGetProperty("showDateId", out var sid) ? sid.GetInt32() : 0,
            ShowDate               = DateTime.Parse(el.GetProperty("showDate").GetString() ?? DateTime.MinValue.ToString("o")),
            BookingDate            = DateTime.Parse(el.GetProperty("bookingDate").GetString() ?? DateTime.MinValue.ToString("o"))
        };
    }

    public async Task SaveBookingDraftAsync(BookingModel draft)
    {
        var payload = new
        {
            bookingId = draft.BookingId.ToString(),
            roomId = draft.RoomId,
            cinemaId = draft.CinemaId,
            movieId = draft.MovieId,
            showDateId = draft.ShowDateId,
            seatId = draft.SeatId,
            seatNo = draft.SeatNo,
            rowName = draft.RowName,
            seatType = draft.SeatType,
            seatPrice = draft.SeatPrice,
            showDate = draft.ShowDate.ToString("o"),
            scopeKey = BuildScopeKey(draft.RoomId, draft.CinemaId, draft.MovieId, draft.ShowDateId)
        };

        await _js.InvokeAsync<bool>("cinematix.saveBookingDraft", payload);
    }

    public async Task<List<BookingModel>> GetBookingDraftsByScopeAsync(string scopeKey)
    {
        try
        {
            var raw = await _js.InvokeAsync<JsonElement[]>("cinematix.getBookingDraftsByScope", scopeKey);
            return raw.Select(MapJsonToBookingModel).ToList();
        }
        catch
        {
            return new List<BookingModel>();
        }
    }

    public async Task DeleteBookingDraftBySeatAsync(string scopeKey, int seatId)
    {
        await _js.InvokeAsync<bool>("cinematix.deleteBookingDraftBySeat", scopeKey, seatId);
    }

    public async Task ClearBookingDraftsByScopeAsync(string scopeKey)
    {
        await _js.InvokeAsync<bool>("cinematix.clearBookingDraftsByScope", scopeKey);
    }

    public async Task ClearAllBookingDraftsAsync()
    {
        await _js.InvokeAsync<bool>("cinematix.clearAllBookingDrafts");
    }

    public async Task SaveVoucherDetailAsync(BookingVoucherDetailDataModel detail)
    {
        var payload = new
        {
            bookingVoucherDetailId = detail.BookingVoucherDetailId.ToString(),
            bookingVoucherHeadId = detail.BookingVoucherHeadId.ToString(),
            buildingName = detail.BuildingName,
            movieName = detail.MovieName,
            roomName = detail.RoomName,
            seatId = detail.SeatId,
            seat = detail.Seat,
            seatPrice = detail.SeatPrice,
            showDateId = detail.ShowDateId,
            showDate = detail.ShowDate.ToString("o"),
            bookingDate = detail.BookingDate.ToString("o")
        };
        await _js.InvokeAsync<bool>("cinematix.saveVoucherDetail", payload);
    }

    public async Task<List<BookingVoucherDetailDataModel>> GetVoucherDetailsAsync()
    {
        try
        {
            var raw = await _js.InvokeAsync<JsonElement[]>("cinematix.getVoucherDetails");
            return raw.Select(MapJsonToVoucherDetail).ToList();
        }
        catch
        {
            return new List<BookingVoucherDetailDataModel>();
        }
    }

    public async Task SaveVoucherHeadAsync(BookingVoucherHeadDataModel head)
    {
        var payload = new
        {
            bookingVoucherHeadId = head.BookingVoucherHeadId.ToString(),
            bookingVoucherNo = head.BookingVoucherNo.ToString(),
            bookingDate = head.BookingDate.ToString("o")
        };
        await _js.InvokeAsync<bool>("cinematix.saveVoucherHead", payload);
    }

    public async Task<List<BookingVoucherHeadDataModel>> GetVoucherHeadsAsync()
    {
        try
        {
            var raw = await _js.InvokeAsync<JsonElement[]>("cinematix.getVoucherHeads");
            return raw.Select(MapJsonToVoucherHead).ToList();
        }
        catch
        {
            return new List<BookingVoucherHeadDataModel>();
        }
    }

    private static string BuildScopeKey(int roomId, int cinemaId, int movieId, int showDateId)
        => $"{roomId}:{cinemaId}:{movieId}:{showDateId}";

    private static BookingModel MapJsonToBookingModel(JsonElement el)
    {
        return new BookingModel
        {
            BookingId = Guid.TryParse(el.GetProperty("bookingId").GetString(), out var id) ? id : Guid.Empty,
            RoomId = el.GetProperty("roomId").GetInt32(),
            CinemaId = el.GetProperty("cinemaId").GetInt32(),
            MovieId = el.GetProperty("movieId").GetInt32(),
            ShowDateId = el.GetProperty("showDateId").GetInt32(),
            SeatId = el.GetProperty("seatId").GetInt32(),
            SeatNo = el.GetProperty("seatNo").GetString() ?? "",
            RowName = el.GetProperty("rowName").GetString() ?? "",
            SeatType = el.GetProperty("seatType").GetString() ?? "",
            SeatPrice = el.GetProperty("seatPrice").GetInt32(),
            ShowDate = DateTime.Parse(el.GetProperty("showDate").GetString() ?? DateTime.MinValue.ToString("o"))
        };
    }

    private static BookingVoucherDetailDataModel MapJsonToVoucherDetail(JsonElement el)
    {
        return new BookingVoucherDetailDataModel
        {
            BookingVoucherDetailId = Guid.TryParse(el.GetProperty("bookingVoucherDetailId").GetString(), out var id) ? id : Guid.Empty,
            BookingVoucherHeadId = Guid.TryParse(el.GetProperty("bookingVoucherHeadId").GetString(), out var hid) ? hid : Guid.Empty,
            BuildingName = el.GetProperty("buildingName").GetString() ?? "",
            MovieName = el.GetProperty("movieName").GetString() ?? "",
            RoomName = el.GetProperty("roomName").GetString() ?? "",
            SeatId = el.GetProperty("seatId").GetInt32(),
            Seat = el.GetProperty("seat").GetString() ?? "",
            SeatPrice = el.GetProperty("seatPrice").GetInt32(),
            ShowDateId = el.TryGetProperty("showDateId", out var sid) ? sid.GetInt32() : 0,
            ShowDate = DateTime.Parse(el.GetProperty("showDate").GetString() ?? DateTime.MinValue.ToString("o")),
            BookingDate = DateTime.Parse(el.GetProperty("bookingDate").GetString() ?? DateTime.MinValue.ToString("o"))
        };
    }

    private static BookingVoucherHeadDataModel MapJsonToVoucherHead(JsonElement el)
    {
        return new BookingVoucherHeadDataModel
        {
            BookingVoucherHeadId = Guid.TryParse(el.GetProperty("bookingVoucherHeadId").GetString(), out var id) ? id : Guid.Empty,
            BookingVoucherNo = Guid.TryParse(el.GetProperty("bookingVoucherNo").GetString(), out var voucherNo) ? voucherNo : Guid.Empty,
            BookingDate = DateTime.Parse(el.GetProperty("bookingDate").GetString() ?? DateTime.MinValue.ToString("o"))
        };
    }
}
