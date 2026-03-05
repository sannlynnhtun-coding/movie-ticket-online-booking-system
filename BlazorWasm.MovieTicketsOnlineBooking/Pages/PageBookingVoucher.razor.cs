using BlazorWasm.MovieTicketsOnlineBooking.Models;
using BlazorWasm.MovieTicketsOnlineBooking.Models.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BlazorWasm.MovieTicketsOnlineBooking.Pages;

public partial class PageBookingVoucher
{
    private List<BookingVoucherDetailViewModel>? _voucherDetailLst { get; set; }
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        var voucherDetailLst = await _dbService.GetBookingVoucherDetail();
        var voucherHeadLst = await _dbService.GetBookingVoucherHead();
        var voucherHead = voucherHeadLst.MaxBy(x => x.BookingDate);

        if (voucherHead != null)
        {
            _voucherDetailLst = voucherDetailLst
                .Where(v => v.BookingVoucherHeadId == voucherHead.BookingVoucherHeadId)
                .ToList();

            // Save tickets to IndexedDB for offline persistence
            var ticketsToSave = _voucherDetailLst.Select(v => new TicketRecord
            {
                BookingVoucherDetailId = v.BookingVoucherDetailId,
                BookingVoucherHeadId = v.BookingVoucherHeadId,
                BuildingName = v.BuildingName,
                MovieName = v.MovieName,
                RoomName = v.RoomName,
                SeatId = v.SeatId,
                Seat = v.Seat,
                SeatPrice = v.SeatPrice,
                ShowDate = v.ShowDate,
                BookingDate = v.BookingDate
            }).ToList();

            if (ticketsToSave.Count > 0)
            {
                await TicketStore.SaveTicketsAsync(ticketsToSave);
            }
        }
        _isLoading = false;
    }
}