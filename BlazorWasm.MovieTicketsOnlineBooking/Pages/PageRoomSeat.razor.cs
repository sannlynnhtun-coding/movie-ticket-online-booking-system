using BlazorWasm.MovieTicketsOnlineBooking.Models;
using BlazorWasm.MovieTicketsOnlineBooking.Models.ViewModels;
using BlazorWasm.MovieTicketsOnlineBooking.Pages.Dialog;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorWasm.MovieTicketsOnlineBooking.Pages;

public partial class PageRoomSeat
{
    [Parameter] public int RoomId { get; set; }
    [Parameter] public int CinemaId { get; set; }
    [Parameter] public int MovieId { get; set; }

    private bool _isLoading = true;
    private List<BookingVoucherDetailViewModel> _voucherDetailLst { get; set; } = new();

    private RoomDetailModel? _roomDetail;
    private MovieScheduleViewModel? _selectedShow;
    private DateTime ShowDate => _selectedShow?.ShowDateTime ?? default;
    private int ShowId => _selectedShow?.ShowId ?? 0;
    private List<BookingModel> _bookingData { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _roomDetail = await _dbService.GetRoomDetail(RoomId, CinemaId, MovieId);

        if (_roomDetail?.ShowDate is { Count: > 0 })
        {
            _selectedShow = _roomDetail.ShowDate[0];
        }

        await ReloadSeatStateAsync();
        _isLoading = false;
    }

    private async Task HandleSeatClick(RoomSeatViewModel seat)
    {
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

        var result = _bookingData.FirstOrDefault(v => v.SeatId == model.SeatId);
        if (result is not null) return;

        await _dbService.SetBookingList(model, RoomId, CinemaId, MovieId, ShowId, ShowDate);
        _bookingData = await _dbService.GetBookingList(RoomId, CinemaId, MovieId, ShowId) ?? new();
    }

    private async Task SelectedShowDate(MovieScheduleViewModel show)
    {
        _selectedShow = show;
        await ReloadSeatStateAsync();
    }

    private async Task ReloadSeatStateAsync()
    {
        if (_selectedShow is null)
        {
            _bookingData = new();
            _voucherDetailLst = new();
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
        var dialog = await DialogService.ShowAsync<DeleteBookingSeat>();
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            if (seatId == default || _selectedShow is null) return;
            await _dbService.DeleteBookingSeat(seatId, RoomId, CinemaId, MovieId, ShowId);
            _bookingData = await _dbService.GetBookingList(RoomId, CinemaId, MovieId, ShowId) ?? new();
        }
    }
}
