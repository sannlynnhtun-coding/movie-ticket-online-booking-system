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
    Task<List<MovieScheduleViewModel>?> GetMovieSchedule();
    Task<List<MovieScheduleViewModel>> GetRoomShowSchedules(int roomId, int cinemaId, int movieId, DateTime? showDate = null);
    Task<List<DateTime>> GetMovieAvailableDates(int movieId);
    Task<List<CinemaRoomModel>?> GetCinemaAndRoom(int movieId);
    Task<List<CinemaRoomModel>?> GetCinemaAndRoom(int movieId, DateTime? showDate);
    Task<RoomDetailModel> GetRoomDetail(int roomId, int cinemaId, int movieId);
    Task<MovieResponseModel?> GetMovieListByPagination(int pageNo, int pageSize);
    Task SetBookingList(RoomSeatViewModel model, int roomId, int cinemaId, int movieId, int showId, DateTime showDateTime);
    Task<List<BookingModel>?> GetBookingList(int roomId, int cinemaId, int movieId, int showId);

    Task<MovieSearchModel> SearchMovie(string title, int pageNo = 1,
        int pageSize = 3);

    Task<MovieViewModel> GetMovieByRoomId(int roomId);
    Task SetBookingVoucher(int roomId, int cinemaId, int movieId, int showId);
    Task<List<BookingVoucherDetailViewModel>> GetBookingVoucherDetail();
    Task<List<BookingVoucherHeadDataModel>> GetBookingVoucherHead();
    Task DeleteBookingSeat(int seatId, int roomId, int cinemaId, int movieId, int showId);

    Task<CinemaRoomPaginationModel?> GetCinemaRoomPagination(int movieId,
        int pageNo = 1, int pageSize = 5);
    Task<CinemaRoomPaginationModel?> GetCinemaRoomPagination(int movieId, DateTime? showDate,
        int pageNo = 1, int pageSize = 5);

    Task ClearBookingList();
    Task ClearBookingList(int roomId, int cinemaId, int movieId, int showId);
}
