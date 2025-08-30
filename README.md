# Order Management API

E-ticaret platformu için geliştirilmiş kapsamlı sipariş yönetim sistemi API'si.

## Özellikler

- ✅ Yeni sipariş oluşturma (stok kontrolü ile)
- ✅ Siparişleri listeleme
- ✅ Sipariş detaylarını getirme
- ✅ Sipariş silme
- ✅ Sipariş durumu güncelleme
- ✅ Ürün yönetimi
- ✅ Gelişmiş stok yönetimi
- ✅ Düşük stok uyarıları
- ✅ Concurrent sipariş desteği
- ✅ Transaction yönetimi
- ✅ Kapsamlı hata yönetimi

## API Endpoints

### Siparişler
- `POST /api/orders` - Yeni sipariş oluştur
- `GET /api/orders` - Tüm siparişleri listele
- `GET /api/orders/{id}` - Sipariş detayını getir
- `DELETE /api/orders/{id}` - Sipariş sil
- `PATCH /api/orders/{id}/status` - Sipariş durumunu güncelle

### Ürünler
- `GET /api/products` - Tüm ürünleri listele
- `GET /api/products/{id}` - Ürün detayını getir
- `POST /api/products` - Yeni ürün oluştur
- `PATCH /api/products/{id}/stock` - Ürün stokunu güncelle
- `DELETE /api/products/{id}` - Ürün sil

### Stok Yönetimi
- `GET /api/stock/low-stock-alerts` - Düşük stok uyarıları
- `GET /api/stock/check-availability/{productId}` - Stok durumu kontrolü

## Kurulum

### Gereksinimler
- .NET 8.0 SDK
- SQL Server (LocalDB veya tam sürüm)

### Adımlar
1. Repository'yi klonlayın
2. `appsettings.json` dosyasında connection string'i güncelleyin
3. Aşağıdaki komutları çalıştırın:

\`\`\`bash
dotnet restore
dotnet build
dotnet run
\`\`\`

### Docker ile Çalıştırma
\`\`\`bash
docker-compose up -d
\`\`\`

## Swagger UI
API çalıştırıldıktan sonra Swagger UI'ya şu adresten erişebilirsiniz:
- Development: `https://localhost:5001` veya `http://localhost:5000`

## Örnek Kullanım

### Yeni Sipariş Oluşturma
\`\`\`json
POST /api/orders
{
  "customerName": "Ahmet Yılmaz",
  "customerEmail": "ahmet@example.com",
  "shippingAddress": "İstanbul, Türkiye",
  "items": [
    {
      "productId": 1,
      "quantity": 2
    },
    {
      "productId": 2,
      "quantity": 1
    }
  ]
}
\`\`\`

### Sipariş Listesi
\`\`\`json
GET /api/orders
\`\`\`

## Teknolojiler
- ASP.NET Core 8.0
- Entity Framework Core
- SQL Server
- Swagger/OpenAPI
- Docker
