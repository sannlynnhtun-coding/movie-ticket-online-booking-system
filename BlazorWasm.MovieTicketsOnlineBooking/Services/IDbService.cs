using BlazorWasm.MovieTicketsOnlineBooking.Models;
using BlazorWasm.MovieTicketsOnlineBooking.Models.DataModels;
using BlazorWasm.MovieTicketsOnlineBooking.Models.ViewModels;

namespace BlazorWasm.MovieTicketsOnlineBooking.Services;

public interface IDbService
{
    Task<List<MovieViewModel>?> GetMovieList();
    Task<List<CinemaViewModel>?> GetCinemaList();
    Task<List<CinemaRoomViewModel>?> GetCinemaRoom();
    Task<List<MovieShowDateTimeViewModel>?> GetMovieShowDateTime();
    Task<List<CinemaRoomModel>?> GetCinemaAndRoom(int movieId);
    Task<RoomDetailModel> GetRoomDetail(int roomId, int cinemaId, int movieId);
    Task<MovieResponseModel?> GetMovieListByPagination(int pageNo, int pageSize);
    Task SetBookingList(RoomSeatViewModel model, int roomId, int cinemaId, int movieId, int showDateId, DateTime showDateTime);
    Task<List<BookingModel>?> GetBookingList(int roomId, int cinemaId, int movieId, int showDateId);

    Task<MovieSearchModel> SearchMovie(string title, int pageNo = 1,
        int pageSize = 3);

    Task<MovieViewModel> GetMovieByRoomId(int roomId);
    Task SetBookingVoucher(int roomId, int cinemaId, int movieId, int showDateId);
    Task<List<BookingVoucherDetailViewModel>> GetBookingVoucherDetail();
    Task<List<BookingVoucherHeadDataModel>> GetBookingVoucherHead();
    Task DeleteBookingSeat(int seatId, int roomId, int cinemaId, int movieId, int showDateId);

    Task<CinemaRoomPaginationModel?> GetCinemaRoomPagination(int movieId,
        int pageNo = 1, int pageSize = 5);

    Task ClearBookingList();
    Task ClearBookingList(int roomId, int cinemaId, int movieId, int showDateId);
}
