using BlazorWasm.MovieTicketsOnlineBooking.Models;
using BlazorWasm.MovieTicketsOnlineBooking.Models.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BlazorWasm.MovieTicketsOnlineBooking.Pages;

public partial class PageRoomSeat
{
    [Parameter] public int RoomId { get; set; }
    [Parameter] public int CinemaId { get; set; }
    [Parameter] public int MovieId { get; set; }
    [Parameter] public int ShowId { get; set; }

    private bool _isLoading = true;
    private List<BookingVoucherDetailViewModel> _voucherDetailLst { get; set; } = new();
    private List<BookedTicketRow> _bookedTicketRows { get; set; } = new();

    private RoomDetailModel? _roomDetail;
    private MovieScheduleViewModel? _selectedShow;
    private DateTime ShowDate => _selectedShow?.ShowDateTime ?? default;
    private List<BookingModel> _bookingData { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _roomDetail = await _dbService.GetRoomDetail(RoomId, CinemaId, MovieId);

        if (_roomDetail?.ShowDate is { Count: > 0 })
        {
            _selectedShow = _roomDetail.ShowDate.FirstOrDefault(x => x.ShowId == ShowId) ?? _roomDetail.ShowDate[0];
            ShowId = _selectedShow.ShowId;
        }

        await ReloadSeatStateAsync();
        _isLoading = false;
    }

    private async Task HandleSeatClick(RoomSeatViewModel seat)
    {
        if (_selectedShow is null) return;

        var pairSeat = FindCouplePairSeat(seat);
        if (pairSeat is not null)
        {
            if (IsSeatBooked(seat.SeatId) || IsSeatBooked(pairSeat.SeatId))
            {
                return;
            }

            var seatSelected = _bookingData.Any(b => b.SeatId == seat.SeatId);
            var pairSelected = _bookingData.Any(b => b.SeatId == pairSeat.SeatId);

            if (seatSelected || pairSelected)
            {
                await DeleteSeatWithoutDialog(seat.SeatId);
                await DeleteSeatWithoutDialog(pairSeat.SeatId);
            }
            else
            {
                await AddSeatIfNotSelected(seat);
                await AddSeatIfNotSelected(pairSeat);
            }

            _bookingData = await _dbService.GetBookingList(RoomId, CinemaId, MovieId, ShowId) ?? new();
            return;
        }

        var existing = _bookingData.FirstOrDefault(b => b.SeatId == seat.SeatId);
        if (existing is not null)
        {
            await DeleteBookingSeat(seat.SeatId);
        }
        else
        {
            await ToBookingList(seat);
        }
    }

    private async Task ToBookingList(RoomSeatViewModel model)
    {
        if (_selectedShow is null) return;
        await AddSeatIfNotSelected(model);
        _bookingData = await _dbService.GetBookingList(RoomId, CinemaId, MovieId, ShowId) ?? new();
    }

    private async Task SelectedShowDate(MovieScheduleViewModel show)
    {
        _selectedShow = show;
        ShowId = show.ShowId;
        await ReloadSeatStateAsync();
    }

    private async Task ReloadSeatStateAsync()
    {
        if (_selectedShow is null)
        {
            _bookingData = new();
            _voucherDetailLst = new();
            _bookedTicketRows = new();
            return;
        }

        _bookingData = await _dbService.GetBookingList(RoomId, CinemaId, MovieId, ShowId) ?? new();
        await LoadBookedTicketsForDate();
    }

    private async Task LoadBookedTicketsForDate()
    {
        var serverVouchers = await _dbService.GetBookingVoucherDetail() ?? new();
        var localTickets = await TicketStore.GetAllTicketsAsync() ?? new();
        var seatIdsInRoom = _roomDetail?.RoomSeatData?.Select(s => s.SeatId).ToHashSet() ?? new HashSet<int>();

        var scopedServer = serverVouchers.Where(v =>
            v.ShowId == ShowId &&
            seatIdsInRoom.Contains(v.SeatId));

        var matchedLocal = localTickets
            .Where(t => t.ShowId == ShowId && seatIdsInRoom.Contains(t.SeatId))
            .Select(t => new BookingVoucherDetailViewModel
            {
                BookingVoucherDetailId = t.BookingVoucherDetailId,
                SeatId = t.SeatId,
                Seat = t.Seat,
                ShowId = t.ShowId,
                ShowDate = t.ShowDate
            });

        _voucherDetailLst = scopedServer
            .Concat(matchedLocal)
            .GroupBy(x => x.SeatId)
            .Select(g => g.First())
            .ToList();

        _bookedTicketRows = _voucherDetailLst
            .OrderBy(x => x.Seat)
            .Select(x => new BookedTicketRow
            {
                Seat = x.Seat,
                ShowDate = x.ShowDate,
                TicketNo = x.BookingVoucherDetailId != Guid.Empty ? x.BookingVoucherDetailId.ToString()[..8] : "-"
            })
            .ToList();
    }

    private async Task SetBookingVoucher()
    {
        if (_selectedShow is null) return;
        await _dbService.SetBookingVoucher(RoomId, CinemaId, MovieId, ShowId);
        NavManager.NavigateTo("/voucher");
    }

    private void BackToCinemaRoom()
    {
        NavManager.NavigateTo($"/cinema/{MovieId}");
    }

    private async Task DeleteBookingSeat(int seatId)
    {
        if (seatId == default || _selectedShow is null) return;
        await _dbService.DeleteBookingSeat(seatId, RoomId, CinemaId, MovieId, ShowId);
        _bookingData = await _dbService.GetBookingList(RoomId, CinemaId, MovieId, ShowId) ?? new();
    }

    private async Task AddSeatIfNotSelected(RoomSeatViewModel model)
    {
        var alreadySelected = _bookingData.Any(v => v.SeatId == model.SeatId);
        if (alreadySelected) return;
        if (IsSeatBooked(model.SeatId)) return;
        await _dbService.SetBookingList(model, RoomId, CinemaId, MovieId, ShowId, ShowDate);
    }

    private async Task DeleteSeatWithoutDialog(int seatId)
    {
        if (_selectedShow is null) return;
        await _dbService.DeleteBookingSeat(seatId, RoomId, CinemaId, MovieId, ShowId);
    }

    private bool IsSeatBooked(int seatId)
    {
        return _voucherDetailLst.Any(v => v.SeatId == seatId);
    }

    private bool IsCouplePairBooked(RoomSeatViewModel seat)
    {
        var pairSeat = FindCouplePairSeat(seat);
        return pairSeat is not null && IsSeatBooked(pairSeat.SeatId);
    }

    private RoomSeatViewModel? FindCouplePairSeat(RoomSeatViewModel seat)
    {
        if (_roomDetail?.RoomSeatData is null) return null;
        if (!string.Equals(seat.SeatType, "couple", StringComparison.OrdinalIgnoreCase)) return null;
        if (!int.TryParse(seat.SeatNo, out var seatNo)) return null;

        var pairNo = seatNo % 2 == 0 ? seatNo - 1 : seatNo + 1;
        return _roomDetail.RoomSeatData.FirstOrDefault(s =>
            s.RoomId == seat.RoomId &&
            s.RowName == seat.RowName &&
            string.Equals(s.SeatType, "couple", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(s.SeatNo, out var n) &&
            n == pairNo);
    }

    private sealed class BookedTicketRow
    {
        public string Seat { get; set; } = "";
        public DateTime ShowDate { get; set; }
        public string TicketNo { get; set; } = "";
    }
}
