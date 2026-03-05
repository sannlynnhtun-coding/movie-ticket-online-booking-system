# Movie Ticket Online Booking System

A modern, responsive web application for browsing movies and booking tickets, built with **Blazor WebAssembly**. This project demonstrates a complete end-to-end booking flow, from movie discovery to seat selection and ticket generation.

## 🚀 Key Features

- **🎬 Movie Discovery**: Browse current and upcoming movies with detailed information.
- **📅 Cinema & Showtimes**: Select preferred cinemas and specific show dates/times.
- **💺 Interactive Seat Selection**: Visual room layout with real-time seat availability (simulated).
- **🎫 Ticket Management**: View booking vouchers and history.
- **💾 Local Data Persistence**: Uses browser storage (IndexedDB/LocalStorage) for managing bookings and session data.
- **🎨 Dynamic Theming**: Support for multiple UI themes and professional aesthetics.
- **📱 Fully Responsive**: Optimized for both desktop and mobile viewing.

## 🛠️ Tech Stack

- **Frontend**: [Blazor WebAssembly](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor) (C#/.NET 6)
- **UI/UX**: HTML5, Vanilla CSS3, Razor Components
- **Data Storage**: Browser LocalStorage & IndexedDB (Service-managed)
- **Dependency Injection**: Scoped and Singleton services for state management.
- **Serialization**: Newtonsoft.Json for data handling.

## 📂 Project Structure

```text
├── BlazorWasm.MovieTicketsOnlineBooking
│   ├── Models/           # Data Transfer Objects & View Models
│   ├── Pages/            # Core UI Components & Page Routing
│   │   ├── PageMoviesCard.razor   # Movie listing
│   │   ├── PageRoomSeat.razor     # Seat selection logic
│   │   └── PageBookingVoucher.razor # Ticket output
│   ├── Services/         # Business logic & Persistence services
│   ├── Shared/           # Layouts & Reusable components
│   └── wwwroot/          # Static assets (CSS, Images, JS)
└── BlazorWasm.MovieTicketsOnlineBooking.sln
```

## 🏁 Getting Started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later.

### Installation & Run

1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-username/movie-ticket-online-booking-system.git
   cd movie-ticket-online-booking-system
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Run the application**:
   ```bash
   dotnet run --project BlazorWasm.MovieTicketsOnlineBooking
   ```

4. **Access the app**: Open your browser and navigate to `https://localhost:7111` (or the port specified in your terminal).

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.