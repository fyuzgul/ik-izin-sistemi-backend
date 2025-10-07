# İzin Sistemi Backend

Bu proje .NET 8.0 kullanarak çok katmanlı mimari ile geliştirilmiş bir izin talep sistemidir.

## Proje Yapısı

- **LeaveManagement.Entity**: Veritabanı modelleri ve DbContext
- **LeaveManagement.DataAccess**: Repository pattern ve Unit of Work
- **LeaveManagement.Business**: İş mantığı ve servisler
- **LeaveManagement.API**: Web API controller'ları

## Gereksinimler

- .NET 8.0 SDK
- SQL Server (LocalDB dahil)
- Visual Studio 2022 veya VS Code

## Kurulum

1. Projeyi klonlayın
2. `LeaveManagement.API` klasörüne gidin
3. `dotnet restore` komutu ile paketleri yükleyin
4. `dotnet build` komutu ile projeyi derleyin
5. `dotnet run` komutu ile projeyi çalıştırın

## Veritabanı

Proje LocalDB kullanır ve otomatik olarak oluşturulur. Bağlantı string'i `appsettings.json` dosyasında bulunur.

## API Endpoints

### Leave Requests
- GET `/api/leaverequests` - Tüm izin taleplerini getir
- GET `/api/leaverequests/{id}` - Belirli izin talebini getir
- POST `/api/leaverequests` - Yeni izin talebi oluştur
- PUT `/api/leaverequests/{id}/approve` - İzin talebini onayla/reddet
- DELETE `/api/leaverequests/{id}` - İzin talebini sil

### Employees
- GET `/api/employees` - Tüm çalışanları getir
- GET `/api/employees/{id}` - Belirli çalışanı getir
- POST `/api/employees` - Yeni çalışan oluştur
- PUT `/api/employees/{id}` - Çalışan bilgilerini güncelle

### Leave Types
- GET `/api/leavetypes` - Tüm izin türlerini getir
- POST `/api/leavetypes` - Yeni izin türü oluştur
- PUT `/api/leavetypes/{id}` - İzin türünü güncelle

### Departments
- GET `/api/departments` - Tüm departmanları getir
- POST `/api/departments` - Yeni departman oluştur
- PUT `/api/departments/{id}` - Departman bilgilerini güncelle

## İzin Talebi Akışı

1. Çalışan izin talebi oluşturur
2. Departman yöneticisi talebi onaylar/reddeder
3. İK müdürü talebi onaylar/reddeder
4. Onaylanan talepler çalışanın izin bakiyesinden düşer

## Test Verileri

Proje ilk çalıştırıldığında otomatik olarak test verileri oluşturulur:
- 5 farklı izin türü
- 4 departman
