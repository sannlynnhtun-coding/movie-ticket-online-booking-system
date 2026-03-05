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

    private RoomDetailModel? _roomDetail = null;
    private SeatNoModel? Seat = new();
    private DateTime ShowDate { get; set; }
    private List<BookingModel> _bookingData { get; set; } = new();
    private int seatId = 0;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _roomDetail = await _dbService.GetRoomDetail(RoomId, CinemaId, MovieId);
        
        if (_roomDetail?.ShowDate != null && _roomDetail.ShowDate.Count > 0)
        {
            ShowDate = _roomDetail.ShowDate[0].ShowDateTime;
        }

        await LoadBookedTicketsForDate();
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
        var result = _bookingData.FirstOrDefault(v => v.SeatId == model.SeatId);
        if (result is not null) return;

        seatId = model.SeatId;
        var data = model;
        if (ShowDate != default(DateTime))
        {
            await _dbService.SetBookingList(data, ShowDate);
            _bookingData = await _dbService.GetBookingList();
        }
    }

    private async Task SelectedShowDate(DateTime date)
    {
        ShowDate = date;
        await LoadBookedTicketsForDate();
    }

    private async Task LoadBookedTicketsForDate()
    {
        // 1. Get from "backend API" (mock DB)
        var serverVouchers = await _dbService.GetBookingVoucherDetail() ?? new();
        
        // 2. Supplement with any offline-persisted ones for this specific show date/movie
        var localTickets = await TicketStore.GetAllTicketsAsync() ?? new();
        
        // Combine them locally to mark seats as booked (red)
        var matchedLocal = localTickets
            .Where(t => t.ShowDate == ShowDate && (_roomDetail?.RoomSeatData?.Any(s => s.SeatId == t.SeatId) ?? false))
            .Select(t => new BookingVoucherDetailViewModel 
            { 
                SeatId = t.SeatId, 
                Seat = t.Seat 
            });

        _voucherDetailLst = serverVouchers.Concat(matchedLocal).GroupBy(x => x.SeatId).Select(g => g.First()).ToList();
    }

    private void SetBookingVoucher()
    {
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
            if (seatId == default) return;
            await _dbService.DeleteBookingSeat(seatId);
            _bookingData = await _dbService.GetBookingList();
        }
    }
}