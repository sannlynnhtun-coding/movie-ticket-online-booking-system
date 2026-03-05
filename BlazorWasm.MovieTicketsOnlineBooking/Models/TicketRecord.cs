namespace BlazorWasm.MovieTicketsOnlineBooking.Models;

public class TicketRecord
{
    public Guid BookingVoucherDetailId { get; set; }
    public Guid BookingVoucherHeadId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public string MovieName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public int SeatId { get; set; }
    public string Seat { get; set; } = string.Empty;
    public int SeatPrice { get; set; }
    public int ShowDateId { get; set; }
    public DateTime ShowDate { get; set; }
    public DateTime BookingDate { get; set; }
}
